-- ==============================================================================
-- DocuMind AI - Supabase Database Initialization Script
-- Run this in the Supabase Dashboard -> SQL Editor
-- ==============================================================================

-- 1. Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- 2. Verify pgvector is enabled
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_extension WHERE extname = 'vector'
    ) THEN
        RAISE EXCEPTION 'pgvector extension failed to initialize.';
    END IF;
END $$;

-- Notice:
-- Entity Framework Core 9 in DocuMind.API automatically runs EnsureCreated / migrations
-- upon first launch with ConnectionStrings:DefaultConnection.
--
-- Connection String format for Supabase:
-- Transaction Pooler (port 6543) or Direct Connection (port 5432):
-- Host=aws-0-[region].pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.[project-ref];Password=[your-password];SSL Mode=Require;Trust Server Certificate=true;
