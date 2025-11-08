# Alpine images lack glibc, which the Tailwind CLI binary shipped with tailwindcss-dotnet requires.
# Stick to the default Debian-based dotnet images for compatibility unless explicitly overridden.
ARG DOTNET_SDK_VERSION=8.0
ARG DOTNET_RUNTIME_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build
WORKDIR /src

# copy project files for better restore layer caching
COPY MarkBartha.HarvestDemo.sln ./
COPY Directory.Build.props ./
COPY .config/dotnet-tools.json ./.config/dotnet-tools.json
COPY src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web/MarkBartha.HarvestDemo.OrchardCore.Web.csproj src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web/
COPY src/Backend/Modules/MarkBartha.HarvestDemo.OrchardCore.ToDos/MarkBartha.HarvestDemo.OrchardCore.ToDos.csproj src/Backend/Modules/MarkBartha.HarvestDemo.OrchardCore.ToDos/
COPY src/Backend/Themes/MarkBartha.HarvestDemo.OrchardCore.Theme/MarkBartha.HarvestDemo.OrchardCore.Theme.csproj src/Backend/Themes/MarkBartha.HarvestDemo.OrchardCore.Theme/
COPY src/Shared/MarkBartha.HarvestDemo.Assets/MarkBartha.HarvestDemo.Assets.csproj src/Shared/MarkBartha.HarvestDemo.Assets/
COPY src/Shared/MarkBartha.HarvestDemo.Domain/MarkBartha.HarvestDemo.Domain.csproj src/Shared/MarkBartha.HarvestDemo.Domain/

RUN dotnet tool restore
RUN dotnet restore src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web/MarkBartha.HarvestDemo.OrchardCore.Web.csproj

# copy the remaining sources
COPY . .
RUN dotnet publish src/Backend/MarkBartha.HarvestDemo.OrchardCore.Web/MarkBartha.HarvestDemo.OrchardCore.Web.csproj -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_RUNTIME_VERSION}
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080

ENTRYPOINT [ "dotnet", "MarkBartha.HarvestDemo.OrchardCore.Web.dll" ]
