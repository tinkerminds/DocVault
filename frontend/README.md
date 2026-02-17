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
