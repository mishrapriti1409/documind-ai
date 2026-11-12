Project type: GenAI + RAG + Enterprise SaaS

Problem: Organizations deal with hundreds of policies, contracts, technical documents and compliance reports.

What you build:

AI-Powered Enterprise Document Intelligence Platform

A platform where users upload PDFs, Word documents and Excel files and interact with their content.

Features:
- Ask questions across multiple documents.
- Extract important clauses and information.
- Compare two versions of a policy or contract.
- Identify missing information.
- Generate document summaries.
- Highlight potential compliance issues.
- Provide page-level references for every answer.
AI concepts:
- RAG architecture.
- Embeddings and vector search.
- Chunking and semantic retrieval.
- OCR and document parsing.
- LLM evaluation.
- Hallucination reduction.
Suggested tech stack:
- ASP.NET Core Web API.
- Python AI microservice.
- Azure OpenAI.
- Azure Blob Storage.
- PostgreSQL + pgvector.
- Angular.
- Docker.
Product thinking: Build role-based access, document processing status, user feedback, usage analytics and an admin dashboard.


# PROJECT: DocuMind AI
## Enterprise Document Intelligence & AI Knowledge Management Platform

Act as a Senior Full-Stack Architect, AI Engineer, .NET Developer, Product Designer and Technical Product Manager.

Your task is to design and develop a production-style portfolio application named **DocuMind AI**.

This must be a fully functional, end-to-end AI-powered enterprise document intelligence platform, not a static UI prototype.

The application should demonstrate enterprise software engineering, Generative AI, RAG, Python, ASP.NET Core, database design, API integration, security and product thinking.

---

# 1. PRODUCT VISION

Build an intelligent enterprise knowledge management platform where organizations can upload internal documents and interact with their content using natural language.

The platform should help employees, managers, compliance teams and auditors quickly find, understand, summarize and compare information from organizational documents.

Users should be able to:
- Upload and manage business documents.
- Ask questions about uploaded documents.
- Search across multiple documents.
- Receive AI-generated answers with source citations.
- Compare document versions.
- Generate document summaries.
- Extract important information.
- View document analytics.
- Manage access permissions.
- Review audit history.

The AI must ground its answers in retrieved document content and must not invent information.

Product tagline: "Turn Enterprise Documents Into Actionable Intelligence."

# 2. TECHNOLOGY STACK

Use the following architecture:

Frontend:
- Angular with the latest stable compatible version.
- TypeScript.
- Angular Material or a clean custom component system.
- Responsive design.
- Chart.js for analytics.

Main Backend:
- C#.
- ASP.NET Core Web API using .NET 10 LTS, if the environment supports it.
- Entity Framework Core.
- ASP.NET Core Identity.
- JWT authentication.
- REST APIs.
- Background processing.

AI Backend:
- Python 3.12+.
- FastAPI.
- LangChain or direct LLM orchestration.
- OpenAI API or Azure OpenAI.
- Embedding models.
- Retrieval-Augmented Generation (RAG).

Database:
- PostgreSQL.
- pgvector extension.

Storage:
- Azure Blob Storage abstraction.
- Local storage implementation for development.

Document Processing:
- PyMuPDF.
- python-docx.
- openpyxl.
- OCR integration for scanned documents.

Infrastructure:
- Docker.
- Docker Compose.
- Git.
- GitHub Actions-ready CI/CD structure.

Use environment variables for configuration and API credentials.

If a requested technology is unavailable, explain the limitation and choose the closest compatible alternative. Do not silently replace the entire architecture.

# 3. APPLICATION MODULES

Implement the following modules.

## A. Authentication and User Management

Implement:
- User registration.
- Login.
- Logout.
- Password management.
- JWT authentication.
- Role-based authorization.
- Organization management.
- User profile management.
- Admin user management.

Roles:
1. Admin
2. Manager
3. Employee
4. Auditor

Each organization must have isolated data.

## B. Document Management

Create a document management workspace.

Features:
- Upload PDF, DOCX, TXT, CSV and XLSX files.
- Drag-and-drop uploads.
- Multiple document uploads.
- File validation.
- Document categories.
- Tags.
- Search and filters.
- Document preview.
- Document deletion.
- Document version management.
- Processing status.

Document processing states:
- Uploaded
- Pending
- Processing
- Completed
- Failed

Display processing errors and provide retry functionality.

## C. AI Document Processing

Develop an asynchronous document ingestion pipeline.

Workflow:

1. Receive uploaded document.
2. Validate file type and size.
3. Store original file.
4. Create document metadata.
5. Trigger a background processing job.
6. Extract text.
7. Preserve page numbers and structural metadata.
8. Clean extracted text.
9. Split content into chunks.
10. Generate embeddings.
11. Store chunks and embeddings in PostgreSQL with pgvector.
12. Update document processing status.

Use configurable chunk sizes and overlapping text.

Start with approximately 800 tokens per chunk and 120 tokens of overlap, and make these configurable.

Use background processing instead of keeping HTTP upload requests open during AI processing.

## D. AI Chat and RAG

Create an interactive AI chat workspace.

Users must be able to:
- Ask questions about one document.
- Ask questions about selected documents.
- Search across all documents they are authorized to access.
- Ask follow-up questions.
- View previous conversations.
- Receive answers with citations.
- Open source documents.
- View supporting excerpts.
- Provide answer feedback.

RAG workflow:

1. Receive user question.
2. Validate user and organization permissions.
3. Generate question embedding.
4. Retrieve relevant document chunks.
5. Apply document access filters.
6. Rank relevant results.
7. Construct context.
8. Send context and question to the LLM.
9. Generate an answer grounded in the context.
10. Return answer with source document, page number and excerpt.

Use hybrid retrieval where practical.

If evidence is insufficient, return an explicit insufficient-evidence response.

Never fabricate page references or citations.

Treat uploaded documents as untrusted data and protect against prompt injection.

## E. Document Comparison

Implement a document comparison workspace.

Users can select two documents or two versions of a document.

Features:
- Compare content.
- Identify added sections.
- Identify removed sections.
- Identify modified information.
- Highlight differences.
- Generate an AI summary.
- Identify changes in dates, numbers and obligations.
- Show source page references.

Allow users to review and verify AI-generated comparisons.

## F. Document Summarization and Extraction

Allow users to generate:
- Short summaries.
- Detailed summaries.
- Executive summaries.
- Key points.
- FAQs.
- Important dates.
- Monetary values.
- Names and entities.
- Contract obligations.
- Responsibilities and deadlines.

Use structured JSON outputs where appropriate and validate generated data.

## G. Analytics Dashboard

Create an analytics dashboard with:
- Total documents.
- Processed documents.
- Failed documents.
- Total AI questions.
- Active users.
- Document uploads over time.
- Frequently accessed documents.
- Popular document categories.
- Average processing time.
- User feedback.
- AI usage and cost estimates.

Include charts and filters by date and document category.

Do not present unmeasured metrics as real results.

## H. Security and Audit

Implement:
- Role-based access control.
- Organization-level tenant isolation.
- Document-level access permissions.
- Secure file handling.
- API authentication.
- Input validation.
- Rate limiting.
- Audit logging.
- Secure internal service communication.
- Document deletion and associated embedding cleanup.
- AI request logging without unnecessarily storing sensitive document contents.

Enforce permissions on the backend and during vector retrieval.

# 4. DATABASE DESIGN

Create normalized database tables for:

- Organizations
- Users
- Documents
- DocumentPermissions
- DocumentChunks
- Conversations
- Messages
- MessageSources
- ProcessingJobs
- AuditLogs
- UserFeedback

Use UUID identifiers, appropriate foreign keys, indexes, timestamps and tenant-aware relationships.

Store embeddings in pgvector.

Maintain relationships between:
- Document
- Document chunks
- Page numbers
- AI answers
- Retrieved source references

Generate database migrations and seed data.

# 5. API ARCHITECTURE

ASP.NET Core APIs should handle:
- Authentication.
- User management.
- Organization management.
- Document uploads.
- Document metadata.
- Permissions.
- Chat history.
- Analytics.
- Audit logs.

Python FastAPI should handle:
- Document processing.
- Text extraction.
- Chunking.
- Embedding generation.
- Vector retrieval.
- RAG generation.
- Summarization.
- Document comparison.

Use versioned REST endpoints, request validation, consistent response structures and centralized error handling.

Document all APIs using Swagger/OpenAPI.

Secure internal AI service endpoints.

# 6. USER INTERFACE AND DESIGN

Build a professional enterprise SaaS interface.

Design direction:
- Minimal.
- Modern.
- Clean.
- Professional.
- White/off-white background.
- Dark navy typography.
- Subtle blue and purple accents.
- Rounded cards.
- Consistent spacing.
- Responsive layout.
- Accessible typography and contrast.

Create the following pages:

1. Login and Registration.
2. Main Dashboard.
3. Document Library.
4. Document Upload.
5. Document Preview.
6. AI Chat Workspace.
7. Document Comparison.
8. Summary and Extraction.
9. Analytics.
10. User Management.
11. Roles and Permissions.
12. Audit Logs.
13. Settings.

Use a persistent sidebar containing:
- Overview
- Documents
- AI Assistant
- Compare Documents
- Analytics
- User Management
- Audit Logs
- Settings

The AI Chat Workspace should have:
- Chat conversation area.
- Document selector.
- Message input.
- AI response area.
- Citation references.
- Source preview panel.
- Conversation history.

Provide proper loading, empty, error and success states.

Do not use static dummy data to simulate core functionality.

# 7. AI EVALUATION

Create an evaluation framework for RAG.

Include a test dataset with approximately 50 sample questions and expected source documents.

Measure:
- Retrieval precision.
- Retrieval recall.
- Answer faithfulness.
- Answer relevance.
- Citation accuracy.
- Response latency.
- Estimated model cost.

Test:
- Questions with answers in documents.
- Questions without supporting evidence.
- Conflicting documents.
- Outdated documents.
- Restricted document access.
- Prompt injection attempts.

Create an evaluation report with actual measured results.

# 8. DEVELOPMENT APPROACH

Do not generate the entire application in one uncontrolled implementation.

Work incrementally.

Phase 1:
- Analyze the project requirements.
- Create system architecture.
- Create folder structure.
- Create database schema.
- Initialize Angular, ASP.NET Core and Python projects.
- Build the initial design system.

Phase 2:
- Implement authentication.
- Implement organizations and roles.
- Build document upload.
- Implement storage.
- Implement database integration.

Phase 3:
- Implement asynchronous document processing.
- Implement extraction.
- Implement chunking.
- Implement embeddings.
- Integrate pgvector.

Phase 4:
- Implement RAG.
- Implement semantic search.
- Implement AI chat.
- Implement source citations.
- Implement conversation history.

Phase 5:
- Implement summarization.
- Implement document comparison.
- Implement structured extraction.

Phase 6:
- Implement analytics.
- Implement audit logs.
- Implement permissions.
- Add security protections.

Phase 7:
- Write unit and integration tests.
- Test the complete user journey.
- Add Docker Compose.
- Create deployment instructions.
- Prepare a demo dataset.

After every phase:
1. Verify that the code compiles.
2. Run the relevant tests.
3. Fix errors.
4. Update README documentation.
5. Summarize completed features.
6. List remaining work.

Do not mark a feature complete if it is only represented by a UI mockup.

# 9. ENGINEERING STANDARDS

Follow:
- SOLID principles.
- Clean architecture where appropriate.
- Dependency injection.
- Repository/service separation where justified.
- Async/await.
- Proper exception handling.
- Logging.
- Configuration management.
- DTOs and request validation.
- Secure coding practices.
- Maintainable and reusable code.
- Separation of AI logic from business logic.

Avoid unnecessary abstractions and microservices.

Do not hardcode API keys or secrets.

Use `.env.example` with placeholder values.

Include meaningful comments for complex AI workflows.

# 10. FINAL DELIVERABLES

The final project should include:

1. Complete Angular frontend.
2. Complete ASP.NET Core backend.
3. Python AI service.
4. PostgreSQL database integration.
5. pgvector integration.
6. Working RAG pipeline.
7. Working document upload and processing.
8. AI chat with citations.
9. Document comparison.
10. Document summarization.
11. Authentication and authorization.
12. Analytics dashboard.
13. Audit logging.
14. Unit and integration tests.
15. Docker Compose configuration.
16. API documentation.
17. Architecture documentation.
18. Database schema documentation.
19. Setup and deployment instructions.
20. GitHub-ready README.

# 11. IMPORTANT INSTRUCTIONS

Before writing code, inspect the existing workspace.

If it is empty, initialize the complete project structure.

First provide:
- Architecture overview.
- Folder structure.
- Database design.
- Development milestones.
- UI design direction.

Then begin implementation.

Do not stop after generating the architecture or a frontend prototype.

Build a working end-to-end vertical slice first:
- Register and log in.
- Upload a document.
- Process the document.
- Generate embeddings.
- Ask a question.
- Retrieve relevant content.
- Generate an answer.
- Display citations.

Only after this complete workflow works should you proceed with the remaining modules.

If API credentials are unavailable, implement the provider integration and clearly explain how to configure the credentials. Do not pretend that live AI functionality has been tested.

Make sensible implementation decisions without repeatedly asking for approval on routine technical details.

At each milestone, report:
- Files created or modified.
- Features implemented.
- Commands to run.
- Tests performed.
- Known issues.
- Next milestone.

The goal is to deliver a professional, functional enterprise AI product that can be demonstrated in a software engineering or AI engineering interview.