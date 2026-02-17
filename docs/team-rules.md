# Team Working Rules — Day 1

---

## Branching Rules

| Rule                  | Detail                                                |
| --------------------- | ----------------------------------------------------- |
| **No direct commits** | Never push directly to `main` or `dev`                |
| **Feature branches**  | All work happens in `feature/<task-name>` branches    |
| **Branch from `dev`** | Every feature branch is created from `dev`            |
| **Merge via PR only** | All feature branches merge to `dev` via Pull Request  |
| **Promote to `main`** | `dev` → `main` merges happen via PR after QA sign-off |

---

## Pull Request Rules

1. Every PR must have a **clear title** following the commit convention:
   ```
   type(scope): description
   ```
2. Every PR must have a **description** explaining what was changed and why
3. **CI must pass** before a PR can be merged — no exceptions
4. At least **1 reviewer** must approve the PR
5. **No force-pushes** to `main` or `dev`

---

## Commit Convention

Format: **`type(scope): description`**

| Type       | Use for                               |
| ---------- | ------------------------------------- |
| `feat`     | New feature                           |
| `fix`      | Bug fix                               |
| `docs`     | Documentation changes                 |
| `chore`    | Maintenance, dependencies, config     |
| `ci`       | CI/CD pipeline changes                |
| `refactor` | Code restructure (no behavior change) |
| `test`     | Adding or updating tests              |

**Examples:**

```
feat(api): add document upload controller
fix(frontend): correct file size display
docs(infra): update Azure setup plan
chore(backend): update Cosmos SDK to 3.38
```

---

## CI Gate

- CI runs on every PR to `main` and `dev`
- **Both** backend and frontend builds must pass
- A failing CI **blocks merge** — no manual overrides
- Fix the issue, push again, CI re-runs automatically

---

## Day 1 Checklist per Developer

- [ ] Clone the repository
- [ ] Create a feature branch from `dev`
- [ ] Make changes in the assigned folder
- [ ] Commit with the correct message format
- [ ] Push and open a PR to `dev`
- [ ] Wait for CI to pass and reviewer approval
- [ ] Merge via the PR (squash merge preferred)
