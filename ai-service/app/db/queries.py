"""Raw PostgreSQL + pgvector queries for chunk upsert and cosine similarity search."""
from __future__ import annotations
from uuid import UUID
import sqlalchemy as sa
import structlog
from app.db.connection import AsyncSessionLocal

logger = structlog.get_logger()


async def upsert_document_chunks(
    document_id: UUID,
    organization_id: UUID,
    chunks: list[dict],
) -> None:
    """
    Atomically replace all chunks for a document with new chunks & embeddings.
    """
    async with AsyncSessionLocal() as session:
        # Delete prior chunks for idempotent re-processing
        await session.execute(
            sa.text('DELETE FROM "DocumentChunks" WHERE "DocumentId" = :doc_id'),
            {"doc_id": str(document_id)},
        )

        for chunk in chunks:
            embedding_vector_str = "[" + ",".join(f"{x:.6f}" for x in chunk["embedding"]) + "]"
            await session.execute(
                sa.text("""
                    INSERT INTO "DocumentChunks"
                        ("Id", "DocumentId", "OrganizationId", "ChunkIndex", "Content", "PageNumber", "TokenCount", "Embedding", "CreatedAt", "UpdatedAt")
                    VALUES
                        (gen_random_uuid(), :doc_id, :org_id, :chunk_idx, :content, :page_num, :tokens, :embedding::vector, NOW(), NOW())
                """),
                {
                    "doc_id": str(document_id),
                    "org_id": str(organization_id),
                    "chunk_idx": chunk["chunk_index"],
                    "content": chunk["content"],
                    "page_num": chunk["page_number"],
                    "tokens": chunk["token_count"],
                    "embedding": embedding_vector_str,
                },
            )
        await session.commit()
        logger.info("Upserted document chunks", document_id=str(document_id), count=len(chunks))


async def search_chunks(
    query_embedding: list[float],
    organization_id: UUID,
    document_ids: list[UUID] | None,
    top_k: int = 8,
) -> list[dict]:
    """
    Perform pgvector cosine similarity search (<=> operator) strictly isolated to the tenant organization.
    """
    embedding_str = "[" + ",".join(f"{x:.6f}" for x in query_embedding) + "]"

    async with AsyncSessionLocal() as session:
        params: dict = {
            "org_id": str(organization_id),
            "embedding": embedding_str,
            "top_k": top_k,
        }

        doc_filter = ""
        if document_ids:
            doc_ids_str = [str(d) for d in document_ids]
            doc_filter = 'AND "DocumentId" = ANY(:doc_ids::uuid[])'
            params["doc_ids"] = doc_ids_str

        query_sql = f"""
            SELECT
                "Id" as chunk_id,
                "DocumentId" as document_id,
                "PageNumber" as page_number,
                "Content" as excerpt,
                1 - ("Embedding" <=> :embedding::vector) as relevance_score
            FROM "DocumentChunks"
            WHERE "OrganizationId" = :org_id
            {doc_filter}
            ORDER BY "Embedding" <=> :embedding::vector
            LIMIT :top_k
        """

        result = await session.execute(sa.text(query_sql), params)
        rows = result.mappings().all()

        return [
            {
                "chunk_id": row["chunk_id"],
                "document_id": row["document_id"],
                "page_number": row["page_number"],
                "excerpt": row["excerpt"],
                "relevance_score": float(row["relevance_score"]),
            }
            for row in rows
        ]

