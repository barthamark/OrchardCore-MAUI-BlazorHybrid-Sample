# Repository Guidelines

## Project Structure & Module Organization
The solution `MarkBartha.HarvestDemo.sln` groups the Orchard CMS host, theme, and shared asset pipeline. `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` hosts Orchard Core; it owns `Program.cs`, deployment config, and runtime `App_Data` (treat `logs/` and `Sites/` as generated state). `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Theme` holds Razor views, theme-specific `Assets/`, and the compiled `wwwroot/`. Shared styling lives in `src/Shared/MarkBartha.HarvestDemo.Assets`, which runs the Tailwind build and vendored libraries via LibMan.

## Build, Test, and Development Commands
- `dotnet restore MarkBartha.HarvestDemo.sln` - pull NuGet dependencies.
- `dotnet tool restore` - install local CLI tools (`tailwind`, `libman`) defined in `.config/dotnet-tools.json`.
- `dotnet build MarkBartha.HarvestDemo.sln` - compile the host and theme; invokes Tailwind and asset copy targets automatically.
- `dotnet build src/Frontend/MarkBartha.HarvestDemo.App.Maui/MarkBartha.HarvestDemo.App.Maui.csproj -f net9.0-windows10.0.19041.0` - validates the MAUI client without needing Android toolchains. Full solution builds currently fail on Android (`XA5300`) until the .NET MAUI Android workload / SDK is installed or `AndroidSdkDirectory` is configured.
- `dotnet run --project src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` - start the CMS locally (add `--launch-profile Development` when debugging HTTPS).
- `dotnet watch run --project src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` - hot-reload Razor and C# changes using the watcher include/exclude rules.
- `dotnet tool run libman restore` - refresh `wwwroot/vendors/` when `libman.json` changes.

## Coding Style & Naming Conventions
Use the default .NET style: four-space indents, braces on new lines, PascalCase for types and methods, and camelCase for locals. Keep asynchronous APIs suffixed with `Async`, and prefer Orchard Core shape names that mirror their Razor file paths. Run `dotnet format` before submitting to keep nullable-enabled code clean and consistent.

## Asset Pipeline Notes
Theme builds call `CopyAssets.targets` and `RunTailwindBuild.targets`, so place raw CSS in `Assets/css` and shared styles in `src/Shared/.../Assets`. Shared libraries from `src/Shared/.../wwwroot` (e.g., `vendors/lucide/font/lucide.css`) are copied by the same target into each consumer's `wwwroot/`; override `SharedPublishedAssetsDestinationFolder` if a project needs a different static root (the future MAUI app will point this at its hybrid `wwwroot`). If a change touches Tailwind configuration, run `dotnet tailwind exec -i ./Assets/css/app.css -o ./wwwroot/css/app.css` manually to verify the output. Do not edit files under `wwwroot/` directly; treat them as generated artifacts.

- `Targets/CopyAssets.targets` now depends on a `RestoreSharedLibManAssets` target that runs `dotnet tool run libman restore` once per build (tracked by `obj/libman.restore.stamp`). If a build fails because vendor assets are stale, confirm LibMan output instead of bypassing the target.

## Testing Guidelines
No automated test project ships yet; new features should include focused unit or integration tests alongside the feature. Prefer xUnit and locate future test projects under `tests/` matching the project under test (for example, `tests/MarkBartha.HarvestDemo.OrchardCore.Web.Tests`). Until the harness lands, document manual validation steps in the PR and attach relevant Orchard recipes to seed data when needed.

## Commit & Pull Request Guidelines
Write commit subjects in the imperative mood (e.g., `Add tenant setup recipe`), and keep the first line under 72 characters. Each pull request should link to its issue, describe user-visible changes, and include screenshots or screencasts for UI updates. Confirm `dotnet build`, `dotnet tailwind exec`, and local Orchard startup succeed before requesting review, and mention any follow-up work in the PR notes.

## Theme & Auth UI Notes (2025-11-01)
- Account-related shapes live under `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Theme/Views`. They were refactored to follow the `Temp/account-pages.html` layout: hero badge, Tailwind card, and supporting copy. Keep Razor logic intact—only adjust markup/classes inside each shape.
- Lucide icons are rendered via `<i class="icon-...">` using the bundled font CSS. Tailwind utilities still handle color/sizing on the `<i>` element or its wrapper.
- Buttons rely on `.btn` utilities from `common.css`; anchors acting as buttons should carry `btn` to pick up pointer/underline behavior. `.btn-icon` is also defined there and must remain hover-transparent.
- Password fields include toggle buttons driven by the global `initPasswordToggles` helper (defined inline where needed). To add a new toggle, set `data-password-toggle`, `data-password-icon` attributes, and include the shared init script if not already present on the page.
- “Remember me” checkboxes should be wrapped in a single label with `inline-flex` so clicking the label toggles the input.
- Success/confirmation cards intentionally omit inner icons—only the hero badge uses a decorative icon with a soft brand background. Match existing patterns when introducing new screens.

## Backend Module Notes (2025-11-03)
- The ToDos Orchard Core module exposes `api/todos` plus a new `api/user-profile` endpoint. `UserProfileController` now loads data through `IUserProfileStore` to keep future profile storage changes localized.
- `UserProfileStore` lives under `src/Backend/Modules/MarkBartha.HarvestDemo.OrchardCore.ToDos/Services` and currently maps Orchard `User` records to `MarkBartha.HarvestDemo.Domain.Models.UserProfile`. The store is registered in `Startup` for dependency injection.
- Shared domain models (e.g., `UserProfile`, `TodoItem`) live under `src/Shared/MarkBartha.HarvestDemo.Domain`. Properties are initialized with empty strings to avoid nullable warnings in API payloads.

## MAUI Client Notes (2025-11-03)
- HTTP access funnels through `AuthenticatedHttpClient`, which injects the bearer token retrieved from `ExternalAuthenticationStateProvider`.
- Todo functionality is split between `ITodoService` (implemented by `TodoApiService`) and `TodoState`, which keeps an in-memory cache plus concurrency control through an internal semaphore.
- User identity in the UI no longer reads JWT claims. Instead, `IUserProfileService` (`UserProfileApiService`) calls `GET /api/user-profile`, and `UserProfileState` caches/refreshes the data. Components access the state via DI and listen to its `StateChanged` event (e.g., `MenuPanel.razor`).
- When adding new screens that need the profile, call `await UserProfileState.EnsureLoadedAsync(ct)` to avoid duplicate fetches. `UserProfileState.ReloadAsync` is already protected against concurrent calls.
- Nullable warnings remain in several MAUI files (notably `TodoState`, `TodoServiceException`, and Razor components) and should be cleaned up before enabling warnings-as-errors.
