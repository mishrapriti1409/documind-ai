# DocuMind AI - Enterprise Document Intelligence & RAG Platform

DocuMind AI is a production-grade, multi-tenant enterprise document intelligence platform built with **ASP.NET Core 9 (Clean Architecture)**, a high-performance **Python 3.12+ FastAPI AI microservice**, and an **Angular 18 standalone SPA**. 

It enables organizations to securely ingest documents (PDF, DOCX, XLSX), perform semantic sliding-window chunking, generate high-dimensional vector embeddings, execute semantic similarity search with PostgreSQL `pgvector`, engage in context-grounded conversational RAG with page-level citations and prompt injection defense, generate 9 types of structured document summaries, and execute semantic cross-document comparisons.

---

## Architecture Overview

DocuMind AI adheres strictly to Clean Architecture and microservices principles:

```
┌────────────────────────────────────────────────────────────────────────┐
│                        Angular 18 Frontend SPA                         │
│   (Design Tokens, JWT Auth, Upload Hub, RAG Chat, Compare, Analytics)  │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ HTTP / REST (Bearer JWT)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                       ASP.NET Core 9 Web API                           │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐  │
│  │   API Layer      │  │ Application DTOs │  │  Domain Entities     │  │
│  │ (Controllers, MW)│  │ & Use Cases      │  │  (Tenant, RBAC, Doc) │  │
│  └─────────┬────────┘  └────────┬─────────┘  └──────────┬───────────┘  │
│            └────────────────────┼───────────────────────┘              │
│                                 ▼                                      │
│                  Infrastructure (EF Core 9, Npgsql)                    │
└───────────────┬────────────────────────────────────────┬───────────────┘
                │ Internal HTTP (API Key)                │ Npgsql (pgvector)
                ▼                                        ▼
┌──────────────────────────────────────┐  ┌──────────────────────────────┐
│       Python AI Microservice         │  │   PostgreSQL 16 + pgvector   │
│ (FastAPI, PyMuPDF, tiktoken, OpenAI) │  │ (Tenants, Docs, Embeddings,  │
│ - Ingestion & Sliding Window Chunking│  │  1536-dim Cosine Similarity, │
│ - Semantic RAG & Prompt Injection    │  │  Audit Logs, User Identity)  │
│ - 9 Summary Modes & Doc Comparison   │  │                              │
└──────────────────────────────────────┘  └──────────────────────────────┘
```

---

## Key Features

1. **Multi-Format Ingestion**: PyMuPDF (`.pdf`), python-docx (`.docx`), and openpyxl (`.xlsx`) extractors with layout preservation, OCR text normalization, and metadata tracking.
2. **Semantic Sliding-Window Chunking**: Token-based chunking using `tiktoken` (600 tokens with 100-token overlap) respecting paragraph, heading, and sentence boundaries.
3. **pgvector Cosine Search**: 1536-dimensional embeddings (`text-embedding-3-small` / Azure OpenAI) stored with IVFFlat indexing and tenant-partitioned cosine distance queries (`<=>`).
4. **Context-Grounded RAG Engine**:
   - Zero-shot prompt injection mitigation with XML guard boundary tagging.
   - Relevance thresholding: returns explicit insufficient evidence notifications when similarity falls below 0.70.
   - Strict page-level citations: each answer bullet links directly to document name, page number, and original excerpt.
   - Cost & token telemetry per query.
5. **9 Specialized Summary & Extraction Modes**:
   - Executive, Short Overview, Detailed Breakdown, Key Points, FAQs, Dates & Deadlines, Financial & Monetary, Contractual Obligations, Entities & Parties.
6. **Cross-Document Semantic Comparison**:
   - Automated side-by-side contract/policy diffing highlighting Added, Removed, and Modified provisions with page citations.
7. **Enterprise Security & Multi-Tenancy**:
   - Strict tenant isolation enforced at EF Core query filter and database level (`OrganizationId`).
   - Role-Based Access Control (`Admin`, `Manager`, `Employee`, `Auditor`).
   - Tamper-evident, immutable audit trail for compliance.
8. **RAG Evaluation Benchmark**:
   - Automated 50-question golden evaluation dataset across 8 domain categories.
   - Computes Faithfulness, Answer Relevance, Context Recall, Citation Precision, and Hallucination Rate.

---

## Tech Stack

| Layer | Technologies |
|---|---|
| **Backend** | ASP.NET Core 9, Entity Framework Core 9, Npgsql.EntityFrameworkCore.PostgreSQL, ASP.NET Core Identity, JWT, Swashbuckle OpenAPI |
| **AI Microservice** | Python 3.12+, FastAPI, SQLAlchemy (asyncpg), pgvector-python, PyMuPDF (fitz), python-docx, openpyxl, tiktoken, OpenAI / Azure OpenAI |
| **Frontend** | Angular 18, TypeScript, RxJS, SCSS (CSS custom property design system), Chart.js |
| **Storage & DB** | PostgreSQL 16 with `pgvector` extension |
| **Infrastructure** | Docker, Docker Compose, Nginx Alpine |

---

## Project Structure

```
DocuMind AI/
├── backend/
│   ├── DocuMind.API/             # ASP.NET Core 9 Controllers, Middleware, Program.cs
│   ├── DocuMind.Application/     # CQRS/Services, DTOs, Repository Interfaces
│   ├── DocuMind.Domain/          # 12 Core Domain Entities, Enums (No dependencies)
│   ├── DocuMind.Infrastructure/  # AppDbContext, EF Core mappings, Storage, Repos
│   └── DocuMind.Tests/           # Unit tests (xUnit)
├── ai-service/
│   ├── app/
│   │   ├── api/                  # FastAPI routers (/process, /query, /summarize, /compare)
│   │   ├── core/                 # Config, structlog, security API key verification
│   │   ├── db/                   # Async SQLAlchemy session and pgvector queries
│   │   ├── models/               # Pydantic v2 schemas
│   │   └── services/             # Extractors, Chunker, Embedder, RAG Engine
│   ├── evaluation/               # 50-question benchmark dataset & metrics runner
│   └── main.py                   # FastAPI application entrypoint
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/             # Auth/Doc/Chat/Analytics services, guards, interceptors
│   │   │   ├── layout/           # Responsive Shell layout with collapsible sidebar
│   │   │   └── features/         # Auth, Dashboard, Documents, Chat, Compare, Summary, Analytics, Users, Audit, Settings
│   │   └── styles.scss           # Design system tokens and global styles
│   ├── nginx.conf                # Production SPA reverse proxy & gzip configuration
│   └── Dockerfile
├── docker-compose.yml            # Complete 4-service orchestrator
├── .env.example                  # Environment template
└── README.md
```

---

## Quickstart with Docker Compose

1. Clone the repository and navigate to the project directory:
   ```bash
   cd "DocuMind AI"
   ```

2. Copy the environment configuration:
   ```bash
   cp .env.example .env
   ```
   *Edit `.env` and configure your `OPENAI_API_KEY`.*

3. Start all services:
   ```bash
   docker compose up --build -d
   ```

4. Access the platforms:
   - **Frontend UI**: [http://localhost:4200](http://localhost:4200)
   - **Backend Swagger API**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
   - **AI Microservice Docs**: [http://localhost:8000/docs](http://localhost:8000/docs)
   - **PostgreSQL**: `localhost:5432` (`documind` / `documind_user`)

---

## Local Development (Without Docker)

### 1. Database Setup
Ensure PostgreSQL 16 with pgvector is installed and running:
```sql
CREATE DATABASE documind;
\c documind
CREATE EXTENSION IF NOT EXISTS vector;
```

### 2. Run Python AI Microservice
```bash
cd ai-service
python -m venv venv
source venv/bin/activate  # On Windows: .\venv\Scripts\activate
pip install -r requirements.txt
cp .env.example .env
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

### 3. Run ASP.NET Core 9 Web API
```bash
cd backend/DocuMind.API
dotnet run
```

### 4. Run Angular 18 Frontend
```bash
cd frontend
npm install
npm start
```
Navigate to `http://localhost:4200`.

---

## REST API Reference

| Endpoint | Method | Role | Description |
|---|---|---|---|
| `/api/v1/auth/register` | `POST` | Public | Register new tenant organization and initial Admin |
| `/api/v1/auth/login` | `POST` | Public | Authenticate with email and password |
| `/api/v1/auth/me` | `GET` | Authenticated | Retrieve profile of authenticated user |
| `/api/v1/auth/change-password` | `POST` | Authenticated | Update user password |
| `/api/v1/documents` | `GET` | Authenticated | List paginated tenant documents |
| `/api/v1/documents/upload` | `POST` | Employee+ | Upload document (`.pdf`, `.docx`, `.xlsx`) |
| `/api/v1/documents/{id}` | `GET` | Authenticated | Retrieve document metadata |
| `/api/v1/documents/{id}` | `DELETE` | Manager+ | Soft-delete document |
| `/api/v1/documents/{id}/status` | `GET` | Authenticated | Poll ingestion and embedding progress |
| `/api/v1/documents/{id}/summarize` | `POST` | Authenticated | Generate 1 of 9 summary/extraction modes |
| `/api/v1/chat/conversations` | `POST` | Authenticated | Create new scoped conversation thread |
| `/api/v1/chat/conversations/{id}/messages` | `POST` | Authenticated | Query RAG engine with citation grounding |
| `/api/v1/compare` | `POST` | Authenticated | Semantically compare two documents |
| `/api/v1/analytics/dashboard` | `GET` | Manager+ | Overall document, query, and cost KPIs |
| `/api/v1/analytics/uploads-trend` | `GET` | Manager+ | 30-day ingestion histogram |
| `/api/v1/users` | `GET` | Admin | List all tenant organization members |
| `/api/v1/users/{id}/role` | `PUT` | Admin | Update user RBAC role |
| `/api/v1/audit-logs` | `GET` | Admin, Auditor| Query immutable compliance audit logs |

---

## RAG Evaluation Benchmark

The platform includes a dedicated evaluation framework with 50 synthetic test questions across 8 categories:

```bash
cd ai-service
python -m evaluation.runner
```

**Evaluated Metrics:**
- **Faithfulness**: Validates whether the answer is derived strictly from retrieved context chunks.
- **Answer Relevance**: Semantic similarity between prompt intent and synthesized output.
- **Context Recall**: Ratio of relevant gold chunks retrieved by pgvector.
- **Citation Precision**: Proportion of page citations that accurately ground claims.
- **Hallucination Rate**: Flagged unsupported statements.

Target production benchmark: **Faithfulness > 0.85**, **Citation Precision > 0.90**.

