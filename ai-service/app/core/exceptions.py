"""Custom domain exceptions and centralized FastAPI exception handlers."""
import structlog
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

logger = structlog.get_logger()


class DocumentProcessingError(Exception):
    def __init__(self, document_id: str, detail: str):
        self.document_id = document_id
        self.detail = detail
        super().__init__(detail)


class UnsupportedFileTypeError(Exception):
    def __init__(self, content_type: str):
        self.content_type = content_type
        super().__init__(f"Unsupported file type: {content_type}")


class InsufficientEvidenceError(Exception):
    """Raised when retrieved vector chunks do not contain enough grounding evidence."""
    pass


def register_exception_handlers(app: FastAPI) -> None:
    @app.exception_handler(DocumentProcessingError)
    async def document_processing_handler(request: Request, exc: DocumentProcessingError):
        logger.error("Document ingestion pipeline failure", document_id=exc.document_id, detail=exc.detail)
        return JSONResponse(
            status_code=422,
            content={"error": "processing_failed", "document_id": exc.document_id, "detail": exc.detail},
        )

    @app.exception_handler(UnsupportedFileTypeError)
    async def unsupported_file_handler(request: Request, exc: UnsupportedFileTypeError):
        return JSONResponse(
            status_code=415,
            content={"error": "unsupported_media_type", "content_type": exc.content_type},
        )

    @app.exception_handler(Exception)
    async def generic_exception_handler(request: Request, exc: Exception):
        logger.error("Unhandled microservice exception", exc_info=exc, path=request.url.path)
        return JSONResponse(
            status_code=500,
            content={"error": "internal_server_error", "detail": "An internal AI service error occurred."},
        )

