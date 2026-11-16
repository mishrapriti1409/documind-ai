"""Semantic vector retrieval service."""
from __future__ import annotations
from uuid import UUID
import structlog

from app.core.config import settings
from app.services.embedder import embed_texts
from app.db.queries import search_chunks

logger = structlog.get_logger()


async def retrieve_relevant_chunks(
    question: str,
    organization_id: UUID,
    document_ids: list[UUID] | None,
) -> list[dict]:
    """
    Generate question embedding and query pgvector for top-K cosine similar chunks.
    """
    logger.info("Retrieving relevant chunks", question_len=len(question), org_id=str(organization_id))

    embeddings = await embed_texts([question])
    query_emb = embeddings[0]

    try:
        chunks = await search_chunks(
            query_emb,
            organization_id,
            document_ids,
            top_k=settings.MAX_CHUNKS_PER_QUERY,
        )
        return chunks
    except Exception as e:
        logger.warning("Vector search query failed (Postgres may not be running locally)", error=str(e))
        # Return empty list in development if DB not connected
        return []

