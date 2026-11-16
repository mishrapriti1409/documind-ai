// Authentication & User Models
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'Manager' | 'Employee' | 'Auditor';
  organizationId: string;
  isActive: boolean;
  lastLoginAt?: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  token: string;
  role: string;
  organizationId: string;
  organizationName: string;
}

// Document Models
export type ProcessingStatus = 'Uploaded' | 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface Document {
  id: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  category?: string;
  tags?: string;
  processingStatus: ProcessingStatus;
  processingError?: string;
  totalPages: number;
  totalChunks: number;
  createdAt: string;
  processedAt?: string;
  uploadedBy: string;
}

export interface DocumentListResponse {
  items: Document[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ProcessingStatusDto {
  documentId: string;
  status: string;
  errorMessage?: string;
  attemptCount: number;
  startedAt?: string;
  completedAt?: string;
}

// Chat & RAG Models
export type ConversationScope = 'SingleDocument' | 'MultiDocument' | 'AllDocuments';

export interface Conversation {
  id: string;
  title: string;
  scope: ConversationScope;
  createdAt: string;
  updatedAt: string;
  messageCount: number;
  documentIds?: string[];
}

export interface SourceReference {
  chunkId: string;
  documentId: string;
  documentName: string;
  pageNumber: number;
  excerpt: string;
  relevanceScore: number;
}

export interface Message {
  id: string;
  role: 'User' | 'Assistant';
  content: string;
  tokensUsed: number;
  estimatedCost: number;
  feedbackScore?: number;
  createdAt: string;
  sources: SourceReference[];
}

export interface ChatResponse {
  messageId: string;
  answer: string;
  sources: SourceReference[];
  insufficientEvidence: boolean;
  tokensUsed: number;
  costEstimate: number;
}

// Analytics Models
export interface DashboardStats {
  totalDocuments: number;
  processedDocuments: number;
  failedDocuments: number;
  pendingDocuments: number;
  totalConversations: number;
  totalMessages: number;
  activeUsersLast30Days: number;
  averageProcessingTimeSeconds: number;
  totalAICostEstimate: number;
}

export interface DocumentUploadTrend {
  date: string;
  count: number;
}

export interface UserActivity {
  userId: string;
  userName: string;
  documentsUploaded: number;
  questionsAsked: number;
  lastActive: string;
}

// Summary Models
export interface SummaryResult {
  document_id: string;
  summary_type: string;
  content: string | any;
  tokens_used: number;
}

// Comparison Models
export interface ComparisonSection {
  description: string;
  page_references: number[];
}

export interface ComparisonResult {
  document_id_a: string;
  document_id_b: string;
  added_sections: ComparisonSection[];
  removed_sections: ComparisonSection[];
  modified_sections: ComparisonSection[];
  ai_summary: string;
  tokens_used: number;
}

// Audit Log Models
export interface AuditLog {
  id: string;
  userId?: string;
  userName: string;
  action: string;
  resourceType: string;
  resourceId?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  createdAt: string;
}

export interface AuditLogResponse {
  items: AuditLog[];
  total: number;
  page: number;
  pageSize: number;
}

