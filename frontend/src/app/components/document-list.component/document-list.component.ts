import { Component, Input, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { DocumentService } from '../../services/document.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface Document {
  id: string;
  name: string;
  uploadedBy: string;
  date: string;
  icon: string;
  bgColor: string;
  iconColor: string;
  downloadUrl?: string;
}

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.scss',
})
export class DocumentListComponent implements OnInit, OnDestroy {
  @Input() limit: number = 3;
  documents: Document[] = [];
  isLoading = true;
  isLoggedIn = false;
  private destroy$ = new Subject<void>();

  constructor(
    private documentService: DocumentService,
    private authService: MsalService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.instance.getAllAccounts().length > 0;

    if (this.isLoggedIn) {
      this.loadDocuments();

      // Refresh when a new document is uploaded
      this.documentService.documentUploaded$.pipe(takeUntil(this.destroy$)).subscribe(() => {
        this.loadDocuments();
      });
    } else {
      this.isLoading = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDocuments(): void {
    this.isLoading = true;
    this.documentService.getDocuments().subscribe({
      next: (response) => {
        if (response && response.length > 0) {
          // Sort by uploadedAt descending — most recent first
          const sorted = [...response].sort(
            (a, b) => new Date(b.uploadedAt).getTime() - new Date(a.uploadedAt).getTime(),
          );
          this.documents = this.mapDocuments(sorted).slice(0, this.limit);
        } else {
          this.documents = [];
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.documents = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  private mapDocuments(response: any[]): Document[] {
    return response.map((doc) => ({
      id: doc.id,
      name: doc.fileName || doc.name,
      uploadedBy: doc.uploadedBy || 'User',
      date: this.formatDate(doc.uploadedAt),
      icon: this.getIcon(doc.fileName || doc.name),
      bgColor: this.getBackgroundColor(doc.fileName || doc.name),
      iconColor: this.getIconColor(doc.fileName || doc.name),
      downloadUrl: doc.downloadUrl,
    }));
  }

  private formatDate(dateString: string): string {
    if (!dateString) return 'Unknown';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  private getIcon(fileName: string): string {
    const ext = fileName?.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf':
        return 'picture_as_pdf';
      case 'docx':
      case 'doc':
        return 'description';
      case 'xlsx':
      case 'xls':
        return 'table_view';
      case 'pptx':
      case 'ppt':
        return 'slideshow';
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
      case 'webp':
        return 'image';
      default:
        return 'insert_drive_file';
    }
  }

  private getBackgroundColor(fileName: string): string {
    const ext = fileName?.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf':
        return 'rgb(254, 226, 226)';
      case 'xlsx':
      case 'xls':
        return 'rgb(220, 252, 231)';
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
      case 'webp':
        return 'rgb(254, 243, 199)';
      default:
        return 'rgb(219, 234, 254)';
    }
  }

  private getIconColor(fileName: string): string {
    const ext = fileName?.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf':
        return 'rgb(220, 38, 38)';
      case 'xlsx':
      case 'xls':
        return 'rgb(34, 197, 94)';
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
      case 'webp':
        return 'rgb(217, 119, 6)';
      default:
        return 'rgb(37, 99, 235)';
    }
  }
}
