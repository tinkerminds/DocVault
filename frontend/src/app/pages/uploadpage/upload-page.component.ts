import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface UploadFile {
  id: string;
  name: string;
  size: number;
  progress: number;
  status: 'uploading' | 'ready' | 'completed' | 'failed';
  icon: string;
  bgColor: string;
  iconColor: string;
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
  uploadedFiles: UploadFile[] = [
    {
      id: '1',
      name: 'Project_Alpha_Specs_v2.pdf',
      size: 4.1,
      progress: 65,
      status: 'uploading',
      icon: 'picture_as_pdf',
      bgColor: 'rgb(254, 226, 226)',
      iconColor: 'rgb(220, 38, 38)',
    },
    {
      id: '2',
      name: 'Quarterly_Financials_Q3.docx',
      size: 1.8,
      progress: 0,
      status: 'ready',
      icon: 'description',
      bgColor: 'rgb(219, 234, 254)',
      iconColor: 'rgb(37, 99, 235)',
    },
  ];

  tags: string[] = ['Finance', 'Internal'];
  showSuccessNotification = true;

  constructor(private fb: FormBuilder) {
    this.fileForm = this.fb.group({
      tags: [''],
      description: ['', Validators.required],
      destination: ['Main Vault', Validators.required],
    });
  }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files) {
      for (let file of files) {
        this.addFile(file);
      }
    }
  }

  addFile(file: File): void {
    const newFile: UploadFile = {
      id: Date.now().toString(),
      name: file.name,
      size: file.size / (1024 * 1024),
      progress: 0,
      status: 'ready',
      icon: this.getFileIcon(file.name),
      bgColor: this.getFileColor(file.name).bg,
      iconColor: this.getFileColor(file.name).icon,
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
      console.log('Uploading files with form data:', this.fileForm.value);
      console.log('Tags:', this.tags);
      this.simulateUpload();
    }
  }

  simulateUpload(): void {
    this.uploadedFiles.forEach((file, index) => {
      if (file.status === 'ready') {
        file.status = 'uploading';
        let progress = 0;
        const interval = setInterval(() => {
          progress += Math.random() * 30;
          if (progress >= 100) {
            progress = 100;
            file.progress = progress;
            file.status = 'completed';
            clearInterval(interval);
          } else {
            file.progress = Math.round(progress);
          }
        }, 500);
      }
    });
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
