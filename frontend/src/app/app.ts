import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { NavbarComponent } from './components/navbar.component/navbar.component';
import { FooterComponent } from './components/footer.component/footer.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HttpClientModule, NavbarComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('DocVault.Web');
}
