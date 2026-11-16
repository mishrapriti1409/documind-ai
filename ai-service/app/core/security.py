"""Internal API key authentication dependency using constant-time comparison."""
import hmac
from fastapi import HTTPException, Security, status
from fastapi.security import APIKeyHeader

from app.core.config import settings

_api_key_header = APIKeyHeader(name="X-Internal-Api-Key", auto_error=True)


def verify_internal_api_key(api_key: str = Security(_api_key_header)) -> None:
    """
    Validate the internal secret key passed by ASP.NET Core backend.
    Uses hmac.compare_digest to prevent timing attack side-channels.
    """
    expected = settings.INTERNAL_API_KEY.encode()
    provided = api_key.encode()

    if not hmac.compare_digest(expected, provided):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Forbidden: Invalid or missing internal API key.",
        )

