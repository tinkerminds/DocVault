import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeroComponent } from '../../components/hero.component/hero.component';
import { FeaturesComponent } from '../../components/features.component/features.component';
import { DocumentListComponent } from '../../components/document-list.component/document-list.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HeroComponent, FeaturesComponent, DocumentListComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {}
