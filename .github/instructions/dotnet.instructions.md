---
applyTo: '3-APIs/kaffebar-dotnet*/**'
---

### .NET 10

- **Kaffebar starter:** in `3-APIs/kaffebar-dotnet/Kaffebar`, `dotnet run` serves http://localhost:5035 (`/scalar/v1`, `/openapi/v1.json`). There's no test project.
- **Kaffebar solution:** in `3-APIs/kaffebar-dotnet-solution`, `dotnet test` runs the xUnit + `WebApplicationFactory` tests. `Kaffebar.slnx` also includes `Oppgave9`.
  - Single test: `dotnet test --filter "FullyQualifiedName~CreateOrder_Returns201WithLocation"`
- **Consumer:** in `2-EventDriven/dotnet-consumer`, run `dotnet build DotnetEvents.slnx`, then `dotnet run --project Producer` and `dotnet run --project Consumer`.
  - Migrations: `dotnet ef migrations add <Name> --project Infrastructure --context IdemDbContext`
- The `NU1903` vulnerability warnings come from pinned transitive packages and were already there.

### .NET Kaffebar API

- Configure JSON in two places: `AddControllers().AddJsonOptions(...)` for controllers and `ConfigureHttpJsonOptions(...)` for Minimal APIs. Both need string enums, `WhenWritingNull` and `JsonNumberHandling.Strict`.
- `GET /menu` deliberately stays a Minimal API while orders use `OrdersController` (task 2). Don't unify them.
- `public partial class Program;` exists so the tests can use `WebApplicationFactory<Program>`.
- Tests call `IOrderRepository.Reset()` in their constructor because the in-memory store is shared between tests.
- Config lives in `Kaffebar:*` sections. Override it with environment variables such as `Kaffebar__Events__Enabled`.
- Entra ID auth (task 10) is off by default.

### .NET consumer

- `appsettings.json` lives at the solution root and is linked into each executable with `<None Include="../appsettings.json" Link="appsettings.json" CopyToOutputDirectory="PreserveNewest" />`.
- A new console app, such as `Consumer-2`, needs three things:
  - That `appsettings.json` link.
  - A `ProjectReference` to `../Common/Common.csproj`.
  - An entry in `DotnetEvents.slnx`.
- Connect with `ConnectionHelper.ConnectAsync()`. It reads `RabbitMQ:URI`, which you can override with the `RabbitMQ__URI` environment variable.
- EF Core reads its connection string from `POSTGRES_CONNECTION_STRING`. By default it uses port 5435.