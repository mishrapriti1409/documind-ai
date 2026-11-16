"""PDF parsing and text extraction with page metadata using PyMuPDF (fitz)."""
from __future__ import annotations
import fitz
import structlog

logger = structlog.get_logger()


def extract_pdf(file_path: str) -> list[dict]:
    """
    Extract text on a per-page basis from a PDF file.
    Returns: List of dicts with {"page_number": int, "text": str}.
    """
    pages = []
    try:
        doc = fitz.open(file_path)
        for page_idx in range(len(doc)):
            page = doc.load_page(page_idx)
            text = page.get_text("text")
            if text and text.strip():
                pages.append({
                    "page_number": page_idx + 1,
                    "text": text.strip()
                })
        doc.close()
    except Exception as e:
        logger.error("Failed to extract text from PDF", file_path=file_path, error=str(e))
        raise

    logger.info("Extracted PDF pages", total_pages=len(pages), file_path=file_path)
    return pages

