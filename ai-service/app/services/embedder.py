"""Vector embedding generation with batching and Azure OpenAI support."""
from __future__ import annotations
import structlog
from openai import AsyncOpenAI, AsyncAzureOpenAI
from app.core.config import settings

logger = structlog.get_logger()
_BATCH_SIZE = 100


def get_openai_client():
    if settings.USE_AZURE_OPENAI:
        return AsyncAzureOpenAI(
            azure_endpoint=settings.AZURE_OPENAI_ENDPOINT,
            api_key=settings.AZURE_OPENAI_API_KEY,
            api_version="2024-08-01-preview",
        )
    return AsyncOpenAI(api_key=settings.OPENAI_API_KEY or "sk-dummy-key")


async def embed_texts(texts: list[str]) -> list[list[float]]:
    """
    Generate 1536-dimensional embeddings for a list of strings in batches.
    Returns: List of float arrays.
    """
    if not texts:
        return []

    client = get_openai_client()
    model = settings.AZURE_EMBEDDING_DEPLOYMENT if settings.USE_AZURE_OPENAI else settings.EMBEDDING_MODEL
    embeddings: list[list[float]] = []

    for i in range(0, len(texts), _BATCH_SIZE):
        batch = texts[i:i + _BATCH_SIZE]
        logger.info("Embedding batch", start_index=i, batch_size=len(batch), model=model)
        try:
            response = await client.embeddings.create(input=batch, model=model)
            batch_embs = [item.embedding for item in sorted(response.data, key=lambda x: x.index)]
            embeddings.extend(batch_embs)
        except Exception as e:
            logger.error("Embedding generation failed", error=str(e), batch_start=i)
            # If no API key configured in dev, generate deterministic pseudo-embeddings for graceful testing
            if not settings.OPENAI_API_KEY or "dummy" in settings.OPENAI_API_KEY:
                logger.warning("Falling back to dev pseudo-embeddings (configure OPENAI_API_KEY for real vectors)")
                for t in batch:
                    val = (hash(t) % 1000) / 1000.0
                    embeddings.append([val] * 1536)
            else:
                raise

    return embeddings

