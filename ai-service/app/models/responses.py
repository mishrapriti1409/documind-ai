"""Pydantic response payload definitions."""
from __future__ import annotations
from uuid import UUID
from pydantic import BaseModel


class SourceReference(BaseModel):
    chunk_id: UUID
    document_id: UUID
    page_number: int
    excerpt: str
    relevance_score: float


class QueryResult(BaseModel):
    answer: str
    sources: list[SourceReference]
    tokens_used: int
    cost_estimate: float
    insufficient_evidence: bool = False


class ProcessingResult(BaseModel):
    document_id: UUID
    chunks_created: int
    pages_processed: int
    status: str


class SummaryResult(BaseModel):
    document_id: UUID
    summary_type: str
    content: str | dict | list
    tokens_used: int


class ComparisonSection(BaseModel):
    description: str
    page_references: list[int]


class ComparisonResult(BaseModel):
    document_id_a: UUID
    document_id_b: UUID
    added_sections: list[ComparisonSection]
    removed_sections: list[ComparisonSection]
    modified_sections: list[ComparisonSection]
    ai_summary: str
    tokens_used: int


class HealthResponse(BaseModel):
    status: str = "healthy"
    version: str = "1.0.0"

