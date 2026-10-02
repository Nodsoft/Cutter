# Cutter

## Run locally with Aspire

Install .NET 10, the Aspire CLI, and Docker. Create a GitHub OAuth app with the callback URL `https://localhost:7022/callback/login/github`, then store its credentials in the AppHost's user secrets:

```powershell
dotnet user-secrets set "Parameters:github-client-id" "<client-id>" --project Nodsoft.Cutter.AppHost/Nodsoft.Cutter.AppHost.csproj
dotnet user-secrets set "Parameters:github-client-secret" "<client-secret>" --project Nodsoft.Cutter.AppHost/Nodsoft.Cutter.AppHost.csproj
aspire start --apphost Nodsoft.Cutter.AppHost/Nodsoft.Cutter.AppHost.csproj --non-interactive
```

Aspire starts PostgreSQL, creates the `cutter_dev` database in a Docker volume, and starts the server after the database is ready. The server applies its EF Core migrations before accepting requests. The Aspire database is separate from any PostgreSQL instance started outside the AppHost.
