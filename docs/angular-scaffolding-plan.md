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

## Folder Structure

After scaffolding, create the following empty feature folders:

```
frontend/src/app/
├── auth/                  # Authentication guards, login page (later)
│   └── README.md
├── documents/             # Document list, upload, detail views (later)
│   └── README.md
├── shared/                # Shared components, pipes, services, models
│   └── README.md
├── app.component.ts       # Root standalone component (generated)
├── app.routes.ts          # Route definitions (generated)
└── app.config.ts          # App configuration (generated)
```

Each subfolder contains only a `README.md` placeholder for Day 1.

---

## What's NOT Done on Day 1

- No components created inside `auth/`, `documents/`, or `shared/`
- No routing defined besides the default
- No API service connections
- No authentication logic
