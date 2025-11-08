# Orchard Harvest 2025 Demo App

A cross-platform to-do application developed by [Márk Bartha](https://markbartha.com) for the [Orchard Harvest 2025](https://orchardcore.net/harvest) conference. Built with Orchard Core, .NET MAUI, and Blazor Hybrid.

## Highlights

- **Headless Orchard Core CMS** – Custom theme for the login page, OpenID Connect, and JSON APIs for todos and user profile data.
- **Shared UI System** – TailwindCSS 4 pipeline powered by the Tailwind 4 CLI (downloaded automatically), LibMan vendor assets, and shared components reused across Orchard and MAUI.
- **Shared Asset Pipeline** – `src/Shared/MarkBartha.HarvestDemo.Assets` orchestrates Tailwind compilation and vendor copies on every `dotnet build`, so no Node/npm installs are required.
- **MAUI Blazor Hybrid App** – a single Blazor UI delivered to Windows, macOS, iOS, and Android with native WebAuthenticator flows and offline-friendly state containers.

## Architecture

- `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` – Orchard Core host.
- `src/Backend/Themes/MarkBartha.HarvestDemo.OrchardCore.Theme` – theme with overrides for the login shapes.
- `src/Backend/Modules/MarkBartha.HarvestDemo.OrchardCore.ToDos` – module for to-dos + user-profile API endpoints and services.
- `src/Frontend/MarkBartha.HarvestDemo.App.Maui` – .NET MAUI Blazor Hybrid client with Duende OIDC integration.
- `src/Shared/MarkBartha.HarvestDemo.Assets` – shared Tailwind 4/LibMan pipeline (executed automatically during builds).
- `src/Shared/MarkBartha.HarvestDemo.Domain` – shared POCO models referenced by both backend and MAUI layers.

## Prerequisites

- .NET 8 SDK (for the Orchard Core app) and .NET 9 SDK (for the MAUI app)
- .NET workloads: `dotnet workload install maui maui-android maui-ios maui-maccatalyst maui-windows`
- Platform SDKs as needed (Android SDK, Xcode, Windows SDK)
- Optional: Docker + [flyctl](https://fly.io/docs/hands-on/install-flyctl/) if deploying to Fly.io

## Setup

### Orchard Core backend

```
dotnet tool restore
dotnet build MarkBartha.HarvestDemo.sln
dotnet run --project src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web
```

The Auto Setup feature seeds tenants, OpenID applications, and sample content the first time the site boots—no manual configuration is required.

### MAUI app

```
# Windows
dotnet build src/Frontend/MarkBartha.HarvestDemo.App.Maui/MarkBartha.HarvestDemo.App.Maui.csproj -f net9.0-windows10.0.19041.0

# Android
dotnet build src/Frontend/MarkBartha.HarvestDemo.App.Maui/MarkBartha.HarvestDemo.App.Maui.csproj -f net9.0-android

# iOS
dotnet build src/Frontend/MarkBartha.HarvestDemo.App.Maui/MarkBartha.HarvestDemo.App.Maui.csproj -f net9.0-ios

# Mac Catalyst
dotnet build src/Frontend/MarkBartha.HarvestDemo.App.Maui/MarkBartha.HarvestDemo.App.Maui.csproj -f net9.0-maccatalyst
```

If you run the backend locally, update `AppConfig.BackendBaseUrl` accordingly. The MAUI app already registers the `harvestdemo://callback` scheme and uses platform WebAuthenticator flows.

## Docker / Fly.io

This project deploys via [Fly.io](https://fly.io) for simplicity. To keep that workflow:

1. Install [flyctl](https://fly.io/docs/hands-on/install-flyctl/) and authenticate (`fly auth login`).
2. Edit `fly.toml` to set your Fly app name, region, and secrets, or just delete it and regenerate it using the CLI.
3. Build or deploy:

   ```
   docker build -t harvestdemo .
   fly deploy
   ```

If you host elsewhere, reuse the Dockerfile and substitute your platform-specific deployment manifest.

---

Feel free to open issues, send PRs, or reach out through any of the channels listed on [markbartha.com](https://markbartha.com). I'd love to hear what you build with this demo.
