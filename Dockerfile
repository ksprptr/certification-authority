# syntax=docker/dockerfile:1

# The app targets net7.0 — keep the SDK, the runtime and dotnet-ef on the same major.
ARG DOTNET_VERSION=7.0
# Debian 12. The default net7.0 images are Debian 11, whose apt repositories are past
# end of life and no longer serve the security pool.
ARG RUNTIME_TAG=7.0-bookworm-slim

# --------------------------------------------------------------------------------------
# build — restore, compile and publish the Blazor Server app
# --------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG TARGETARCH
WORKDIR /src

# Restore first, on its own layer, so a source-only change does not re-download NuGet.
# Also restore for the concrete runtime identifier: the migrations bundle is published
# against a RID and fails with NETSDK1112 if that runtime pack was never fetched.
COPY CABlazorApp/CABlazorApp.csproj CABlazorApp/
RUN case "$TARGETARCH" in \
        amd64) echo linux-x64 ;; \
        arm64) echo linux-arm64 ;; \
        arm)   echo linux-arm ;; \
        *)     echo "unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac > /rid \
 && dotnet restore CABlazorApp/CABlazorApp.csproj \
 && dotnet restore CABlazorApp/CABlazorApp.csproj -r "$(cat /rid)"

COPY . .
RUN dotnet publish CABlazorApp/CABlazorApp.csproj \
        -c Release \
        -o /out \
        --no-restore \
        /p:UseAppHost=false

# --------------------------------------------------------------------------------------
# skia — fetch the native SkiaSharp library the Aspose packages forget to ship
# --------------------------------------------------------------------------------------
# Aspose.Cells draws through SkiaSharp, but the Aspose packages only pull in
# SkiaSharp.NativeAssets.Win32 and .macOS — there is no Linux native asset in the graph, on
# any architecture. Without libSkiaSharp.so next to the app, signing an .xls/.xlsx throws
# DllNotFoundException. Resolve the version from the published deps.json so this cannot
# drift away from the SkiaSharp the app actually loads, and pick the build for the
# architecture being targeted.
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS skia
ARG TARGETARCH
WORKDIR /skia
COPY --from=build /out/CABlazorApp.deps.json .
RUN set -eu; \
    version="$(grep -o '"SkiaSharp/[0-9][0-9A-Za-z.-]*"' CABlazorApp.deps.json | head -1 | sed 's|"SkiaSharp/||; s|"||')"; \
    [ -n "$version" ] || { echo "SkiaSharp not found in deps.json" >&2; exit 1; }; \
    case "$TARGETARCH" in \
        amd64) rid=linux-x64 ;; \
        arm64) rid=linux-arm64 ;; \
        arm)   rid=linux-arm ;; \
        *)     echo "unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac; \
    echo "SkiaSharp $version for $rid"; \
    printf '%s' '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net7.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="'"$version"'" /></ItemGroup></Project>' > skia.csproj; \
    dotnet restore skia.csproj; \
    cp "$(find /root/.nuget/packages/skiasharp.nativeassets.linux -path "*/runtimes/$rid/native/libSkiaSharp.so" | head -1)" /libSkiaSharp.so

# --------------------------------------------------------------------------------------
# bundle — turn the EF Core migrations into a single executable
# --------------------------------------------------------------------------------------
# Program.cs never calls Migrate(), so the schema has to be applied from the outside.
# `migrations bundle` packs Data/Migrations into one binary that takes a connection string,
# which keeps the SDK out of the image that actually runs against the database.
FROM build AS bundle
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN mkdir -p /fd \
 && dotnet tool install --global dotnet-ef --version 7.0.* \
 && dotnet ef migrations bundle \
        --project CABlazorApp/CABlazorApp.csproj \
        --startup-project CABlazorApp/CABlazorApp.csproj \
        --context ApplicationDbContext \
        --configuration Release \
        --target-runtime "$(cat /rid)" \
        --force \
        --output /fd/efbundle

# --------------------------------------------------------------------------------------
# migrate — one-shot container that brings the database up to the latest migration
# --------------------------------------------------------------------------------------
# Framework-dependent, on the same base as the runtime stage — the bundle is ~250 MB of
# app dependencies, and a self-contained one would duplicate the whole .NET runtime on top
# of that. Sharing the base means the two images share every layer but the bundle itself.
FROM mcr.microsoft.com/dotnet/aspnet:${RUNTIME_TAG} AS migrate
WORKDIR /app
# The user is created before the COPYs so ownership and mode can be set by COPY itself.
# A `chmod`/`chown` in a RUN would rewrite the 250 MB bundle into a second layer, and the
# image would carry it twice.
RUN useradd --system --create-home --home-dir /home/app --shell /usr/sbin/nologin app
COPY --from=bundle --chown=app:app --chmod=755 /fd/efbundle ./efbundle
COPY --chown=app:app --chmod=755 docker/migrate-entrypoint.sh ./migrate-entrypoint.sh
USER app
ENTRYPOINT ["./migrate-entrypoint.sh"]

# --------------------------------------------------------------------------------------
# runtime — the image that serves the app
# --------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:${RUNTIME_TAG} AS runtime

# Aspose (PDF / Words / Cells / Slides) rasterises and measures text when it rewrites a
# document's metadata, so the image needs fontconfig and at least one real font family.
# curl is here only for the healthcheck.
RUN apt-get update \
 && apt-get install --no-install-recommends -y \
        curl \
        fontconfig \
        libfontconfig1 \
        libfreetype6 \
        libgdiplus \
        fonts-dejavu-core \
        fonts-liberation \
 && rm -rf /var/lib/apt/lists/*

# A home directory of its own: ASP.NET Core Data Protection writes its key ring to
# $HOME/.aspnet/DataProtection-Keys, and that path is a volume in compose — without it
# every restart invalidates all auth cookies and every open Blazor circuit.
RUN useradd --system --create-home --home-dir /home/app --shell /usr/sbin/nologin app
ENV HOME=/home/app

WORKDIR /app
COPY --from=build /out ./
COPY --from=skia /libSkiaSharp.so ./libSkiaSharp.so

# Signed files and generated certificates land in <ContentRoot>/Archive. Create it in the
# image so the named volume inherits the right owner instead of root.
RUN mkdir -p /app/Archive /home/app/.aspnet/DataProtection-Keys \
 && chown -R app:app /app/Archive /home/app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_gcServer=0

EXPOSE 8080
USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -fsS http://127.0.0.1:8080/ -o /dev/null || exit 1

ENTRYPOINT ["dotnet", "CABlazorApp.dll"]
