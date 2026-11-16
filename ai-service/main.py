"""DocuMind AI Microservice — FastAPI Application Entry Point."""
from contextlib import asynccontextmanager
import structlog
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api.router import api_router
from app.core.config import settings
from app.core.logging import setup_logging
from app.core.exceptions import register_exception_handlers
from app.db.connection import init_db

setup_logging()
logger = structlog.get_logger()


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info(
        "DocuMind AI microservice starting up",
        environment=settings.ENVIRONMENT,
        llm_model=settings.LLM_MODEL,
        embedding_model=settings.EMBEDDING_MODEL,
    )
    try:
        await init_db()
        logger.info("Database connection validated")
    except Exception as e:
        logger.warning("Database connectivity check failed on startup (will retry on requests)", error=str(e))
    yield
    logger.info("DocuMind AI microservice shutting down")


app = FastAPI(
    title="DocuMind AI Microservice",
    description="Asynchronous document ingestion, semantic chunking, pgvector search, RAG, and summarization.",
    version="1.0.0",
    lifespan=lifespan,
    docs_url="/docs" if settings.ENVIRONMENT != "production" else None,
    redoc_url=None,
)

# CORS Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4200", "http://localhost:5000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Exception handlers
register_exception_handlers(app)

# Include main versioned API router
app.include_router(api_router, prefix="/api/v1")

