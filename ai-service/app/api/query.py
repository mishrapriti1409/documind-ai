from fastapi import APIRouter, Depends
from app.core.security import verify_internal_api_key
from app.models.requests import QueryRequest
from app.models.responses import QueryResult
from app.services.rag_engine import answer_question

router = APIRouter()

@router.post("/query/rag", response_model=QueryResult, tags=["RAG"])
async def query_rag_endpoint(
    request: QueryRequest,
    _: None = Depends(verify_internal_api_key),
):
    return await answer_question(
        question=request.question,
        organization_id=request.organization_id,
        document_ids=request.document_ids,
        conversation_history=request.conversation_history,
    )

