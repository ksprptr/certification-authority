# Certification Authority

> A self-hosted certification authority. Generate an RSA key pair, sign a document with it, hand
> the signature to someone else and let them verify it — and keep every certificate and every
> signed file in a personal archive.

- [Features](#features)
- [Project](#project)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Run](#run)
- [How it works](#how-it-works)
- [Format support](#format-support)
- [Footprint](#footprint)
- [Deployment](#deployment)
- [License](#license)

## Features

- Generate a self-signed 4096-bit RSA certificate, valid for ten years, and download it as a zip
  holding the password-protected `.pfx` and the public `.crt`
- Sign a document with that `.pfx`. The signature is SHA-512 / PKCS#1, and it is written **into the
  document itself** — as XMP metadata for PDF, a custom document property for Office files, a
  trailing block for `.txt` and `.csv` — so the file that comes back is still a normal, openable
  file
- Verify a signed document against the public certificate, by uploading the signed copy, the
  original and the `.crt`
- Documents the UI accepts: `.pdf`, `.doc(x|m)`, `.xls(x|m)`, `.ppt(x|m)`, `.csv`, `.txt` — not all
  of which survive a Linux container, see [Format support](#format-support). Certificates:
  `.pfx`, `.p12`, `.crt`, `.cer`, `.pem`, `.p7b`, `.p7c`, `.p7s`
- A per-user archive of generated certificates and signed files, each one re-downloadable or
  deletable
- Accounts are ASP.NET Core Identity — registration with email confirmation, password reset,
  two-factor with an authenticator app, recovery codes, and personal-data export or deletion
- The UI is Czech throughout
- In Docker: a non-root container, a database reachable only over the compose network, a persisted
  Data Protection key ring so a restart doesn't sign everyone out, and a compose file that deploys
  to Coolify unmodified

## Project

| Name                                            | Description                                                                |
| ----------------------------------------------- | -------------------------------------------------------------------------- |
| [CABlazorApp](./CABlazorApp/)                   | The whole app — a .NET 7 Blazor Server project                             |
| [Services](./CABlazorApp/Services/)             | Key generation, signing, verification, and the per-format metadata writers |
| [Areas/Identity](./CABlazorApp/Areas/Identity/) | The scaffolded, Czech-translated Identity UI                               |

## Prerequisites

- [Docker](https://docs.docker.com/get-started/get-docker/) — the only thing you need, and the
  simplest way to run the whole stack. Both amd64 and arm64 work
- [.NET SDK 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) — only for running the app
  natively, outside of Docker

## Setup

Only needed to run locally — on Coolify the same variables go in the resource's environment tab.

```bash
cp .env.example .env
```

Every variable is documented inline in that file. The one that has no default is the database
password:

```bash
openssl rand -base64 24   # SERVICE_PASSWORD_MSSQL
```

SQL Server enforces its own password policy — at least eight characters drawn from three of
{uppercase, lowercase, digit, symbol} — and refuses to start if the password is weaker.

## Run

Everything in Docker (recommended):

```bash
docker compose up -d --build   # http://localhost:5080
docker compose logs -f ca-app
docker compose down            # volumes keep your data
```

Migrations run automatically — the one-shot `ca-migrate` service applies them and the app waits for
it to exit cleanly.

The host port comes from `docker-compose.override.yml`, which Compose merges in
automatically and Coolify never reads.

Natively, with only the database in Docker — uncomment the `ca-db` ports block in
`docker-compose.override.yml` first:

```bash
docker compose up -d ca-db

export ConnectionStrings__DefaultConnection='Server=127.0.0.1,1433;Database=CABlazorApp;User Id=sa;Password=<SERVICE_PASSWORD_MSSQL>;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
dotnet tool install --global dotnet-ef --version 7.0.*
dotnet ef database update --project CABlazorApp --context ApplicationDbContext

dotnet run --project CABlazorApp   # https://localhost:7080, http://localhost:5135
```

Checks:

```bash
dotnet build
```

## How it works

| Route                        | What happens                                                      |
| ---------------------------- | ----------------------------------------------------------------- |
| `/`                          | The landing page                                                  |
| `/generate`                  | Generate, sign and verify — three forms, signed in only           |
| `/archiv`                    | Everything you have generated or signed, with download and delete |
| `/Identity/Account/*`        | Register, sign in, two-factor, account management                 |
| `/Identity/Account/Manage/*` | Password, email, authenticator app, personal data                 |

Signing never uploads a private key anywhere it is kept: the `.pfx` is read into memory, the RSA
key signs the bytes, and only the signature and the rewritten document are stored. Verification is
the mirror image — the signature is pulled back out of the document's metadata and checked against
the public key in the `.crt`.

Each account gets its own directory under `<ContentRoot>/Archive/<email>/`, split into
`certificates/` and `files/`, with a `temp/` used while a zip or a verification is being assembled.
In Docker that tree is the `ca-archive` volume, so it outlives the container.

Registration requires a confirmed email, and the app ships no mail sender — the confirmation link
is printed straight onto the page after you register, which is enough for a single-operator
deployment. Wire up an `IEmailSender` if you need the real thing.

## Format support

Signing rewrites the document to carry the signature, and for everything except `.txt` and `.csv`
that rewrite goes through Aspose. The Aspose versions this project pins predate ARM, and they ship
no Linux native assets at all — so what works depends on the format and on the machine:

| Format          | Library       | linux/amd64 | linux/arm64 |
| --------------- | ------------- | ----------- | ----------- |
| `.txt`, `.csv`  | none          | yes         | yes         |
| `.doc`, `.docx` | Aspose.Words  | yes         | yes         |
| `.xls`, `.xlsx` | Aspose.Cells  | yes¹        | yes¹        |
| `.pdf`          | Aspose.PDF    | no²         | no³         |
| `.ppt`, `.pptx` | Aspose.Slides | yes         | no⁴         |

1. Only because the image supplies `libSkiaSharp.so` itself. The Aspose packages pull in the
   Windows and macOS SkiaSharp natives and nothing for Linux, so a stock publish throws
   `DllNotFoundException` here on **any** Linux. The `skia` stage in the Dockerfile resolves the
   right version out of the published `deps.json` and drops the matching build next to the app.
2. Aspose.PDF 23.5 depends on `System.Drawing.Common` 7.0.0, which no longer supports Unix at all
   — the 6.x `System.Drawing.EnableUnixSupport` switch was removed. Downgrading that package to
   6.0.0 fixes it on amd64.
3. Even with `System.Drawing.Common` 6.0.0 and libgdiplus in place, Aspose.PDF 23.5 can open a PDF
   on arm64 but throws on save.
4. Aspose.Slides 23.5 ships exactly one Linux native,
   `libaspose.slides.drawing.capi_x86_64_libstdcpp_libc2.23.so`. There is no arm64 build of it.

Nothing above is a packaging problem that the Dockerfile can solve — the fix is in the project
file, and this repository's Docker setup deliberately leaves the app alone. Newer Aspose releases
do handle arm64: `Aspose.PDF.Drawing` 25.7 signs PDFs on arm64 with no `System.Drawing` at all, and
Aspose.Slides 25.7 dropped the native library and is pure managed. Bumping those two references is
the one change that would make every format work everywhere.

Unlicensed, Aspose also runs in evaluation mode: a watermark on the documents it saves, and a row
cap in Aspose.Cells. `.txt` and `.csv` go through the app's own code and are unaffected.

## Footprint

Measured on a fresh arm64 deploy, with shared image layers counted once:

| What                   | Size    | Grows?                                                                                                                                      |
| ---------------------- | ------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `azure-sql-edge` image | 2.5 GB  | no                                                                                                                                          |
| app image              | 1.0 GB  | no — 350 MB of it is shared with the migrator                                                                                               |
| migrator image         | 372 MB  | no, and it only runs at deploy time                                                                                                         |
| `ca-db-data` volume    | ~92 MB  | barely — Identity rows are bytes, and Azure SQL Edge defaults every database to SIMPLE recovery, so the transaction log does not accumulate |
| `ca-keys` volume       | ~1 KB   | no                                                                                                                                          |
| `/app/Archive`         | 0       | never touches the disk — see below                                                                                                          |
| container logs         | ≤ 90 MB | capped, 10 MB × 3 files × 3 services                                                                                                        |

So roughly **3.9 GB of images and under 100 MB of data**, and it stays there.

The one thing that would have grown without bound is the archive: anyone who registers can
upload, uploads go up to 32 MB each, and nothing in the app ever deletes. It is mounted as a
RAM-backed `tmpfs` capped at 128 MB (`ARCHIVE_SIZE`) instead of a volume — uploads work, the
archive page works, a write past the cap fails rather than filling anything up, and a restart
clears it. For a showcase that is the point; if you want it to persist, swap the `tmpfs` block
in `docker-compose.yml` for the `ca-archive` volume the comment there describes.

SQL Server sizes its buffer pool against the whole host unless told otherwise, so it is capped
at 1 GB (`MSSQL_MEMORY_LIMIT_MB`); it settles around 380 MB in practice, and the app around
55 MB.

Redeploys leave build cache behind on the server. Coolify can prune it on a schedule, or
`docker builder prune` by hand.

## Deployment

| Description | Values                  |
| ----------- | ----------------------- |
| **Server:** | Coolify                 |
| **Type:**   | Docker Compose          |
| **URL:**    | https://ca.ksprptr.dev/ |

`docker-compose.yml` is written to deploy as-is:

1. New resource → **Docker Compose**, pointed at this repository, with `docker-compose.yml` as the
   compose file.
2. Attach your domain to the **ca-app** service. Coolify sees `SERVICE_FQDN_CAAPP_8080` in that
   service and generates the proxy configuration for container port 8080 from it.
3. `SERVICE_PASSWORD_MSSQL` is generated by Coolify — the `SERVICE_PASSWORD_` prefix is what asks
   it to. Nothing else has to be set.
4. Deploy. The migration container runs first and the app only starts once it has exited cleanly.

`ca-app` publishes no host port; the proxy reaches it over the Docker network, and the database is
not reachable from outside the compose network at all. `ASPNETCORE_FORWARDEDHEADERS_ENABLED` is on,
so the app sees the real scheme and client address behind the proxy.

**It has to be a host of its own, not a sub-path.** `_Layout.cshtml` hardcodes
`<base href="~/">` and nothing calls `UsePathBase`, so under `example.com/certification-authority`
the page would ask for `/site.css` and `/_blazor` at the domain root and never load. One line of
`app.UsePathBase(...)` in `Program.cs` would fix it — this repository deliberately leaves the app
untouched, so a subdomain it is.

Keep it one label deep. Behind Cloudflare, Universal SSL covers `example.com` and `*.example.com`
and stops there; `ca.archive.example.com` is two levels and needs Advanced Certificate Manager.

Blazor Server is a persistent WebSocket, not a request/response app. Cloudflare proxies WebSockets
on every plan and SignalR's keep-alive holds the connection open, so nothing needs configuring —
but if the UI ever goes dead after a while, that connection is the first thing to look at.

The three volumes are the state worth keeping: `ca-db-data` (the database), `ca-archive`
(certificates and signed files) and `ca-keys` (the Data Protection key ring — lose it and every
session cookie is invalidated).

If Coolify's generated password ever trips SQL Server's complexity rule, set
`SERVICE_PASSWORD_MSSQL` by hand in the environment tab and redeploy.

To put the app in front of a managed SQL Server instead of the bundled container, set
`CONNECTION_STRING` and drop the `ca-db` service.

## License

> This software is developed by **Petr Kašpar** and is licensed under the MIT License.  
> For more details, please refer to the [LICENSE](./LICENSE) file.
