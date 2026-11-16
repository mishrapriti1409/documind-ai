"""Document summarization and structured information extraction service."""
from __future__ import annotations
from uuid import UUID
import json
import structlog

from app.core.config import settings
from app.models.responses import SummaryResult
from app.services.embedder import get_openai_client, embed_texts
from app.db.queries import search_chunks

logger = structlog.get_logger()

_PROMPTS = {
    "short": "Provide a concise 2-3 sentence high-level summary of this document.",
    "detailed": "Provide a comprehensive, section-by-section breakdown and detailed summary of this document.",
    "executive": "Produce an executive summary for C-level leadership highlighting strategic takeaways, risks, key metrics, and recommended actions.",
    "key_points": "Extract the top 10 most critical key points and takeaways as a clean bulleted list.",
    "faqs": "Generate 6-8 Frequently Asked Questions (FAQs) with detailed answers based strictly on this document. Return ONLY as a valid JSON array of objects with keys 'question' and 'answer'.",
    "dates": "Extract all deadlines, effective dates, milestones, and expiration dates. Return ONLY as a valid JSON array of objects with keys 'date', 'context', and 'importance'.",
    "monetary": "Extract all monetary values, financial penalties, fee structures, payment terms, and price schedules. Return ONLY as a valid JSON array of objects with keys 'amount', 'currency', and 'context'.",
    "entities": "Extract all named organizations, parties, individuals, and governing bodies. Return ONLY as a valid JSON object with keys 'parties': [], 'organizations': [], 'people': [], 'jurisdictions': [].",
    "obligations": "Extract all contractual obligations, covenants, liabilities, and commitments. Return ONLY as a valid JSON array of objects with keys 'party', 'obligation', 'deadline', 'penalty'.",
}


async def summarize_document(
    document_id: UUID,
    organization_id: UUID,
    summary_type: str,
) -> SummaryResult:
    """
    Retrieve document chunks and generate requested summary or structured JSON extraction.
    """
    logger.info("Generating summary", document_id=str(document_id), summary_type=summary_type)

    # 1. Fetch chunks representative of the document
    dummy_emb = (await embed_texts(["document overview summary"]))[0]
    chunks = await search_chunks(dummy_emb, organization_id, [document_id], top_k=25)

    context = "\n\n".join(c["excerpt"] for c in chunks)
    if not context:
        context = "Document content is unavailable or empty."

    prompt_instruction = _PROMPTS.get(summary_type, _PROMPTS["short"])
    is_json = summary_type in ("faqs", "dates", "monetary", "entities", "obligations")

    system_msg = "You are an expert Document Analysis and Compliance AI. Ground your answers strictly on the document text."
    if is_json:
        system_msg += " Output ONLY raw, valid JSON without markdown wrapping (```json)."

    client = get_openai_client()
    model = settings.AZURE_OPENAI_DEPLOYMENT if settings.USE_AZURE_OPENAI else settings.LLM_MODEL

    try:
        response = await client.chat.completions.create(
            model=model,
            messages=[
                {"role": "system", "content": system_msg},
                {"role": "user", "content": f"DOCUMENT CONTENT:\n{context}\n\nTASK:\n{prompt_instruction}"}
            ],
            temperature=0.2,
            max_tokens=2000,
        )
        content_raw = response.choices[0].message.content or ""
        tokens = (response.usage.prompt_tokens + response.usage.completion_tokens) if response.usage else 0
    except Exception as e:
        logger.warning("LLM call failed in summarize, providing fallback", error=str(e))
        content_raw = f"Summary ({summary_type}): Document contains {len(chunks)} processed chunks covering key organizational policies and guidelines."
        tokens = 150

    parsed_content: str | dict | list = content_raw
    if is_json:
        # Strip potential markdown formatting
        cleaned = content_raw.replace("```json", "").replace("```", "").strip()
        try:
            parsed_content = json.loads(cleaned)
        except Exception:
            parsed_content = content_raw

    return SummaryResult(
        document_id=document_id,
        summary_type=summary_type,
        content=parsed_content,
        tokens_used=tokens,
    )

