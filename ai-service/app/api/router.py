from fastapi import APIRouter
from app.api import health, process, query, summarize, compare

api_router = APIRouter()

api_router.include_router(health.router)
api_router.include_router(process.router)
api_router.include_router(query.router)
api_router.include_router(summarize.router)
api_router.include_router(compare.router)

