# Architecture & Design Decisions

This document explains the reasoning behind the key backend mechanisms in BattleArena — written for anyone (recruiter, interviewer, or future me) who wants to understand *why* something was built a certain way, not just *what* it does.

---

## 1. Server-side Score Validation

**Problem:** A client could submit an arbitrarily high score to inflate leaderboard rankings.

**Solution:** The server records a `StartedAt` timestamp (UTC) when a match begins (`POST /api/v1/matches/start`). When the client submits a score, `MatchService.SubmitScoreAsync` computes `elapsed = (now - StartedAt).TotalSeconds` and rejects the score if `score / elapsed > MaxScorePerSecond` (configurable in `appsettings.json`, default 50). Matches that have already ended (`EndedAt != null`) are also rejected. Only validated scores are persisted and forwarded to the Redis leaderboard.

## 2. Refresh Token Flow with SHA-256 Hashing

**Problem:** Storing refresh tokens in plaintext means a database breach directly exposes every active session.

**Solution:** `AuthService` generates a 64-byte cryptographically random token (`RandomNumberGenerator`), returns the plaintext to the client, but stores only its SHA-256 hash in PostgreSQL. On refresh, the incoming plaintext is hashed and compared against the stored hash. Tokens have a 7-day expiry and an `IsRevoked` flag for explicit logout. The `TokenHash` column has a unique index for O(1) lookup.

## 3. Global Exception Handling (RFC 7807)

**Problem:** Unhandled exceptions could leak stack traces or return inconsistent error shapes across endpoints.

**Solution:** A centralized `GlobalExceptionHandler` (implementing `IExceptionHandler`) catches all exceptions. Business exceptions extend an abstract `AppException` base class that declares a `StatusCode` and `Title`; these are mapped to their declared HTTP status (400, 403, 404, 409) without being logged as errors. Unexpected exceptions fall back to a generic 500 and are logged with request context. All error responses use the standard `ProblemDetails` format.

## 4. Transactional In-Game Purchases with Audit Log

**Problem:** Deducting gold, updating inventory, and recording history are three separate writes — a partial failure could leave the database in an inconsistent state (e.g. gold deducted but item not added).

**Solution:** `ShopService.BuyAsync` wraps the entire operation in an explicit `BeginTransactionAsync` / `CommitAsync` block. The `PurchaseHistory` row (which snapshots the total price at purchase time) is inserted within the same transaction, so it rolls back together with the gold/inventory changes on any failure. Check constraints in PostgreSQL enforce `Quantity > 0` and `Price >= 0` at the database level.

## 5. API Versioning + Pagination

**Problem:** API evolution could break existing clients; unbounded result sets could degrade performance.

**Solution:** URL-segment versioning (`/api/v{version}/...`) via `Asp.Versioning`, with a `ConfigureSwaggerOptions` class that auto-generates one Swagger document per version. Pagination uses a reusable `PagedRequest` / `PagedResult<T>` pattern with server-side validation (max page size 100, enforced via `BadRequestException`). The response envelope includes `TotalCount`, `Page`, `PageSize`, and computed `TotalPages`.

## 6. Redis Backplane for SignalR Multi-Instance

**Problem:** Running multiple server instances behind a load balancer would cause SignalR messages to be delivered only to clients connected to the same instance.

**Solution:** SignalR is configured with `.AddStackExchangeRedis()` using a dedicated channel prefix (`BattleArenaBackendAPI`). This ensures chat messages published by any instance are relayed to all others via Redis Pub/Sub.

## 7. Chat Rate Limiting

**Problem:** A single client could flood the global chat with messages.

**Solution:** `GlobalChatHub` uses Redis `INCR` + `EXPIRE` to count messages per connection within a 10-second sliding window (max 5 messages). The TTL is set only on the first increment to avoid resetting the window timer on each message. Exceeding the limit throws a `HubException` back to the client.

---

## Project Structure

```
BattleArenaBackendAPI/
├── Configuration/
│   └── ConfigureSwaggerOptions.cs      # Per-version Swagger doc generator
├── Controllers/
│   ├── AuthController.cs               # Register, Login, Refresh, Logout
│   ├── ClaimsPrincipalExtensions.cs     # JWT claim extraction helper
│   ├── InventoryController.cs          # Get user inventory (paginated)
│   ├── LeaderboardController.cs        # Submit score, Get top 10
│   ├── MatchController.cs              # Start match
│   └── ShopController.cs              # Browse items, Buy item
├── Data/
│   ├── AppDbContext.cs                 # EF Core context with Fluent API config
│   ├── AppDbContextFactory.cs          # Design-time factory for EF migrations
│   └── DbSeeder.cs                    # Idempotent seed data (13 shop items)
├── DTOs/
├── Exceptions/
│   ├── AppException.cs                 # Abstract base (StatusCode + Title)
│   ├── BadRequestException.cs / ConflictException.cs / ForbiddenException.cs / NotFoundException.cs
├── Hubs/
│   └── GlobalChatHub.cs               # SignalR hub with Redis rate limiting
├── Middleware/
│   └── GlobalExceptionHandler.cs       # IExceptionHandler → ProblemDetails
├── Migrations/                         # EF Core migration history
├── Models/
│   ├── Item.cs / Match.cs / PurchaseHistory.cs / RefreshToken.cs / User.cs / UserInventory.cs
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── ILeaderboardService.cs / LeaderboardService.cs
│   ├── IMatchService.cs / MatchService.cs
│   └── IShopService.cs / ShopService.cs
├── wwwroot/                            # Minimal demo frontend
├── Program.cs                          # Composition root & middleware pipeline
├── appsettings.json                    # Connection strings, JWT config, GameSettings
└── BattleArenaBackendAPI.csproj
```

## Full API Reference

### Authentication (`/api/v1/auth`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/register` | No | Create a new user account (starts with 1000 gold) |
| `POST` | `/login` | No | Authenticate and receive access token + refresh token |
| `POST` | `/refresh` | No | Exchange a valid refresh token for a new access token |
| `POST` | `/logout` | No | Revoke a refresh token |

### Shop (`/api/v1/shop`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `GET` | `/items?page=1&pageSize=20` | No | Browse shop items (paginated) |
| `POST` | `/buy` | Yes | Purchase an item (deducts gold, updates inventory, logs to PurchaseHistory) |

### Inventory (`/api/v1/inventory`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `GET` | `/?page=1&pageSize=20` | Yes | View the authenticated user's inventory (paginated) |

### Match & Leaderboard

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/v1/matches/start` | Yes | Start a new match (records server-side timestamp) |
| `POST` | `/api/v1/leaderboard/score` | Yes | Submit score for a match (validated against elapsed time) |
| `GET` | `/api/v1/leaderboard/top` | No | Retrieve top 10 leaderboard entries from Redis |

### Real-time

| Endpoint | Description |
|----------|-------------|
| `/hub/global-chat` | WebSocket hub for global chat. Supports JWT auth via `access_token` query string. |