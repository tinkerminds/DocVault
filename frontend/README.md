# DocVaultWeb

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.0.4.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.


# Frontend — Angular 17+ SPA

## Overview

The frontend is an **Angular 17+** single-page application using standalone components, Angular Router, and Angular Material. It provides the user interface for uploading, searching, and downloading documents.

## Tech Stack

| Layer      | Technology                          |
| ---------- | ----------------------------------- |
| Framework  | Angular 17+ (standalone components) |
| UI Library | Angular Material                    |
| Styling    | SCSS                                |
| Auth       | MSAL.js (`@azure/msal-angular`)     |
| Telemetry  | Application Insights Web SDK        |

## Feature Modules

| Module       | Components / Services                                   | Day     |
| ------------ | ------------------------------------------------------- | ------- |
| `documents/` | Upload (drag-and-drop), List, Detail, Search            | Day 1–2 |
| `auth/`      | Login, Auth Guard, Auth Interceptor                     | Day 2   |
| `dashboard/` | App Insights summary (uploads, processing time, errors) | Day 4   |
| `shared/`    | DocumentService, TelemetryService, models, interceptors | Day 1–3 |

## Routes

| Route            | Component              | Auth |
| ---------------- | ---------------------- | ---- |
| `/documents`     | Document List          | ✅   |
| `/documents/:id` | Document Detail        | ✅   |
| `/upload`        | Upload (drag-and-drop) | ✅   |
| `/search`        | Full-text Search       | ✅   |
| `/dashboard`     | App Insights Dashboard | ✅   |
| `/login`         | MSAL Login             | ❌   |

## Quick Start

```bash
cd frontend
npm install
ng serve
```

> See [`docs/angular-scaffolding-plan.md`](../docs/angular-scaffolding-plan.md) for full initialization, folder structure, and dependency details.
