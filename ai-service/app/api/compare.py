from fastapi import APIRouter, Depends
from app.core.security import verify_internal_api_key
from app.models.requests import CompareRequest
from app.models.responses import ComparisonResult
from app.services.comparator import compare_documents

router = APIRouter()

@router.post("/documents/compare", response_model=ComparisonResult, tags=["Comparison"])
async def compare_endpoint(
    request: CompareRequest,
    _: None = Depends(verify_internal_api_key),
):
    return await compare_documents(
        document_id_a=request.document_id_a,
        document_id_b=request.document_id_b,
        organization_id=request.organization_id,
    )

