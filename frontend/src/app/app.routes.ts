import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { UploadPageComponent } from './pages/uploadpage/upload-page.component';

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
  },
];
