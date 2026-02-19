import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, Subject } from 'rxjs';
import { App } from './app';
import { DocumentService } from './services/document.service';
import { MsalService, MsalBroadcastService, MSAL_GUARD_CONFIG } from '@azure/msal-angular';
import { InteractionType } from '@azure/msal-browser';

// Mock DocumentService so no real HTTP calls are made during tests
const mockDocumentService = {
  getDocuments: () => of([]),
  uploadDocument: () => of({}),
  deleteDocument: () => of(void 0),
  documentUploaded$: new Subject<void>().asObservable(),
};

// Mock MsalService so we don't need a real Azure AD connection in tests
const mockMsalService = {
  instance: {
    handleRedirectPromise: () => Promise.resolve(null),
    getAllAccounts: () => [],
    getActiveAccount: () => null,
    setActiveAccount: () => {},
  },
  loginRedirect: () => of(void 0),
  logout: () => of(void 0),
};

// Mock MsalBroadcastService
const mockMsalBroadcastService = {
  inProgress$: of(0), // InteractionStatus.None = 0
  msalSubject$: new Subject<void>().asObservable(),
};

// Mock MSAL Guard config
const mockMsalGuardConfig = {
  interactionType: InteractionType.Redirect,
  authRequest: { scopes: ['user.read'] },
};

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DocumentService, useValue: mockDocumentService },
        { provide: MsalService, useValue: mockMsalService },
        { provide: MsalBroadcastService, useValue: mockMsalBroadcastService },
        { provide: MSAL_GUARD_CONFIG, useValue: mockMsalGuardConfig },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render navbar', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-navbar')).toBeTruthy();
  });
});
