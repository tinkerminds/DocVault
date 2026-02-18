import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MsalService, MSAL_GUARD_CONFIG, MsalGuardConfiguration } from '@azure/msal-angular';
import { InteractionType } from '@azure/msal-browser';
import { DocumentService } from '../../services/document.service';
import { SnackbarService } from '../../services/snackbar.service';

interface UploadFile {
  id: string;
  name: string;
  size: number;
  progress: number;
  status: 'uploading' | 'ready' | 'completed' | 'failed';
  icon: string;
  bgColor: string;
  iconColor: string;
  file: File; // Store actual File object
}

@Component({
  selector: 'app-upload-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './upload-page.component.html',
  styleUrl: './upload-page.component.scss',
})
export class UploadPageComponent {
  fileForm: FormGroup;
  uploadedFiles: UploadFile[] = [];
  tags: string[] = [];

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    private router: Router,
    private authService: MsalService,
    private snackbar: SnackbarService,
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
  ) {
    this.fileForm = this.fb.group({
      tags: [''],
      description: [''],
      destination: ['Main Vault', Validators.required],
    });

    // Redirect to login if not authenticated
    const accounts = this.authService.instance.getAllAccounts();
    if (accounts.length === 0) {
      const authRequest = this.msalGuardConfig.authRequest;
      const scopes = authRequest && typeof authRequest !== 'function'
        ? authRequest.scopes ?? []
        : [];
      this.authService.loginRedirect({ scopes });
    }
  }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        this.addFile(files[i]);
      }
    }
  }

  addFile(file: File): void {
    const newFile: UploadFile = {
      id: Date.now().toString() + Math.random(),
      name: file.name,
      size: file.size / (1024 * 1024),
      progress: 0,
      status: 'ready',
      icon: this.getFileIcon(file.name),
      bgColor: this.getFileColor(file.name).bg,
      iconColor: this.getFileColor(file.name).icon,
      file: file // Store the actual File object
    };
    this.uploadedFiles.push(newFile);
  }

  removeFile(fileId: string): void {
    this.uploadedFiles = this.uploadedFiles.filter((f) => f.id !== fileId);
  }

  clearAllFiles(): void {
    this.uploadedFiles = [];
  }

  addTag(event: any): void {
    const input = event.target;
    if (input.value && input.value.trim()) {
      if (!this.tags.includes(input.value.trim())) {
        this.tags.push(input.value.trim());
      }
      input.value = '';
    }
  }

  removeTag(tag: string): void {
    this.tags = this.tags.filter((t) => t !== tag);
  }

  uploadAllFiles(): void {
    if (this.fileForm.valid && this.uploadedFiles.length > 0) {
      const tagsString = this.tags.join(',');
      let completedCount = 0;
      let failedCount = 0;
      const readyFiles = this.uploadedFiles.filter(f => f.status === 'ready');
      const totalFiles = readyFiles.length;

      if (totalFiles === 0) return;

      readyFiles.forEach(fileWrapper => {
        fileWrapper.status = 'uploading';
        fileWrapper.progress = 0;

        this.documentService.uploadDocument(fileWrapper.file, tagsString)
          .subscribe({
            next: () => {
              fileWrapper.status = 'completed';
              fileWrapper.progress = 100;
              completedCount++;

              // When all uploads have resolved (success or fail), show summary
              if (completedCount + failedCount === totalFiles) {
                if (failedCount === 0) {
                  const label = totalFiles === 1 ? 'file' : 'files';
                  this.snackbar.success(
                    `${totalFiles} ${label} uploaded successfully! ✅`,
                    3000,
                  );
                  setTimeout(() => this.router.navigate(['/documents']), 2000);
                } else {
                  this.snackbar.error(
                    `${completedCount} uploaded, ${failedCount} failed. Check your files and try again.`,
                    5000,
                  );
                }
              }
            },
            error: (err) => {
              fileWrapper.status = 'failed';
              fileWrapper.progress = 0;
              failedCount++;
              console.error('Upload failed for', fileWrapper.name, ':', err);

              // Show individual error snackbar immediately
              const status = err?.status;
              const msg = status === 401
                ? `Upload failed: You are not signed in. Please log in and try again.`
                : status === 413
                  ? `"${fileWrapper.name}" is too large to upload.`
                  : `Failed to upload "${fileWrapper.name}". Please try again.`;
              this.snackbar.error(msg, 5000);

              completedCount++; // count toward total to trigger summary check
              if (completedCount + failedCount === totalFiles && completedCount > 0) {
                // Some succeeded — show partial success
                this.snackbar.info(
                  `${completedCount - failedCount} of ${totalFiles} files uploaded.`,
                  4000,
                );
              }
            }
          });
      });
    }
  }

  cancelUpload(): void {
    this.fileForm.reset({ destination: 'Main Vault' });
    this.uploadedFiles = [];
    this.tags = [];
  }

  private getFileIcon(fileName: string): string {
    const ext = fileName.split('.').pop()?.toLowerCase();
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
      default:
        return 'insert_drive_file';
    }
  }

  private getFileColor(fileName: string): { bg: string; icon: string } {
    const ext = fileName.split('.').pop()?.toLowerCase();
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
      default:
        return { bg: 'rgb(243, 244, 246)', icon: 'rgb(107, 114, 128)' };
    }
  }
}
