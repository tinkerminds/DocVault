import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Document {
  id: string;
  name: string;
  category: string;
  uploadedBy: string;
  uploadDate: string;
  size: string;
  icon: string;
  bgColor: string;
  iconColor: string;
}

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

  documents: Document[] = [
    {
      id: '1',
      name: 'Q3_Financial_Report.pdf',
      category: 'Finance',
      uploadedBy: 'Sarah',
      uploadDate: 'Oct 24, 2023',
      size: '2.4 MB',
      icon: 'picture_as_pdf',
      bgColor: 'rgb(254, 226, 226)',
      iconColor: 'rgb(220, 38, 38)',
    },
    {
      id: '2',
      name: 'Employee_Handbook_v2.docx',
      category: 'HR',
      uploadedBy: 'John',
      uploadDate: 'Sep 12, 2023',
      size: '850 KB',
      icon: 'description',
      bgColor: 'rgb(219, 234, 254)',
      iconColor: 'rgb(37, 99, 235)',
    },
    {
      id: '3',
      name: 'Project_Alpha_Specs.xlsx',
      category: 'Engineering',
      uploadedBy: 'Mike',
      uploadDate: 'Aug 05, 2023',
      size: '4.1 MB',
      icon: 'table_view',
      bgColor: 'rgb(220, 252, 231)',
      iconColor: 'rgb(34, 197, 94)',
    },
    {
      id: '4',
      name: 'Marketing_Assets_2024.zip',
      category: 'Marketing',
      uploadedBy: 'Emma',
      uploadDate: 'Jul 22, 2023',
      size: '156 MB',
      icon: 'folder_zip',
      bgColor: 'rgb(254, 243, 199)',
      iconColor: 'rgb(217, 119, 6)',
    },
    {
      id: '5',
      name: 'Client_Contract_Template.pdf',
      category: 'Legal',
      uploadedBy: 'David',
      uploadDate: 'Jun 15, 2023',
      size: '1.2 MB',
      icon: 'picture_as_pdf',
      bgColor: 'rgb(254, 226, 226)',
      iconColor: 'rgb(220, 38, 38)',
    },
  ];

  allDocuments: Document[] = [];
  filteredDocuments: Document[] = [];
  totalResults: number = 0;
  paginatedDocuments: Document[] = [];

  ngOnInit(): void {
    this.allDocuments = [...this.documents];
    this.totalResults = this.allDocuments.length;
    this.filterAndPaginate();
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

  downloadDocument(doc: Document): void {
    console.log('Downloading:', doc.name);
  }

  viewDocument(doc: Document): void {
    console.log('Viewing:', doc.name);
  }

  moreActions(doc: Document): void {
    console.log('More actions for:', doc.name);
  }
}
