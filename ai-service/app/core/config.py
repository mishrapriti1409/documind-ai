"""Pydantic Settings environment configuration."""
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    ENVIRONMENT: str = "development"

    # Database
    DATABASE_URL: str = "postgresql+asyncpg://documind:documind_password@localhost:5432/documind"

    # OpenAI Settings
    OPENAI_API_KEY: str = ""
    USE_AZURE_OPENAI: bool = False
    AZURE_OPENAI_ENDPOINT: str = ""
    AZURE_OPENAI_API_KEY: str = ""
    AZURE_OPENAI_DEPLOYMENT: str = "gpt-4o"
    AZURE_EMBEDDING_DEPLOYMENT: str = "text-embedding-3-small"

    # Internal Security
    INTERNAL_API_KEY: str = "dev_internal_secret_key_32_characters"

    # AI Pipeline Models & Hyperparameters
    LLM_MODEL: str = "gpt-4o"
    EMBEDDING_MODEL: str = "text-embedding-3-small"
    CHUNK_SIZE: int = 800
    CHUNK_OVERLAP: int = 120
    MAX_CHUNKS_PER_QUERY: int = 8
    MAX_FILE_SIZE_MB: int = 50


settings = Settings()

