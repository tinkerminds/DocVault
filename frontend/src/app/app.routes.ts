import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { HomeComponent } from './pages/home/home.component';
import { UploadPageComponent } from './pages/uploadpage/upload-page.component';
import { DocumentListPageComponent } from './pages/documentlist/document-list-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
  },
  {
    path: 'home',
    component: HomeComponent,
  },
  {
    path: 'upload',
    component: UploadPageComponent,
    canActivate: [MsalGuard],
  },
  {
    path: 'documents',
    component: DocumentListPageComponent,
    canActivate: [MsalGuard],
  },
];
