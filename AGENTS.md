# Repository Guidelines

## Project Structure & Module Organization
The solution `MarkBartha.HarvestDemo.sln` groups the Orchard CMS host, theme, and shared asset pipeline. `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` hosts Orchard Core; it owns `Program.cs`, deployment config, and runtime `App_Data` (treat `logs/` and `Sites/` as generated state). `src/Backend/MarkBartha.HarvestDemo.OrchardCore.Theme` holds Razor views, theme-specific `Assets/`, and the compiled `wwwroot/`. Shared styling lives in `src/Shared/MarkBartha.HarvestDemo.Assets`, which runs the Tailwind build and vendored libraries via LibMan.

## Build, Test, and Development Commands
- `dotnet restore MarkBartha.HarvestDemo.sln` - pull NuGet dependencies.
- `dotnet tool restore` - install local CLI tools (`tailwind`, `libman`) defined in `.config/dotnet-tools.json`.
- `dotnet build MarkBartha.HarvestDemo.sln` - compile the host and theme; invokes Tailwind and asset copy targets automatically.
- `dotnet run --project src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` - start the CMS locally (add `--launch-profile Development` when debugging HTTPS).
- `dotnet watch run --project src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web` - hot-reload Razor and C# changes using the watcher include/exclude rules.
- `dotnet tool run libman restore` - refresh `wwwroot/vendors/` when `libman.json` changes.

## Coding Style & Naming Conventions
Use the default .NET style: four-space indents, braces on new lines, PascalCase for types and methods, and camelCase for locals. Keep asynchronous APIs suffixed with `Async`, and prefer Orchard Core shape names that mirror their Razor file paths. Run `dotnet format` before submitting to keep nullable-enabled code clean and consistent.

## Asset Pipeline Notes
Theme builds call `CopyAssets.targets` and `RunTailwindBuild.targets`, so place raw CSS in `Assets/css` and shared styles in `src/Shared/.../Assets`. Shared libraries from `src/Shared/.../wwwroot` (e.g., `vendors/lucide/dist/cjs/lucide.min.js`) are copied by the same target into each consumer’s `wwwroot/`; override `SharedPublishedAssetsDestinationFolder` if a project needs a different static root (the future MAUI app will point this at its hybrid `wwwroot`). If a change touches Tailwind configuration, run `dotnet tailwind exec -i ./Assets/css/app.css -o ./wwwroot/css/app.css` manually to verify the output. Do not edit files under `wwwroot/` directly; treat them as generated artifacts.

## Testing Guidelines
No automated test project ships yet; new features should include focused unit or integration tests alongside the feature. Prefer xUnit and locate future test projects under `tests/` matching the project under test (for example, `tests/MarkBartha.HarvestDemo.OrchardCore.Web.Tests`). Until the harness lands, document manual validation steps in the PR and attach relevant Orchard recipes to seed data when needed.

## Commit & Pull Request Guidelines
Write commit subjects in the imperative mood (e.g., `Add tenant setup recipe`), and keep the first line under 72 characters. Each pull request should link to its issue, describe user-visible changes, and include screenshots or screencasts for UI updates. Confirm `dotnet build`, `dotnet tailwind exec`, and local Orchard startup succeed before requesting review, and mention any follow-up work in the PR notes.
