from fastapi import APIRouter
from app.models.responses import HealthResponse

router = APIRouter()

@router.get("/health", response_model=HealthResponse, tags=["Health"])
async def health_check():
    return HealthResponse()

