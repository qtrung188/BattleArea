# BattleArena Backend API

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-Leaderboard%20%2B%20Backplane-DC382D?logo=redis&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--time%20Chat-512BD4?logo=dotnet&logoColor=white)

A personal portfolio project simulating the backend for a game arena system — built to practice production-grade backend patterns: JWT auth with refresh token rotation, server-side score validation, transactional purchases, real-time chat, and a Redis-backed leaderboard.

📖 **Design decisions and problem/solution write-ups → [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**

---

## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 (URL-based versioning) |
| Database | PostgreSQL + EF Core 8 |
| Cache / Real-time | Redis (leaderboard, SignalR backplane, rate limiting) |
| Real-time | SignalR (global chat) |
| Auth | JWT + refresh token rotation (SHA-256 hashed) |
| Docs | Swagger / Swashbuckle |

## Key Features

- 🔐 JWT auth with SHA-256 hashed refresh tokens
- 🎯 Server-side score validation (anti-cheat)
- 💰 Transactional purchase flow with audit log
- 💬 Real-time global chat (SignalR + Redis backplane) with rate limiting
- 📄 API versioning + reusable pagination
- ⚠️ Centralized exception handling → RFC 7807 `ProblemDetails`

Full explanation of each mechanism (with the "why") is in [docs/ARCHITECTURE.md](docs/Architecture.md).

## API Overview

| Area | Base Route |
|---|---|
| Auth | `/api/v1/auth` (register, login, refresh, logout) |
| Shop | `/api/v1/shop` (browse, buy) |
| Inventory | `/api/v1/inventory` |
| Match & Leaderboard | `/api/v1/matches`, `/api/v1/leaderboard` |
| Chat | `/hub/global-chat` (SignalR) |

Full request/response docs available via Swagger UI at `/swagger` when running locally.

## Getting Started

```bash
git clone https://github.com/qtrung188/BattleArena.git
cd BattleArena/BattleArenaBackendAPI
```

1. Configure `appsettings.json` — PostgreSQL connection string, Redis connection, JWT secret (see `appsettings.json` for required keys).
2. Apply migrations:
   ```bash
   dotnet ef database update
   ```
3. Run:
   ```bash
   dotnet run
   ```
4. Open `/swagger` for the API, or `/` for the demo frontend (`wwwroot`).

## Roadmap

- [ ] Unit tests (xUnit + Moq + EF Core InMemory)
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Cloud deployment (Azure / AWS)
- [ ] Response caching for hot-read endpoints

---

Built for learning and portfolio purposes.
