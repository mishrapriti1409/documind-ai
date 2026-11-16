"""XLSX cell data extraction using openpyxl."""
from __future__ import annotations
from openpyxl import load_workbook
import structlog

logger = structlog.get_logger()


def extract_xlsx(file_path: str) -> list[dict]:
    """
    Extract cell values across all sheets in an Excel workbook.
    Each sheet is represented as a distinct 'page'.
    Returns: List of dicts with {"page_number": int, "text": str}.
    """
    try:
        wb = load_workbook(file_path, read_only=True, data_only=True)
        pages = []

        for idx, sheet_name in enumerate(wb.sheetnames):
            ws = wb[sheet_name]
            lines = [f"=== Sheet: {sheet_name} ==="]

            for row in ws.iter_rows(values_only=True):
                non_empty = [str(cell) for cell in row if cell is not None and str(cell).strip()]
                if non_empty:
                    lines.append("\t".join(non_empty))

            if len(lines) > 1:
                pages.append({
                    "page_number": idx + 1,
                    "text": "\n".join(lines)
                })

        wb.close()
        logger.info("Extracted XLSX sheets", total_sheets=len(pages), file_path=file_path)
        return pages
    except Exception as e:
        logger.error("Failed to extract text from XLSX", file_path=file_path, error=str(e))
        raise

