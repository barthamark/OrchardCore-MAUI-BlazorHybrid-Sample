# Harvest Demo – Agent Playbook

This repo hosts the Orchard Harvest 2025 demo prepared by [Márk Bartha](https://markbartha.com). Use this guide whenever an AI assistant needs to pick up work in the codebase.

## Solution Layout

| Path | Purpose |
|------|---------|
| `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` | Orchard host, deployment config, Auto Setup recipes, forwarded-header middleware. Treat `App_Data/` (logs, Sites) as generated. |
| `src/Backend/Themes/MarkBartha.HarvestDemo.OrchardCore.Theme` | Razor theme, Tailwind sources, OpenID application migrations for the MAUI + Web clients. |
| `src/Backend/Modules/MarkBartha.HarvestDemo.OrchardCore.ToDos` | Todos + user profile APIs (`api/todos`, `api/user-profile`) plus DI registrations. |
| `src/Frontend/MarkBartha.HarvestDemo.App.Maui` | .NET MAUI Blazor Hybrid client (Windows/macOS/iOS/Android) using Duende OIDC and WebAuthenticator. |
| `src/Shared/MarkBartha.HarvestDemo.Assets` | Shared Tailwind 4 + LibMan asset pipeline used by both Orchard and MAUI. |
| `src/Shared/MarkBartha.HarvestDemo.Domain` | Shared POCO models (`TodoItem`, `UserProfile`, etc.). |

## Tooling & Builds

1. `dotnet tool restore` – required before **any** build. The MSBuild targets assume `tailwindcss-dotnet` and `libman` already exist.
2. `dotnet restore MarkBartha.HarvestDemo.sln`
3. `dotnet build MarkBartha.HarvestDemo.sln` – runs Tailwind + LibMan via the shared targets; skip editing `wwwroot/` outputs manually.
4. Orchard dev server:  
   `dotnet watch run --project src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web --launch-profile Development`  
   Auto Setup provisions tenants/OpenID on first boot.
5. MAUI builds (pick platform workloads you have installed):  
   `dotnet build src/Frontend/...App.Maui.csproj -f net9.0-android` (or `-windows10.0.19041.0`, `-ios`, `-maccatalyst`).  
   Update `AppConfig.BackendBaseUrl` when pointing to a local backend.

### Asset Pipeline Notes

- Tailwind 4 CLI is fetched automatically by `MarkBartha.HarvestDemo.Assets`; no Node/npm install is necessary.
- `CopyAssets.targets` depends on `RestoreSharedLibManAssets`, which runs `dotnet tool run libman restore` once per build (`obj/libman.restore.stamp`). If vendor files look stale, rerun the build instead of editing `wwwroot/`.
- Place raw CSS in each theme’s `Assets/css` and shared styles in `src/Shared/.../Assets`. The MSBuild targets copy results into consumer `wwwroot/` folders.

## Auth & Platform Notes

- Orchard uses OpenIddict with two public clients: `harvestdemo.maui` and `harvestdemo.web`. The MAUI app relies on the `harvestdemo://callback` URI and `http://127.0.0.1:7605/callback/` (loopback) for Windows.
- `Program.cs` in the Orchard host enables ASP.NET Core forwarded headers so deployed instances (Fly.io) emit `https://` issuers—do not remove `UseForwardedHeaders()`.
- MAUI authentication flows:
  - Android/iOS use `WebAuthenticator.Default` plus the exported callback activity `HarvestDemoWebAuthenticatorCallbackActivity` (intent filter for `harvestdemo://callback`).
  - Desktop builds use a loopback HTTP listener (`LoopbackHttpListener`) + `Process.Start`.
- Secure tokens are stored via `SecureStorage`; `ExternalAuthenticationStateProvider` handles refresh tokens via Duende `OidcClient`.

## UI & Theme Guidelines

- Account/login Razor files follow the layout in `src/Shared/MarkBartha.HarvestDemo.Assets/Temp/account-pages.html`. Stick to the hero badge + card structure and use `.btn` utilities.
- Lucide icons render via `<i class="icon-...">`. Adjust wrapper sizing rather than overriding Lucide’s `font-size: inherit`.
- Password toggles rely on `initPasswordToggles`. Set the appropriate `data-password-*` attributes whenever you introduce another toggle.
- Remember `.btn-icon` must stay hover-transparent. Anchors acting as CTA buttons should include `.btn`.

## Testing & QA

There are no automated test projects yet. When you add features, include targeted unit/integration tests under `tests/<Project>.Tests` and document manual validation steps (Orchard recipe, MAUI device, etc.) in PRs.

## Deployment & Docker

- Root-level `Dockerfile` targets the Debian-based .NET SDK/ASP.NET images (Tailwind CLI needs glibc). It restores tools, publishes the Orchard host, and ignores the MAUI app via `.dockerignore`.
- `fly.toml` sits at the repo root; Fly.io’s CLI (`flyctl`) picks it up automatically. Update `app`, region, and secrets before deploying.  
- If vendors replicate this setup elsewhere, reuse the Dockerfile and swap in another orchestrator manifest.

## House Rules

- Never edit generated outputs (`wwwroot`, `App_Data`).  
- Preserve shared nullable conventions (properties initialized to `string.Empty`).  
- When modifying shared assets or config, ensure both the Orchard host and MAUI client builds still succeed (`dotnet build MarkBartha.HarvestDemo.sln` + at least one MAUI target).  
- Commit messages: imperative mood, ≤72 chars. PRs should mention user-visible changes, link issues, and include screenshots/GIFs for UI tweaks.
