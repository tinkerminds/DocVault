import { Component, OnInit, OnDestroy, HostListener, ElementRef, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MsalService, MSAL_GUARD_CONFIG, MsalGuardConfiguration } from '@azure/msal-angular';
import { InteractionType } from '@azure/msal-browser';
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
  isAuthenticated = false;
  userName = '';

  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  constructor(
    private router: Router,
    private documentService: DocumentService,
    private elementRef: ElementRef,
    private authService: MsalService,
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration
  ) { }

  ngOnInit(): void {
    this.checkAuth();

    // Only load documents if authenticated
    if (this.isAuthenticated) {
      this.loadDocumentsForSearch();
    }

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

  login(): void {
    if (this.msalGuardConfig.interactionType === InteractionType.Redirect) {
      const authRequest = this.msalGuardConfig.authRequest;
      const scopes = authRequest && typeof authRequest !== 'function'
        ? authRequest.scopes ?? []
        : [];
      this.authService.loginRedirect({ scopes });
    }
  }

  logout(): void {
    this.authService.logoutRedirect({
      postLogoutRedirectUri: '/',
    });
  }

  navigateToHome(): void {
    this.router.navigate(['/']);
  }

  toggleSearch(): void {
    this.isSearchOpen = !this.isSearchOpen;
    if (this.isSearchOpen) {
      this.searchQuery = '';
      this.filteredDocuments = this.allDocuments.slice(0, 5);
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

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isSearchOpen && !this.elementRef.nativeElement.contains(event.target)) {
      this.closeSearch();
    }
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isSearchOpen) {
      this.closeSearch();
    }
  }

  private checkAuth(): void {
    const accounts = this.authService.instance.getAllAccounts();
    this.isAuthenticated = accounts.length > 0;
    if (this.isAuthenticated) {
      const account = accounts[0];
      this.userName = account.name || account.username || '';
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
