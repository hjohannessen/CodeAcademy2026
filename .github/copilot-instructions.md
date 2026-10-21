# Copilot instructions: Code Academy 2026

Docs, task texts, code comments, UI strings and test display names are in Norwegian (Bokmål). When you add text, use the language of the file you're in.

| Folder | Session | Contents |
| --- | --- | --- |
| `1-DevOps/` | 1: CI + Docker | `idempotweet` (Next.js 16 + Postgres). Participants write `.github/workflows/ci.yml`, `docker.yml` and `1-DevOps/2-Docker/{Dockerfile,.dockerignore,docker-compose.yml}` |
| `2-EventDriven/` | 2: RabbitMQ | `dotnet-consumer` (.NET 10: `Common`, `Producer`, `Consumer`, `Infrastructure`) |
| `3-APIs/` | 3: Kaffebar REST API | A .NET code-first track and a Java contract-first track, each with a starter folder and a `-solution` folder |
| `4-Frontend/` | 4: live UI | `kaffebar-web` (Next.js 16, React 19, TanStack Query), built on the session 3 API and RabbitMQ |

## Build, test and lint

Each project builds from its own folder. There's no root solution or workspace. Some FASIT text mentions a root `CodeAcademy2026.sln` and `4-Frontend/kaffebar-api`; neither exists.

### Next.js apps (`1-DevOps/idempotweet`, `4-Frontend/kaffebar-web`)

Both use Yarn 4 through corepack: run `corepack enable`, then `yarn install`. CI uses `yarn install --immutable`.

- Common scripts: `yarn dev` (port 3000), `yarn build`, `yarn lint`, `yarn test:watch`. `yarn test` runs Vitest with jsdom and only collects tests under `__tests__/**`.
- Single test: `yarn test __tests__/components.test.tsx -t "statusflyten"`. You can filter by file path, by test name with `-t`, or both.
- idempotweet only:
  - `yarn test:coverage` uses v8, with thresholds in `vitest.config.ts`.
  - `yarn seed` loads 200 demo idems and needs Postgres.
- kaffebar-web only:
  - `USE_MOCK_API=true yarn dev` runs without any containers.
  - `yarn generate:types` regenerates `src/types/kaffebar.ts` from `${KAFFEBAR_API_URL:-http://localhost:8080}/openapi.json`.



### Containers and the justfile

- The root `--profile dotnet`. Either starts RabbitMQ plus the session 3 solution API on port 8080.
- Opt-in profiles:
  - `consumer`
  - `idempotweet` needs your own `1-DevOps/2-Docker/Dockerfile`. If it's missing, copy `Dockerfile-solution`.
  - `frontend` needs `4-Frontend/kaffebar-web/Dockerfile`, which is a skeleton until someone writes it.
- The `justfile` only drives idempotweet: `just setup`, `just dev` (Postgres from `docker-compose.dev.yml` plus `yarn dev`), `just test`, `just seed`, `just stop`, `just clean`.
- Podman works in place of Docker: use `podman compose ...`, and run `just use-podman` to set `COMPOSE` in `.env`.
- Windows:
  - `just` recipes need `sh` on PATH (for example from Git Bash). Without it, run the commands from the `justfile` directly.
  - The docs use POSIX `VAR=value cmd` syntax. In PowerShell, write `$env:USE_MOCK_API="true"; yarn dev` instead.
  - The `package.json` scripts work as-is because Yarn runs them in its own portable shell.

## Architecture: how the sessions connect

- **Events:**
  - **idempotweet:** `POST /api/publish` writes to Postgres. With `ENABLE_RABBITMQ=true`, it instead sends an `idem.created` message to the durable queue `idem-events` and returns 202.
  - **Session 2 consumers:** these practice fanout, direct, topic and headers exchanges. They can also persist idems to Postgres (JPA/Flyway in Java, EF Core in .NET's `Infrastructure/`).
  - **Kaffebar solution APIs:** these publish `order.created` and `order.status-changed` to the durable topic exchange `kaffebar`, using the envelope `{ type, timestamp, data: Order, previousStatus? }`.
  - **Auto-barista:** it moves orders `PENDING → BREWING → READY` every 8 seconds.
  - Event publishing and the auto-barista are **off by default**. The root compose file turns them on with environment variables (`Kaffebar__Events__Enabled`, `KAFFEBAR_EVENTS_ENABLED` and similar).
  - Publishing is fire-and-forget. A broker outage must never turn a successful HTTP call into a 500.
- **One contract, two implementations:**
  - **.NET track (code-first):** the solution's JSON options and its `OpenApi/` transformers exist only to make the output match that contract.
  - **Compatible means identical:** same paths, field names, enum values (uppercase strings), status codes and media types.
  - **Errors:** RFC 7807 `application/problem+json`, with validation errors as `errors: [{ field, message }]`.
- **Shared seed data:** the 10-item menu uses fixed ids `c0ffee01-0000-4000-8000-00000000000X`, and the seeded orders use `0de50001-…`. Both API test suites assert on this data.
  - If you change it, change all of them together.
- **The frontend is its own BFF (backend-for-frontend):** the browser only talks to Next.
  - Server components and the route handlers in `src/app/api/orders/**` call the API over HTTP through `src/lib/api.ts`, using `KAFFEBAR_API_URL`.
  - `src/app/api/events/route.ts` subscribes to RabbitMQ through `src/lib/rabbitmq.ts` and relays the events as Server-Sent Events (SSE). It uses amqplib with one exclusive, auto-delete queue per client.
  - `/api/events/demo` is a fake event source for round 1.
- **Frontend types:** they flow from `/openapi.json` to `src/types/kaffebar.ts` (generated, never edit it) and then to `src/types/domain.ts` (the only hand-written bridge).
  - Both solution APIs serve `/openapi.json` for this.
  - The .NET container runs in `Development` because OpenAPI and Scalar sit behind `IsDevelopment()`.

### Ports

| Port | Used by |
| --- | --- |
| 3000 | `yarn dev` for both kaffebar-web and idempotweet (root compose maps the idempotweet container to 3100) |
| 5035 | The .NET Kaffebar API when run locally (starter and solution) |
| 5672 / 15672 | RabbitMQ: AMQP / management UI (guest/guest) |
| 5432 | Postgres from the root compose file and `docker-compose.dev.yml` |
| 5435 | Postgres from the `2-EventDriven/*/docker-compose.yml` files |

The session 2 compose files start their own RabbitMQ (`rabbitmq_ca`) and a container named `postgres`. Both collide with the root compose file, so run only one stack at a time.

## Conventions

### Next.js (both apps)

- **Read the bundled docs first:** Next 16 differs from training data. As `4-Frontend/kaffebar-web/AGENTS.md` says (`CLAUDE.md` loads the same file), read the relevant guide in the app's `node_modules/next/dist/docs/` before writing Next code.
  - `next dev` regenerates the `AGENTS.md` block, so commit it rather than deleting it.
- **Standalone output:** both apps use `output: "standalone"`, so runtime images copy only `.next/standalone`, `.next/static` and `public`.
  - idempotweet's Dockerfile lives in `1-DevOps/2-Docker/`, but its build context is `1-DevOps/idempotweet/`.
- **Config:**
  - idempotweet's `NEXT_PUBLIC_*` flags are baked in at build time, so Docker has to receive them as build args.
  - kaffebar-web's `src/lib/config.ts` is server-only on purpose. Never add a `NEXT_PUBLIC_` prefix to `KAFFEBAR_API_URL` or `RABBITMQ_URL`.
- **`globalThis` state:** state that must survive hot reload or Next's separate bundles lives on `globalThis`, under keys that start with `__kaffebar`. This covers the AMQP connection, the mock store, the mock event bus and the mock barista timer.
- **Route handlers and fetch caching:**
  - Route handlers that use amqplib set `runtime = "nodejs"` and `dynamic = "force-dynamic"`.
  - Fetch caching is set per endpoint: the menu uses `next: { revalidate: 60 }` and orders use `cache: "no-store"`.
- **Events and SSE:**
  - Events invalidate the `["orders"]` TanStack Query rather than patching the cache.
  - Keep callbacks that effects use in a ref, not in the dependency array.
  - In dev, StrictMode opens two SSE connections; that's expected.
  - If events arrive duplicated after a hot reload, restart `yarn dev`.
- **Mock mode and tests:**
  - With `USE_MOCK_API=true`, the app reads an in-memory store in `src/mocks/`, and `instrumentation.ts` starts the mock barista. MSW is only used in tests.
  - Each app's `__tests__/setup.ts` stubs the browser APIs that jsdom lacks (`EventSource`, `IntersectionObserver`).
- **Review rules:** `4-Frontend/BEST-PRACTICE.md` has the frontend rules used in reviews:
  - No data fetching in `useEffect`.
  - Types are generated from the contract.
  - One styling system (Tailwind here).
  - Tokens stay server-side in HttpOnly cookies, never in `localStorage`.
  - Accessible elements: no clickable `div`s.
  - No dependencies that weren't asked for.

## CI

The `.github/workflows/*-solution.yml` files are reference workflows: the session 1 answers plus builds for the session 2 consumers. They run in this repo:

- `ci-solution.yml` runs on PRs that touch `1-DevOps/idempotweet/**`: install, test, coverage and SonarQube. Other workflows can call it with `workflow_call`.
- `docker-solution.yml` reuses CI, then pushes the idempotweet image to GHCR.
- `build-solution.yml` builds both session 2 consumers and runs SonarQube on push and PR to `main`.
- `consumers-solution.yml` pushes the consumer images to GHCR.

SonarQube needs the `SONAR_TOKEN` secret and the `SONAR_HOST_URL` variable.
