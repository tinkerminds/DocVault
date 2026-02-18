import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { DocumentService } from '../../services/document.service';

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
  showSuccessNotification = false;

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    private router: Router
  ) {
    this.fileForm = this.fb.group({
      tags: [''],
      description: [''],
      destination: ['Main Vault', Validators.required],
    });
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
      const totalFiles = this.uploadedFiles.filter(f => f.status === 'ready').length;

      this.uploadedFiles.forEach(fileWrapper => {
        if (fileWrapper.status === 'ready') {
          fileWrapper.status = 'uploading';
          fileWrapper.progress = 0;

          this.documentService.uploadDocument(fileWrapper.file, tagsString)
            .subscribe({
              next: (response) => {
                fileWrapper.status = 'completed';
                fileWrapper.progress = 100;
                completedCount++;

                // Show success notification when all uploads complete
                if (completedCount === totalFiles) {
                  this.showSuccessNotification = true;

                  // Navigate to documents page after 2 seconds
                  setTimeout(() => {
                    this.router.navigate(['/documents']);
                  }, 2000);
                }
              },
              error: (err) => {
                fileWrapper.status = 'failed';
                fileWrapper.progress = 0;
                console.error('Upload failed for', fileWrapper.name, ':', err);
                completedCount++;
              }
            });
        }
      });
    }
  }

  cancelUpload(): void {
    this.fileForm.reset({ destination: 'Main Vault' });
    this.uploadedFiles = [];
    this.tags = [];
  }

  closeNotification(): void {
    this.showSuccessNotification = false;
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
