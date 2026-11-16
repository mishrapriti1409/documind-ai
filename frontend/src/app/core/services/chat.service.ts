import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatResponse, Conversation, ConversationScope, Message } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/conversations`;

  constructor(private http: HttpClient) {}

  createConversation(title: string, scope: ConversationScope = 'AllDocuments', documentIds?: string[]): Observable<Conversation> {
    return this.http.post<Conversation>(this.apiUrl, {
      title,
      scope,
      documentIds
    });
  }

  getConversations(page: number = 1, pageSize: number = 20): Observable<Conversation[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Conversation[]>(this.apiUrl, { params });
  }

  getConversation(id: string): Observable<Conversation> {
    return this.http.get<Conversation>(`${this.apiUrl}/${id}`);
  }

  getMessages(conversationId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/${conversationId}/messages`);
  }

  sendMessage(conversationId: string, question: string, documentIds?: string[]): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${this.apiUrl}/${conversationId}/messages`, {
      question,
      documentIds
    });
  }

  submitFeedback(conversationId: string, messageId: string, score: number, comment?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conversationId}/messages/${messageId}/feedback`, {
      score,
      comment
    });
  }
}

