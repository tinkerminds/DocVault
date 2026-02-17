# Angular Scaffolding Plan

> **Structure only — no components are implemented.**

---

## Initialization

```bash
# From the repo root
cd frontend

# Create Angular 17+ app with standalone components and routing
ng new DocVault.Web --standalone --routing --style=scss --skip-git --directory=.
```

### Key flags

| Flag            | Reason                                          |
| --------------- | ----------------------------------------------- |
| `--standalone`  | Use standalone components (Angular 17+ default) |
| `--routing`     | Enable the Angular Router with `app.routes.ts`  |
| `--style=scss`  | Use SCSS for styling                            |
| `--skip-git`    | Repo-level `.git` already exists                |
| `--directory=.` | Scaffold into the current `frontend/` folder    |

---

## Install Angular Material

```bash
ng add @angular/material
```

During the interactive setup:

- Choose a prebuilt theme (e.g., **Indigo/Pink** or **Custom**)
- Enable global typography styles: **Yes**
- Enable animations: **Yes**

---

## Additional Dependencies

```bash
# MSAL for Entra ID authentication (Day 2)
npm install @azure/msal-browser @azure/msal-angular

# Application Insights SDK (Day 3)
npm install @microsoft/applicationinsights-web
```

| Package                              | Day   | Purpose                               |
| ------------------------------------ | ----- | ------------------------------------- |
| `@azure/msal-browser`                | Day 2 | MSAL.js for Entra ID login/logout     |
| `@azure/msal-angular`                | Day 2 | Angular-specific MSAL integration     |
| `@microsoft/applicationinsights-web` | Day 3 | Client-side telemetry to App Insights |

---

## Folder Structure

After scaffolding, create the following feature folders:

```
frontend/src/app/
├── auth/                  # Entra ID login, logout, MSAL config, auth guard
│   ├── auth.guard.ts
│   ├── auth.config.ts
│   └── login/
│       └── login.component.ts
├── documents/             # Core document management features
│   ├── upload/            # Drag-and-drop upload with progress indicator
│   │   └── upload.component.ts
│   ├── list/              # Document list (name, date, size, download link)
│   │   └── document-list.component.ts
│   ├── detail/            # Single document view + SAS download URL
│   │   └── document-detail.component.ts
│   └── search/            # Full-text search over excerpts
│       └── search.component.ts
├── dashboard/             # App Insights summary (Day 4)
│   └── dashboard.component.ts
├── shared/                # Shared components, pipes, services, models
│   ├── services/
│   │   ├── document.service.ts    # HTTP calls to the API
│   │   └── telemetry.service.ts   # App Insights wrapper (Day 3)
│   ├── models/
│   │   └── document.model.ts      # TypeScript interface matching Cosmos schema
│   └── interceptors/
│       └── auth.interceptor.ts    # Attach bearer token to API requests
├── app.component.ts       # Root standalone component (generated)
├── app.routes.ts          # Route definitions (see below)
└── app.config.ts          # App configuration (generated)
```

---

## Routing Plan

| Route            | Component                 | Auth Required | Day   |
| ---------------- | ------------------------- | ------------- | ----- |
| `/`              | Redirect to `/documents`  | ✅            | Day 1 |
| `/documents`     | `DocumentListComponent`   | ✅            | Day 1 |
| `/documents/:id` | `DocumentDetailComponent` | ✅            | Day 1 |
| `/upload`        | `UploadComponent`         | ✅            | Day 1 |
| `/search`        | `SearchComponent`         | ✅            | Day 2 |
| `/dashboard`     | `DashboardComponent`      | ✅            | Day 4 |
| `/login`         | `LoginComponent`          | ❌            | Day 2 |

---

## Environment Configuration

```
frontend/src/environments/
├── environment.ts          # Development settings
└── environment.prod.ts     # Production settings
```

Key environment variables:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: "https://localhost:5001/api", // Direct API (Day 1)
  // apiBaseUrl: 'https://docvault-apim.azure-api.net/api',  // APIM (Day 3)
  msalClientId: "<entra-app-client-id>", // Day 2
  msalAuthority: "https://login.microsoftonline.com/<tenant-id>", // Day 2
  appInsightsKey: "<instrumentation-key>", // Day 3
};
```

> **Day 3:** Switch `apiBaseUrl` to point to the APIM endpoint instead of the direct API.

---

## Day-by-Day Component Plan

| Day   | Components to Build                                                       |
| ----- | ------------------------------------------------------------------------- |
| Day 1 | Upload component (drag-and-drop + progress), document list + download     |
| Day 2 | Login/logout with MSAL, auth guard on routes, auth interceptor for tokens |
| Day 3 | Search component, update API base URL to APIM, telemetry service          |
| Day 4 | Dashboard (App Insights summary), polish and bug fixes                    |
