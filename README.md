# Cutter

## Run locally with Aspire

Install .NET 10, Node.js 22 or newer (including npm), the Aspire CLI, and Docker. On Windows ARM64, use the ARM64 Node.js installer and make sure `node --version` and `npm --version` work in your terminal. Create a GitHub OAuth app with the callback URL `https://localhost:7022/callback/login/github`, then store its credentials in the AppHost's user secrets:

```powershell
dotnet user-secrets set "Parameters:github-client-id" "<client-id>" --project Nodsoft.Cutter.AppHost/Nodsoft.Cutter.AppHost.csproj
dotnet user-secrets set "Parameters:github-client-secret" "<client-secret>" --project Nodsoft.Cutter.AppHost/Nodsoft.Cutter.AppHost.csproj
aspire start --apphost Nodsoft.Cutter.AppHost/Nodsoft.Cutter.AppHost.csproj --non-interactive
```

Aspire starts PostgreSQL, creates the `cutter_dev` database in a Docker volume, and starts the server after the database is ready. The server applies its EF Core migrations before accepting requests. The Aspire database is separate from any PostgreSQL instance started outside the AppHost.

## Tailwind CSS

The server uses the official Tailwind CSS v4 npm CLI, which supports native Windows ARM64. `Styles/app.css` imports Tailwind and scans `Components` (including `.razor` and C# code-behind files). Use complete utility names in Razor/C# rather than building names with string interpolation.

The server's MSBuild targets automatically run `npm ci` when the locked dependencies are missing or changed, then compile `wwwroot/css/app.css` before static web assets are collected. Aspire uses these same targets when building its server resource. Standard .NET commands need no separate frontend build step:

```powershell
dotnet build Nodsoft.Cutter.slnx
dotnet publish Nodsoft.Cutter.Server/Nodsoft.Cutter.Server.csproj -c Release -o artifacts/server
```

Release builds minify the stylesheet. Build machines need Node.js/npm and registry access for the first dependency restore; the published server only needs the .NET runtime. If npm is outside `PATH`, set the MSBuild `NpmExecutable` property to its launcher path. After switching Node.js architectures, run `npm ci --include=dev --include=optional` to reinstall the matching native dependencies. `dotnet publish --no-build` uses the CSS from the preceding build, so rebuild after changing utility classes.

For CSS updates while editing components, run the watcher in another terminal alongside Aspire or `dotnet watch`:

```powershell
cd Nodsoft.Cutter.Server
npm ci --include=dev --include=optional
npm run watch:css
```

The watcher uses polling so it also works on Windows ARM64 without a native filesystem watcher. Generated CSS and `node_modules` are ignored by Git. Tailwind dependencies are pinned in `package.json` and `package-lock.json`; update both together with npm. The old Sass files are retained as reference for the planned frontend revamp, but are no longer compiled or published, and the Bootstrap Sass/LibMan build dependencies have been removed. Existing Bootstrap-based markup will need migration to Tailwind.
