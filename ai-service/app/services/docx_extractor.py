"""DOCX text extraction using python-docx."""
from __future__ import annotations
from docx import Document
import structlog

logger = structlog.get_logger()


def extract_docx(file_path: str) -> list[dict]:
    """
    Extract paragraphs from a DOCX document, grouping into simulated pages (~40 paragraphs/page).
    Returns: List of dicts with {"page_number": int, "text": str}.
    """
    try:
        doc = Document(file_path)
        paragraphs = [p.text.strip() for p in doc.paragraphs if p.text and p.text.strip()]

        pages = []
        page_size = 40
        for i in range(0, len(paragraphs), page_size):
            chunk_paras = paragraphs[i:i + page_size]
            pages.append({
                "page_number": (i // page_size) + 1,
                "text": "\n".join(chunk_paras)
            })

        logger.info("Extracted DOCX pages", total_pages=len(pages), file_path=file_path)
        return pages
    except Exception as e:
        logger.error("Failed to extract text from DOCX", file_path=file_path, error=str(e))
        raise

