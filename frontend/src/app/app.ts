import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './components/navbar.component/navbar.component';
import { FooterComponent } from './components/footer.component/footer.component';
import { SnackbarComponent } from './components/snackbar.component/snackbar.component';
import { MsalBroadcastService, MsalService, MSAL_GUARD_CONFIG, MsalGuardConfiguration } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, NavbarComponent, FooterComponent, SnackbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit, OnDestroy {
  isAuthenticated = false;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private authService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration
  ) { }

  async ngOnInit(): Promise<void> {
    // Handle redirect callback (when user comes back from login page)
    await this.authService.instance.handleRedirectPromise();

    // Listen for interaction status changes
    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.checkAuthentication();
      });

    this.checkAuthentication();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private checkAuthentication(): void {
    const accounts = this.authService.instance.getAllAccounts();
    this.isAuthenticated = accounts.length > 0;

    // Set the active account if logged in
    if (this.isAuthenticated && !this.authService.instance.getActiveAccount()) {
      this.authService.instance.setActiveAccount(accounts[0]);
    }
  }
}
