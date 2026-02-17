# Team Working Rules

---

## Branching Rules

| Rule                  | Detail                                               |
| --------------------- | ---------------------------------------------------- |
| **No direct commits** | Never push directly to `main` or `dev`               |
| **Feature branches**  | All work happens in `feature/<task-name>` branches   |
| **Branch from `dev`** | Every feature branch is created from `dev`           |
| **Merge via PR only** | All feature branches merge to `dev` via Pull Request |
| **Promote to `main`** | `dev` → `main` merges happen via PR at end of day    |

### Example Branch Names

```
feature/upload-api
feature/list-api
feature/upload-ui
feature/list-ui
feature/entra-auth
feature/api-auth
feature/keyvault
feature/blob-function
feature/search-api
feature/event-grid
feature/service-bus
feature/apim
feature/app-insights
feature/docker
```

> **One branch per task, not per person.** If two people work on the same task, they collaborate on the same branch.

---

## Pull Request Rules

1. Every PR must have a **clear title** following the commit convention:

   ```
   type(scope): description
   ```

2. **PR description must answer three questions:**
   - **(1) What does this change?** — Summary of the feature or fix
   - **(2) Which Azure service does it touch?** — e.g., Blob Storage, Cosmos DB, Key Vault
   - **(3) How do I test it?** — Steps the reviewer can follow to verify

3. **CI must pass** before a PR can be merged — no exceptions

4. At least **1 reviewer** must approve the PR before merging

5. The reviewer must check:
   - Does it work?
   - Is it secure (no secrets in code)?
   - Does it follow the agreed patterns?

6. **Keep PRs small** — a PR with 500+ changed lines is too big. Split large features into smaller PRs (e.g., first the API endpoint, then the Angular component, then the wiring)

7. **No force-pushes** to `main` or `dev`

8. **Resolve merge conflicts on your feature branch** before requesting review. Never resolve conflicts in `main` or `dev`.

9. Use GitHub's review features: leave inline comments, request changes if something is wrong, and approve when it's ready. **Do not just click Approve without reading.**

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
feat(api): add upload endpoint with blob storage
fix(auth): correct MSAL redirect URI
chore(infra): add Key Vault resource to setup script
docs(cosmos): update data model with excerpt field
ci(deploy): add App Service deployment step
```

### Commit Frequency

- **Aim for 5+ commits per person per day**
- Commit every time you finish a logical unit of work — a working endpoint, a component, a config change
- Don't batch 3 hours of work into one commit — if something breaks, you'll lose it all

### Never Commit

- Connection strings, keys, passwords
- `appsettings.Development.json`, `.env`, `local.settings.json`
- These go in environment variables or Key Vault

---

## CI Gate

- CI runs on every PR to `main` and `dev`
- **Both** backend and frontend builds must pass
- A failing CI **blocks merge** — no manual overrides
- Fix the issue, push again, CI re-runs automatically
- **If CI is red, fix it immediately.** A broken pipeline blocks the entire team.

---

## Daily Workflow

### Every Developer, Every Day

1. Pull latest `dev` (`git pull origin dev`)
2. Create feature branch (`git checkout -b feature/<task-name>`)
3. Work in small increments, committing after each logical change
4. Push branch and open a PR to `dev`
5. Wait for CI to pass + reviewer approval
6. Merge via the PR (squash merge preferred)
7. Repeat for next task

### End-of-Day

1. Ensure all feature PRs are merged into `dev`
2. As a team, create a PR from `dev` → `main`
3. Full team review of the day's combined changes
4. Merge to `main` → CD pipeline auto-deploys to Azure
5. Verify the deployment is working

---

## Security Checklist

- [ ] No secrets in source code
- [ ] No secrets in `appsettings.json`
- [ ] `.gitignore` includes `appsettings.Development.json`, `.env`, `local.settings.json`
- [ ] All secrets stored in Azure Key Vault (Day 2+)
- [ ] Managed Identity used for service-to-service access (Day 2+)
- [ ] SAS tokens used for download links, never raw blob URLs
