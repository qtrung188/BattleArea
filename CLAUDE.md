# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

BattleArenaBackendAPI — an ASP.NET Core 8 Web API. Currently the default project template (only the scaffolded `WeatherForecast` sample controller/model exist); actual battle-arena domain code has yet to be added.

## Commands

Run all commands from the repository root (where `BattleArenaBackendAPI.sln` lives).

- Build: `dotnet build`
- Run (dev, serves Swagger UI at `/swagger`): `dotnet run --project BattleArenaBackendAPI`
- Run a specific launch profile: `dotnet run --project BattleArenaBackendAPI --launch-profile https`
- Restore packages: `dotnet restore`

There is no test project yet. When one is added, run tests with `dotnet test`, and a single test with `dotnet test --filter "FullyQualifiedName~<TestName>"`.

Note: the SDK installed here is 9.0.x while the project targets `net8.0` (`BattleArenaBackendAPI/BattleArenaBackendAPI.csproj`). The 9.x SDK builds net8.0 fine; keep `TargetFramework` at `net8.0` unless intentionally upgrading.

## Architecture

Standard minimal-hosting ASP.NET Core layout under `BattleArenaBackendAPI/`:

- `Program.cs` — single entry point wiring up the whole app: registers controllers + Swagger, then configures the middleware pipeline (Swagger only in Development, HTTPS redirection, authorization, controller routing). All service registration and pipeline changes go here. Note it uses an explicit `public class Program` with a `static Main` (not the top-level-statements style of the default template), so `Program` is directly referenceable — e.g. `WebApplicationFactory<Program>` for integration tests.
- `Controllers/` — attribute-routed API controllers (`[ApiController]`, `[Route("[controller]")]`). Constructor injection via the built-in DI container.
- `appsettings.json` / `appsettings.Development.json` — layered configuration; the `Development` file overrides base settings when `ASPNETCORE_ENVIRONMENT=Development`.
- `Properties/launchSettings.json` — local run profiles and ports: http `5176`, https `7129`, all launching the Swagger UI.

`Nullable` and `ImplicitUsings` are both enabled, so nullable-reference-type annotations are expected and common `using`s are implicit.
