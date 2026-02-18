import { Component, OnInit, OnDestroy, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { DocumentResponse } from '../../models/document.model';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements OnInit, OnDestroy {
  isSearchOpen = false;
  searchQuery = '';
  allDocuments: DocumentResponse[] = [];
  filteredDocuments: DocumentResponse[] = [];
  isLoadingSearch = false;

  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  constructor(
    private router: Router,
    private documentService: DocumentService,
    private elementRef: ElementRef
  ) { }

  ngOnInit(): void {
    // Load all documents once for search recommendations
    this.loadDocumentsForSearch();

    // Debounce search input to avoid filtering on every keystroke
    this.searchSubject
      .pipe(debounceTime(200), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((query) => {
        this.filterDocuments(query);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  navigateToHome(): void {
    this.router.navigate(['/']);
  }

  toggleSearch(): void {
    this.isSearchOpen = !this.isSearchOpen;
    if (this.isSearchOpen) {
      this.searchQuery = '';
      this.filteredDocuments = this.allDocuments.slice(0, 5);
      // Focus the input after the DOM updates
      setTimeout(() => {
        const input = this.elementRef.nativeElement.querySelector('.search-overlay-input');
        if (input) input.focus();
      }, 50);
    }
  }

  closeSearch(): void {
    this.isSearchOpen = false;
    this.searchQuery = '';
    this.filteredDocuments = [];
  }

  onSearchInput(query: string): void {
    this.searchSubject.next(query);
  }

  navigateToDocument(doc: DocumentResponse): void {
    this.closeSearch();
    this.router.navigate(['/documents']);
  }

  getFileIcon(fileName: string): string {
    const ext = fileName?.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf': return 'picture_as_pdf';
      case 'docx': case 'doc': return 'description';
      case 'xlsx': case 'xls': return 'table_view';
      case 'pptx': case 'ppt': return 'slideshow';
      case 'jpg': case 'jpeg': case 'png': case 'gif': case 'webp': return 'image';
      default: return 'insert_drive_file';
    }
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  // Close search overlay when clicking outside the navbar
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isSearchOpen && !this.elementRef.nativeElement.contains(event.target)) {
      this.closeSearch();
    }
  }

  // Close search on Escape key
  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isSearchOpen) {
      this.closeSearch();
    }
  }

  private loadDocumentsForSearch(): void {
    this.isLoadingSearch = true;
    this.documentService.getDocuments()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (docs) => {
          this.allDocuments = docs;
          this.isLoadingSearch = false;
        },
        error: () => {
          this.allDocuments = [];
          this.isLoadingSearch = false;
        }
      });
  }

  private filterDocuments(query: string): void {
    if (!query.trim()) {
      this.filteredDocuments = this.allDocuments.slice(0, 5);
      return;
    }
    const lower = query.toLowerCase();
    this.filteredDocuments = this.allDocuments
      .filter((doc) => doc.fileName.toLowerCase().includes(lower))
      .slice(0, 5);
  }
}
