"""Text normalization and cleaning utility."""
import re


def clean_text(text: str) -> str:
    """Strip binary control characters, normalize whitespace runs and multi-newlines."""
    if not text:
        return ""

    # Remove non-printable control characters except newline and tab
    text = re.sub(r"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]", "", text)

    # Normalize horizontal whitespace
    text = re.sub(r"[ \t]+", " ", text)

    # Normalize 3+ newlines to double newline
    text = re.sub(r"\n{3,}", "\n\n", text)

    return text.strip()

