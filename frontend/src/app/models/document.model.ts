/**
 * Backend API response model for documents
 * Matches DocumentUploadResponse from ASP.NET Core API
 */
export interface DocumentResponse {
    id: string;
    fileName: string;
    contentType: string;
    sizeBytes: number;
    uploadedAt: string;
    status: string;
    downloadUrl: string;
    tags: string[];
    excerpt: string | null;
    thumbnailUrl: string | null;
    description: string | null;
}

/**
 * UI model for displaying documents with additional presentation properties
 */
export interface DocumentUI {
    id: string;
    name: string;
    category: string;
    uploadedBy: string;
    uploadDate: string;
    size: string;
    icon: string;
    bgColor: string;
    iconColor: string;
    downloadUrl: string;
    status: string;
    tags: string[];
    excerpt: string | null;
    thumbnailUrl: string | null;
    description: string | null;
}
