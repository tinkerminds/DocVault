import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../services/document.service';
import { DocumentUI } from '../../models/document.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-document-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './document-list-page.component.html',
  styleUrl: './document-list-page.component.scss',
})
export class DocumentListPageComponent implements OnInit, OnDestroy {
  searchQuery: string = '';
  currentPage: number = 1;
  itemsPerPage: number = 5;
  sortBy: string = 'date';

  allDocuments: DocumentUI[] = [];
  filteredDocuments: DocumentUI[] = [];
  totalResults: number = 0;
  paginatedDocuments: DocumentUI[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  showDeleteDialog: boolean = false;
  documentToDelete: DocumentUI | null = null;
  isDeleting: boolean = false;
  private destroy$ = new Subject<void>();

  constructor(
    private documentService: DocumentService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
    // Subscribe to real-time document upload events
    this.documentService.documentUploaded$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.loadDocuments();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDocuments(): void {
    this.isLoading = true;
    this.errorMessage = '';
    console.log('Loading documents from API for document list page...');

    this.documentService.getDocuments().subscribe({
      next: (documents) => {
        console.log('Documents fetched from API:', documents);
        if (documents && documents.length > 0) {
          this.allDocuments = documents.map((doc) => this.mapToUIDocument(doc));
          // Sort by date descending (most recent first)
          this.allDocuments.sort((a, b) => {
            const dateA = new Date(a.uploadDate).getTime();
            const dateB = new Date(b.uploadDate).getTime();
            return dateB - dateA;
          });
          this.totalResults = this.allDocuments.length;
          console.log('Mapped UI documents:', this.allDocuments);
        } else {
          console.log('No documents from API');
          this.allDocuments = [];
          this.totalResults = 0;
        }
        this.filterAndPaginate();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading documents:', error);
        this.errorMessage = 'Failed to load documents. Please try again.';
        this.allDocuments = [];
        this.totalResults = 0;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  private mapToUIDocument(doc: any): DocumentUI {
    const fileExt = doc.fileName.split('.').pop()?.toLowerCase() || '';
    const colors = this.getFileColor(fileExt);

    return {
      id: doc.id,
      name: doc.fileName,
      category: this.getCategoryFromContentType(doc.contentType),
      uploadedBy: 'User',
      uploadDate: this.formatDate(doc.uploadedAt),
      size: this.formatFileSize(doc.sizeBytes),
      icon: this.getFileIcon(fileExt),
      bgColor: colors.bg,
      iconColor: colors.icon,
      downloadUrl: doc.downloadUrl,
      status: doc.status,
      tags: doc.tags || [],
      excerpt: doc.excerpt || null,
      thumbnailUrl: doc.thumbnailUrl || null,
      description: doc.description || null,
    };
  }

  private getFileIcon(ext: string): string {
    switch (ext) {
      case 'pdf':
        return 'picture_as_pdf';
      case 'docx':
      case 'doc':
        return 'description';
      case 'xlsx':
      case 'xls':
        return 'table_view';
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
        return 'image';
      case 'zip':
      case 'rar':
        return 'folder_zip';
      default:
        return 'insert_drive_file';
    }
  }

  private getFileColor(ext: string): { bg: string; icon: string } {
    switch (ext) {
      case 'pdf':
        return { bg: 'rgb(254, 226, 226)', icon: 'rgb(220, 38, 38)' };
      case 'docx':
      case 'doc':
        return { bg: 'rgb(219, 234, 254)', icon: 'rgb(37, 99, 235)' };
      case 'xlsx':
      case 'xls':
        return { bg: 'rgb(220, 252, 231)', icon: 'rgb(34, 197, 94)' };
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
        return { bg: 'rgb(254, 243, 199)', icon: 'rgb(217, 119, 6)' };
      case 'zip':
      case 'rar':
        return { bg: 'rgb(254, 243, 199)', icon: 'rgb(217, 119, 6)' };
      default:
        return { bg: 'rgb(243, 244, 246)', icon: 'rgb(107, 114, 128)' };
    }
  }

  private getCategoryFromContentType(contentType: string): string {
    if (contentType.includes('pdf')) return 'Document';
    if (contentType.includes('word') || contentType.includes('document')) return 'Document';
    if (contentType.includes('spreadsheet') || contentType.includes('excel')) return 'Spreadsheet';
    if (contentType.includes('image')) return 'Image';
    if (contentType.includes('zip') || contentType.includes('compressed')) return 'Archive';
    return 'File';
  }

  private formatDate(isoDate: string): string {
    const date = new Date(isoDate);
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    };
    return date.toLocaleDateString('en-US', options);
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  onSearchChange(): void {
    this.currentPage = 1;

    // Use server-side search for queries >= 3 characters
    if (this.searchQuery.trim().length >= 3) {
      this.isLoading = true;
      this.documentService.searchDocuments(this.searchQuery.trim()).subscribe({
        next: (documents) => {
          this.filteredDocuments = documents.map((doc) => this.mapToUIDocument(doc));
          this.totalResults = this.filteredDocuments.length;
          this.paginate();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          // Fallback to client-side filter on error
          this.filterClientSide();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
      });
    } else {
      this.filterClientSide();
    }
  }

  private filterClientSide(): void {
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      this.filteredDocuments = this.allDocuments.filter(
        (doc) =>
          doc.name.toLowerCase().includes(query) ||
          doc.category.toLowerCase().includes(query) ||
          (doc.excerpt && doc.excerpt.toLowerCase().includes(query)) ||
          doc.tags.some((tag) => tag.toLowerCase().includes(query)),
      );
    } else {
      this.filteredDocuments = [...this.allDocuments];
    }
    this.totalResults = this.filteredDocuments.length;
    this.paginate();
  }

  filterAndPaginate(): void {
    this.filterClientSide();
  }

  private paginate(): void {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    this.paginatedDocuments = this.filteredDocuments.slice(startIndex, endIndex);
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.filterAndPaginate();
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  nextPage(): void {
    const totalPages = Math.ceil(this.totalResults / this.itemsPerPage);
    if (this.currentPage < totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalResults / this.itemsPerPage);
  }

  getStartIndex(): number {
    return (this.currentPage - 1) * this.itemsPerPage + 1;
  }

  getEndIndex(): number {
    return Math.min(this.currentPage * this.itemsPerPage, this.totalResults);
  }

  downloadDocument(doc: DocumentUI): void {
    if (doc.downloadUrl) {
      window.open(doc.downloadUrl, '_blank');
    } else {
      console.error('Download URL not available for:', doc.name);
    }
  }

  viewDocument(doc: DocumentUI): void {
    // Open document in new tab using download URL
    if (doc.downloadUrl) {
      window.open(doc.downloadUrl, '_blank');
    } else {
      console.error('View URL not available for:', doc.name);
    }
  }

  confirmDelete(doc: DocumentUI): void {
    this.documentToDelete = doc;
    this.showDeleteDialog = true;
  }

  cancelDelete(): void {
    this.showDeleteDialog = false;
    this.documentToDelete = null;
  }

  executeDelete(): void {
    if (!this.documentToDelete) return;

    this.isDeleting = true;
    const docId = this.documentToDelete.id;
    const docName = this.documentToDelete.name;

    this.documentService.deleteDocument(docId).subscribe({
      next: () => {
        console.log('Document deleted successfully:', docName);
        // Remove from local arrays
        this.allDocuments = this.allDocuments.filter((d) => d.id !== docId);
        this.totalResults = this.allDocuments.length;
        this.filterAndPaginate();
        this.showDeleteDialog = false;
        this.documentToDelete = null;
        this.isDeleting = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error deleting document:', error);
        this.errorMessage = `Failed to delete "${docName}". Please try again.`;
        this.showDeleteDialog = false;
        this.documentToDelete = null;
        this.isDeleting = false;
        this.cdr.detectChanges();
      },
    });
  }
}

