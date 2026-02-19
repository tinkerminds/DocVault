import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { DocumentResponse } from '../models/document.model';

@Injectable({
  providedIn: 'root',
})
export class DocumentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/documents`;
  private documentUploadedSubject = new Subject<DocumentResponse>();
  public documentUploaded$ = this.documentUploadedSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get all documents for the current user
   * GET /api/documents
   */
  getDocuments(): Observable<DocumentResponse[]> {
    console.log(`Fetching documents from: ${this.apiUrl}`);
    return this.http.get<DocumentResponse[]>(this.apiUrl).pipe(
      tap((response) => {
        console.log('Documents API response:', response);
      }),
      catchError((error) => {
        console.error('HTTP Error fetching documents:', error);
        return this.handleError(error);
      }),
    );
  }

  /**
   * Upload a new document
   * POST /api/documents
   * @param file - File to upload
   * @param tags - Optional comma-separated tags
   */
  uploadDocument(file: File, tags?: string): Observable<DocumentResponse> {
    const formData = new FormData();
    formData.append('file', file);

    if (tags) {
      formData.append('tags', tags);
    }

    console.log(`Uploading document to: ${this.apiUrl}`, {
      fileName: file.name,
      fileSize: file.size,
      tags,
    });

    return this.http.post<DocumentResponse>(this.apiUrl, formData).pipe(
      tap((response) => {
        console.log('Upload successful, emitting event:', response);
        // Emit document uploaded event for real-time updates
        this.documentUploadedSubject.next(response);
      }),
      catchError((error) => {
        console.error('Upload error:', error);
        return this.handleError(error);
      }),
    );
  }

  /**
   * Delete a document (soft delete)
   * DELETE /api/documents/{id}
   * @param id - Document ID to delete
   */
  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(catchError(this.handleError));
  }

  /**
   * Search documents by filename, tags, or excerpt content (server-side)
   * GET /api/documents/search?q=term
   * @param query - Search term
   */
  searchDocuments(query: string): Observable<DocumentResponse[]> {
    return this.http
      .get<DocumentResponse[]>(`${this.apiUrl}/search`, {
        params: { q: query },
      })
      .pipe(
        tap((response) => {
          console.log('Search results:', response);
        }),
        catchError(this.handleError),
      );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }

    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
