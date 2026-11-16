from fastapi import APIRouter, Depends
from app.core.security import verify_internal_api_key
from app.models.requests import ProcessDocumentRequest
from app.models.responses import ProcessingResult
from app.services.document_processor import process_document

router = APIRouter()

@router.post("/documents/process", response_model=ProcessingResult, tags=["Ingestion"])
async def process_document_endpoint(
    request: ProcessDocumentRequest,
    _: None = Depends(verify_internal_api_key),
):
    return await process_document(
        document_id=request.document_id,
        organization_id=request.organization_id,
        file_path=request.file_path,
        content_type=request.content_type,
    )

