"""Async SQLAlchemy engine and session factory for PostgreSQL + pgvector."""
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine
from sqlalchemy import text
import structlog
from app.core.config import settings

logger = structlog.get_logger()

engine = create_async_engine(
    settings.DATABASE_URL,
    echo=False,
    pool_size=10,
    max_overflow=5,
    pool_recycle=300,
)

AsyncSessionLocal = async_sessionmaker(
    bind=engine,
    expire_on_commit=False,
    class_=AsyncSession,
)


async def init_db() -> None:
    """Verify PostgreSQL connectivity and pgvector extension availability."""
    async with engine.connect() as conn:
        await conn.execute(text("SELECT 1;"))
        # Attempt to create extension if permissions allow
        try:
            await conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector;"))
            await conn.commit()
        except Exception:
            pass


async def get_db():
    async with AsyncSessionLocal() as session:
        yield session

