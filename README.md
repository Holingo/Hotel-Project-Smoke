# Hotel API (ASP.NET Core + EF Core) – projekt zaliczeniowy

REST API do rezerwacji pokoi hotelowych (bez UI). Projekt jest "ponad MVP": oprócz CRUD i reguł domenowych ma paginację, sortowanie, filtrowanie, bonusową regułę cenową (weekend +%), Docker, CI (GitHub Actions), raport pokrycia testów i gotową kolekcję Postmana.

## Technologie
- .NET 8
- ASP.NET Core Web API
- EF Core (Code-First) + SQLite (domyślnie)

## Struktura
```
src/Hotel.Api            // API + kontrolery + Swagger + middleware
src/Hotel.Application    // DTO + usługi + reguły biznesowe
src/Hotel.Infrastructure // DbContext + migracje + SQLite
tests/Hotel.Tests        // testy integracyjne (xUnit)
```

## Uruchomienie (lokalnie)

### 1) Wymagania
- Zainstalowany .NET 8 SDK
- (opcjonalnie) `dotnet-ef` jeśli chcesz tworzyć własne migracje

### 2) Baza danych + migracje
Projekt ma już gotową migrację `InitialCreate` w `src/Hotel.Infrastructure/Migrations`.

**Opcja A (najprostsza):** uruchom API w trybie Development – aplikacja sama zrobi `Database.Migrate()`:
```bash
cd src/Hotel.Api
dotnet run
```

**Opcja B (ręcznie):**
```bash
dotnet tool install --global dotnet-ef

dotnet ef database update   --project src/Hotel.Infrastructure   --startup-project src/Hotel.Api
```

Domyślna baza to plik `hotel.db` w katalogu, z którego uruchamiasz API (connection string w `src/Hotel.Api/appsettings.json`).

### 3) Swagger
Po uruchomieniu w `Development`:
- Swagger UI: `/swagger/index.html`

Aby uruchomić w `Development` na Windows (CMD):
```bat
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/Hotel.Api
```

## Testy
```bash
dotnet test
```

### Pokrycie testów (HTML)
```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```
Otwórz: `coverage-report/index.html`.

## Najważniejsze endpointy (MVP)
- `GET /api/rooms?minCapacity=&onlyActive=&type=&sortBy=&sortDir=&page=&pageSize=`
  - `sortBy`: `number` (domyślnie), `type`, `capacity`, `price`
  - `sortDir`: `asc` (domyślnie) lub `desc`
- `GET /api/rooms/{id}`
- `POST /api/rooms`
- `PUT /api/rooms/{id}`
- `DELETE /api/rooms/{id}` (soft delete -> `IsActive=false`)
- `GET /api/guests`
- `GET /api/guests/{id}`
- `POST /api/guests`
- `PUT /api/guests/{id}`
- `GET /api/reservations/{id}`
- `POST /api/reservations` (walidacje + cena)
- `DELETE /api/reservations/{id}` (anulowanie -> `Status=Canceled`, idempotentne)
- `GET /api/availability?checkIn=YYYY-MM-DD&checkOut=YYYY-MM-DD&minCapacity=&type=`

## Przykładowe requesty

### Utworzenie rezerwacji
```bash
curl -X POST http://localhost:5000/api/reservations   -H "Content-Type: application/json"   -d '{
    "roomId": 2,
    "guestId": 1,
    "checkIn": "2030-03-04",
    "checkOut": "2030-03-07",
    "guestsCount": 2
  }'
```

### Dostępność
```bash
curl "http://localhost:5000/api/availability?checkIn=2030-01-10&checkOut=2030-01-12&minCapacity=2"
```

## Reguły biznesowe (MVP)
- Zakaz nakładania rezerwacji dla tego samego pokoju (overlap)
- `checkIn < checkOut`, min 1 noc, max 30 nocy (konfig w `Reservation:MinNights/MaxNights`)
- `GuestsCount <= Capacity`
- `TotalPrice` liczone per noc.
- **Bonus**: weekend (piątek/sobota) może mieć dopłatę (domyślnie +10%), konfigurowalną w `Reservation`.
- Anulowanie do czasu check-in (UTC date)


## CI/CD (GitHub Actions)
- `.github/workflows/ci.yml` – build + test + generowanie HTML coverage jako artifact.

## Azure (opcjonalnie)
- `.github/workflows/azure-deploy.yml` – ręczny deploy do Azure App Service (wymaga ustawienia secrets: `AZURE_WEBAPP_NAME`, `AZURE_WEBAPP_PUBLISH_PROFILE`).
