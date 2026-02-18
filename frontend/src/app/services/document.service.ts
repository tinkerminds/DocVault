import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { DocumentResponse } from '../models/document.model';

@Injectable({
    providedIn: 'root'
})
export class DocumentService {
    private readonly apiUrl = `${environment.apiBaseUrl}/documents`;

    constructor(private http: HttpClient) { }

    /**
     * Get all documents for the current user
     * GET /api/documents
     */
    getDocuments(): Observable<DocumentResponse[]> {
        return this.http.get<DocumentResponse[]>(this.apiUrl)
            .pipe(
                catchError(this.handleError)
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

        return this.http.post<DocumentResponse>(this.apiUrl, formData)
            .pipe(
                catchError(this.handleError)
            );
    }

    /**
     * Delete a document (soft delete)
     * DELETE /api/documents/{id}
     * @param id - Document ID to delete
     */
    deleteDocument(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`)
            .pipe(
                catchError(this.handleError)
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
