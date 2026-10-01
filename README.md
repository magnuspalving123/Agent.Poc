# Agent.Poc

Small, runnable **synthetic** dealer order / vehicle stock integration, used as a realistic C# codebase
for testing coding agents (existing conventions, tests, bounded maintenance tasks). It is not a copy of any
production system; all contracts, statuses and values are invented.

## What it does

`DealerOrders.Api` is a single ASP.NET Core minimal-API host that:

1. receives a dealer order update (`PUT /api/orders/{orderId}`),
2. validates the contract,
3. maps it to an internal `DealerOrder`,
4. sends a stock update to an external inventory system through a typed `HttpClient` (`IInventoryClient`),
5. stores the order in memory, and
6. serves it back (`GET /api/orders/{orderId}`).

Status mapping to inventory stock status: `Created`/`Confirmed` -> `Reserved`, `Delivered` -> `InStock`, `Cancelled` -> `Released`.

## Build, run, test

Requires .NET SDK 10.0.401 or newer 10.0.x (pinned in `global.json`; a test-environment choice, not a BOS standard).

```powershell
dotnet restore
dotnet build --no-restore --nologo
dotnet test --no-restore --nologo
dotnet run --project src/DealerOrders.Api --urls http://localhost:5080
```

No Azure, cloud services, credentials or private feeds are needed. By default the host uses an in-process
simulated inventory system (`Inventory:Mode = Simulated`); `GET /simulated-inventory/stock-updates` shows what it received.
Set `Inventory:Mode=Http` and `Inventory:BaseUrl=<absolute url>` to call a real HTTP endpoint (`POST v1/stock-updates`).

## API examples

Request (`PUT /api/orders/ORD-1001`):

```json
{
  "orderId": "ORD-1001",
  "dealerId": "DLR-001",
  "status": "Delivered",
  "vehicle": { "vin": "TESTVEH0000000001", "model": "Synth Model A", "modelYear": 2025 },
  "updatedAt": "2026-01-15T10:00:00Z"
}
```

Response `200`:

```json
{
  "orderId": "ORD-1001",
  "dealerId": "DLR-001",
  "status": "Delivered",
  "vehicle": { "vin": "TESTVEH0000000001", "model": "Synth Model A", "modelYear": 2025 },
  "updatedAt": "2026-01-15T10:00:00+00:00",
  "processedAt": "2026-02-01T08:00:00+00:00"
}
```

Outbound stock update sent to the inventory system (`POST v1/stock-updates`):

```json
{ "vin": "TESTVEH0000000001", "dealerId": "DLR-001", "stockStatus": "InStock", "orderReference": "ORD-1001" }
```

## Error semantics (current behaviour)

| Situation | Result |
| --- | --- |
| Invalid or missing input, malformed JSON, route/body order id mismatch | `400` (validation problem details with per-field errors); nothing is sent or stored |
| Order not found on `GET` | `404` problem details |
| Inventory system returns non-success, is unreachable or times out | `502` problem details; the order is **not** stored (an earlier stored version is kept) |
| Caller cancels the request | cancellation is propagated; nothing is stored |

## Limitations

- Orders live in process memory and are lost on restart.
- Last write wins: no duplicate detection, no ordering by `updatedAt`, no status-transition rules.
- No retries: a single inventory failure fails the request.
- No authentication or authorization.
- The simulated inventory client always succeeds.

## Sources and assumptions

BOS-inspired (domain inspiration only, not current BOS business rules or contracts; validity of the sources was not determined):

- Order update / vehicle stock themes: `skill://bos/skills/bos-bygg-leveranse/references/inc0605320-create-vehiclestock-from-vehicledealerorderupdate--444335c938eb.md`
  and `...inc0622208-fjerne-ifs-bruktbilordre-fra-bos-api-vehicledeale--25d97a8d5d22.md` (titles only; both DRAFT).
- Public async methods take a `CancellationToken`, structured logging, no secrets: `skill://bos/skills/bos-code-review/references/skill--ba22f6e96aff.md` (original `skills\bos-code-review\SKILL.md`, lines 173-188).
- At least one happy and one unhappy path per operation: `skill://bos/skills/bos-testing-kvalitet/references/general-guidelines-for-testing--12570f7a16e9.md` (lines 47-55).

Choices made for this mock repository (not BOS requirements): .NET 10 / SDK 10.0.401, xUnit, minimal API, in-memory storage,
the field names and statuses above, the status-to-stock mapping, store-after-inventory-success ordering, and the 502 semantics.
No private source text or production data was copied.

