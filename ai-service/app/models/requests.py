"""Pydantic request payload definitions."""
from __future__ import annotations
from typing import Literal
from uuid import UUID
from pydantic import BaseModel, Field


class ProcessDocumentRequest(BaseModel):
    document_id: UUID
    organization_id: UUID
    file_path: str = Field(..., description="Absolute path on shared volume or local filesystem to the uploaded file.")
    content_type: str = Field(..., description="MIME content type.")


class QueryRequest(BaseModel):
    question: str = Field(..., min_length=1, max_length=2000)
    organization_id: UUID
    document_ids: list[UUID] | None = Field(None, description="Optional document filter. If omitted, searches all organization docs.")
    user_id: UUID
    conversation_id: UUID
    conversation_history: list[dict] | None = None


class SummarizeRequest(BaseModel):
    document_id: UUID
    organization_id: UUID
    summary_type: Literal[
        "short", "detailed", "executive", "key_points",
        "faqs", "dates", "monetary", "entities", "obligations"
    ] = "short"


class CompareRequest(BaseModel):
    document_id_a: UUID
    document_id_b: UUID
    organization_id: UUID

