# Changelog

## 2026-10-01

- Added a runnable synthetic dealer order / vehicle stock mock system: `DealerOrders.Api` (minimal API: `PUT/GET /api/orders/{orderId}`, `/health`), in-memory storage, typed inventory HTTP client with an in-process simulator, validation and mapping.
- Added `tests/DealerOrders.Api.Tests` (53 xUnit unit and in-process HTTP integration tests) and a single solution `DealerOrders.slnx` pinned to SDK 10.0.401 / `net10.0`.
- Added `AGENTS.md`, expanded `README.md`, `.editorconfig` and `.gitignore`.

