# AGENTS.md

Context for AI agents working in this repository. Synthetic dealer order / vehicle stock mock system; see `README.md` for purpose, API and limitations.

## Structure

- `DealerOrders.slnx` - the single solution (repo root).
- `global.json`, `Directory.Build.props` - SDK 10.0.401 (rollForward latestFeature), `net10.0`, nullable, implicit usings, warnings as errors.
- `src/DealerOrders.Api/` - ASP.NET Core minimal-API host (`Program.cs`) plus folders by responsibility:
  `Contracts` (JSON DTOs), `Domain`, `Validation`, `Mapping`, `Storage` (`IOrderRepository`, in-memory), `Inventory` (`IInventoryClient`, typed `HttpClient`, simulator, DI extensions), `Services` (`OrderUpdateService`, orchestration).
- `tests/DealerOrders.Api.Tests/` - xUnit: `Unit/` (validator, mapper, service, HTTP client, repository) and `Integration/` (full HTTP flow via `WebApplicationFactory<Program>` with a stub `HttpMessageHandler` for the inventory system).

## Commands (run from repo root)

```powershell
dotnet restore
dotnet build --no-restore --nologo
dotnet test --no-restore --nologo
dotnet run --project src/DealerOrders.Api --urls http://localhost:5080
```

## Conventions

- File-scoped namespaces, `.editorconfig` at root, warnings are errors.
- Public async methods take and forward a `CancellationToken`; `OperationCanceledException` from the caller is never converted into a failure result.
- Structured logging with message templates (`ILogger<T>`); no secrets or real data.
- External HTTP goes through a typed client behind an interface; failures surface as `InventoryUnavailableException`.
- Validation returns field-keyed error dictionaries; the service returns an `OrderUpdateResult` and the endpoint maps it to HTTP status codes.
- Tests must not call real external systems or depend on real time/sleep; use `FixedTimeProvider`, `FakeInventoryClient` and `StubInventoryHandler` (`TestData.cs`, `TestDoubles.cs`).
- Each operation has happy and unhappy path tests. All data is synthetic (e.g. VIN `TESTVEH0000000001`; real VINs never contain I, O or Q).

