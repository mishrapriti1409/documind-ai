from fastapi import APIRouter, Depends
from app.core.security import verify_internal_api_key
from app.models.requests import SummarizeRequest
from app.models.responses import SummaryResult
from app.services.summarizer import summarize_document

router = APIRouter()

@router.post("/documents/summarize", response_model=SummaryResult, tags=["Summarization"])
async def summarize_endpoint(
    request: SummarizeRequest,
    _: None = Depends(verify_internal_api_key),
):
    return await summarize_document(
        document_id=request.document_id,
        organization_id=request.organization_id,
        summary_type=request.summary_type,
    )

