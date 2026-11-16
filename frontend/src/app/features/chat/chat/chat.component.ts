import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ChatService } from '../../../core/services/chat.service';
import { DocumentService } from '../../../core/services/document.service';
import { Conversation, Document, Message, SourceReference } from '../../../core/models';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit {
  @ViewChild('scrollContainer') private scrollContainer?: ElementRef;

  conversations: Conversation[] = [];
  selectedConversation: Conversation | null = null;
  messages: Message[] = [];
  documents: Document[] = [];

  userQuestion = '';
  selectedDocIds: string[] = [];
  isSending = false;
  isLoadingMessages = false;
  isLoadingConversations = true;
  activeSourceModal: SourceReference | null = null;

  constructor(
    private chatService: ChatService,
    private docService: DocumentService
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
    this.loadConversations();
  }

  loadDocuments(): void {
    this.docService.getList(1, 100, undefined, 'Completed').subscribe({
      next: res => this.documents = res.items
    });
  }

  loadConversations(): void {
    this.isLoadingConversations = true;
    this.chatService.getConversations().subscribe({
      next: convs => {
        this.conversations = convs;
        this.isLoadingConversations = false;
        if (convs.length > 0) {
          this.selectConversation(convs[0]);
        } else {
          this.startNewConversation();
        }
      },
      error: () => this.isLoadingConversations = false
    });
  }

  selectConversation(conv: Conversation): void {
    this.selectedConversation = conv;
    this.selectedDocIds = conv.documentIds || [];
    this.isLoadingMessages = true;

    this.chatService.getMessages(conv.id).subscribe({
      next: msgs => {
        this.messages = msgs;
        this.isLoadingMessages = false;
        this.scrollToBottom();
      },
      error: () => this.isLoadingMessages = false
    });
  }

  startNewConversation(): void {
    const title = 'Document Inquiry ' + new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    this.chatService.createConversation(title, 'AllDocuments').subscribe({
      next: newConv => {
        this.conversations.unshift(newConv);
        this.selectedConversation = newConv;
        this.messages = [];
        this.selectedDocIds = [];
      }
    });
  }

  sendMessage(): void {
    if (!this.userQuestion.trim() || !this.selectedConversation || this.isSending) {
      return;
    }

    const question = this.userQuestion.trim();
    this.userQuestion = '';
    this.isSending = true;

    // Optimistically append user message
    const tempUserMsg: Message = {
      id: 'temp-' + Date.now(),
      role: 'User',
      content: question,
      tokensUsed: 0,
      estimatedCost: 0,
      createdAt: new Date().toISOString(),
      sources: []
    };
    this.messages.push(tempUserMsg);
    this.scrollToBottom();

    const docFilter = this.selectedDocIds.length > 0 ? this.selectedDocIds : undefined;

    this.chatService.sendMessage(this.selectedConversation.id, question, docFilter).subscribe({
      next: res => {
        const assistantMsg: Message = {
          id: res.messageId,
          role: 'Assistant',
          content: res.answer,
          tokensUsed: res.tokensUsed,
          estimatedCost: res.costEstimate,
          createdAt: new Date().toISOString(),
          sources: res.sources
        };
        this.messages.push(assistantMsg);
        this.isSending = false;
        this.scrollToBottom();
      },
      error: () => {
        this.messages.push({
          id: 'err-' + Date.now(),
          role: 'Assistant',
          content: 'An error occurred while querying the document intelligence service. Please verify your connection.',
          tokensUsed: 0,
          estimatedCost: 0,
          createdAt: new Date().toISOString(),
          sources: []
        });
        this.isSending = false;
        this.scrollToBottom();
      }
    });
  }

  submitFeedback(msg: Message, score: number): void {
    if (!this.selectedConversation) return;

    this.chatService.submitFeedback(this.selectedConversation.id, msg.id, score).subscribe({
      next: () => {
        msg.feedbackScore = score;
      }
    });
  }

  openSourceExcerpt(source: SourceReference): void {
    this.activeSourceModal = source;
  }

  closeSourceModal(): void {
    this.activeSourceModal = null;
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }
}

