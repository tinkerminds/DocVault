import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Document {
  id: number;
  name: string;
  uploadedBy: string;
  date: string;
  icon: string;
  bgColor: string;
  iconColor: string;
}

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.scss',
})
export class DocumentListComponent {
  documents: Document[] = [
    {
      id: 1,
      name: 'Q3_Financial_Report.pdf',
      uploadedBy: 'Sarah Jenkins',
      date: 'Oct 24, 2024',
      icon: 'picture_as_pdf',
      bgColor: 'rgb(219, 234, 254)',
      iconColor: 'rgb(37, 99, 235)',
    },
    {
      id: 2,
      name: 'Project_Alpha_Specs.docx',
      uploadedBy: 'Mike Chen',
      date: 'Oct 22, 2024',
      icon: 'description',
      bgColor: 'rgb(219, 234, 254)',
      iconColor: 'rgb(37, 99, 235)',
    },
    {
      id: 3,
      name: 'Inventory_List_2024.xlsx',
      uploadedBy: 'Alex Morgan',
      date: 'Oct 20, 2024',
      icon: 'table_view',
      bgColor: 'rgb(220, 252, 231)',
      iconColor: 'rgb(34, 197, 94)',
    },
  ];
}
