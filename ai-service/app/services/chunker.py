"""Semantic chunking using a token-based sliding window with tiktoken."""
from __future__ import annotations
import tiktoken
import structlog
from app.core.config import settings

logger = structlog.get_logger()
_encoding = tiktoken.get_encoding("cl100k_base")


def count_tokens(text: str) -> int:
    return len(_encoding.encode(text))


def chunk_pages(
    pages: list[dict],
    chunk_size: int | None = None,
    overlap: int | None = None,
) -> list[dict]:
    """
    Split per-page text content into overlapping semantic chunks.
    Preserves page numbers on every chunk for citation tracking.

    Returns: List of dicts with:
        {"content": str, "page_number": int, "chunk_index": int, "token_count": int}
    """
    c_size = chunk_size or settings.CHUNK_SIZE
    c_overlap = overlap or settings.CHUNK_OVERLAP

    chunks = []
    chunk_idx = 0

    for page in pages:
        page_num = page["page_number"]
        text = page["text"]

        tokens = _encoding.encode(text)
        if not tokens:
            continue

        start = 0
        while start < len(tokens):
            end = min(start + c_size, len(tokens))
            chunk_tokens = tokens[start:end]
            chunk_text = _encoding.decode(chunk_tokens).strip()

            if chunk_text:
                chunks.append({
                    "content": chunk_text,
                    "page_number": page_num,
                    "chunk_index": chunk_idx,
                    "token_count": len(chunk_tokens),
                })
                chunk_idx += 1

            if end >= len(tokens):
                break

            start += c_size - c_overlap

    logger.info("Chunking completed", total_chunks=len(chunks), chunk_size=c_size, overlap=c_overlap)
    return chunks

