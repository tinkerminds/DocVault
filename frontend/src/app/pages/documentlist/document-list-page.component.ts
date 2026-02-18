import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../services/document.service';
import { DocumentUI } from '../../models/document.model';

@Component({
  selector: 'app-document-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './document-list-page.component.html',
  styleUrl: './document-list-page.component.scss',
})
export class DocumentListPageComponent implements OnInit {
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

  constructor(
    private documentService: DocumentService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.documentService.getDocuments().subscribe({
      next: (documents) => {
        this.allDocuments = documents.map(doc => this.mapToUIDocument(doc));
        this.totalResults = this.allDocuments.length;
        this.filterAndPaginate();
        this.isLoading = false;

        // Manually trigger change detection
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading documents:', error);
        this.errorMessage = 'Failed to load documents. Please try again.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private mapToUIDocument(doc: any): DocumentUI {
    const fileExt = doc.fileName.split('.').pop()?.toLowerCase() || '';
    const colors = this.getFileColor(fileExt);

    return {
      id: doc.id,
      name: doc.fileName,
      category: this.getCategoryFromContentType(doc.contentType),
      uploadedBy: 'User', // Placeholder - backend doesn't provide this
      uploadDate: this.formatDate(doc.uploadedAt),
      size: this.formatFileSize(doc.sizeBytes),
      icon: this.getFileIcon(fileExt),
      bgColor: colors.bg,
      iconColor: colors.icon,
      downloadUrl: doc.downloadUrl,
      status: doc.status
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
      day: 'numeric'
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
    this.filterAndPaginate();
  }

  filterAndPaginate(): void {
    // Filter documents
    if (this.searchQuery.trim()) {
      this.filteredDocuments = this.allDocuments.filter(
        (doc) =>
          doc.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
          doc.category.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
          doc.uploadedBy.toLowerCase().includes(this.searchQuery.toLowerCase()),
      );
    } else {
      this.filteredDocuments = [...this.allDocuments];
    }

    this.totalResults = this.filteredDocuments.length;

    // Paginate
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

  moreActions(doc: DocumentUI): void {
    console.log('More actions for:', doc.name);
  }
}
