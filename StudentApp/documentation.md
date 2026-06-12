# Dokumentacja techniczna — Student App (backend)

## Przegląd

- Technologia: .NET 9, ASP.NET Core Web API, Entity Framework Core, SQLite (konfiguracja deweloperska).
- Uwierzytelnianie: JWT (HMAC SHA256).
- Lokalizacja: katalog `StudentApp`.

## Uruchomienie lokalne

1. Upewnij się, że masz zainstalowane .NET 9 SDK.
2. Skonfiguruj połączenie do DB w `appsettings.*` — klucz `ConnectionStrings:DefaultConnection`.
3. Uzupełnij klucz JWT w konfiguracji: `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`.
4. Uruchom:
   - `dotnet run` w katalogu `StudentApp`
   - (opcjonalnie) migracje EF Core: `dotnet ef migrations add <Name>` i `dotnet ef database update`.

## Główne pliki / moduły

- `Program.cs` — konfiguracja usług (DbContext, CORS, JWT, OpenAPI).
- `Data/ApplicationDbContext.cs` — definicja DbSet i reguł relacji.
- `Model/Entities/` — encje: `User`, `UserTask`, `Note`.
- `Model/DTO/` — DTO: `TaskDto`, `NoteDto`, `UserDto`, `LoginUserDto`, `RegisterUserDto`.
- `Model/Enums/` — `TaskPriorityDto`, `TaskStatusDto`.
- `Model/Mappers/` — `TasksMapper`, `NotesMapper` (ręczne mapowania).
- `Controllers/` — `AuthController`, `TasksController`, `NotesController`.

## Model domenowy (skrót)

- `User`:
  - Pola: `Id`, `Username`, `Email`, `PasswordHash`, `PasswordSalt`, `CreatedAt`.
  - Relacje: `Tasks`, `Notes`.
  - Indeks unikalny: `Email` (w `OnModelCreating`).
- `UserTask`:
  - Pola: `Id`, `UserId`, `Title`, `Description?`, `DueDate?`, `Priority`, `Status`, `CreatedAt`, `UpdatedAt?`.
  - Domyślnie: `Priority = Medium`, `Status = ToDo`.
- `Note`:
  - Pola: `Id`, `UserId`, `Title?`, `Content`, `CreatedAt`, `UpdatedAt?`.

## DTO i mapowanie

- DTO odpowiadają polom encji (`TaskDto`, `NoteDto`, `UserDto`).
- Mapowanie odbywa się ręcznie w `Model/Mappers/TasksMapper.cs` i `NotesMapper.cs`.

## Kontrolery i endpointy (skrót)

- `AuthController` (`/api/auth`)
  - `POST /register` — rejestracja (`RegisterUserDto`) → `UserDto`.
  - `POST /login` — logowanie (`LoginUserDto`) → `{ token, user }`.
- `TasksController` (`/api/tasks`)
  - `GET /api/tasks` — lista z filtrowaniem (`TaskFilterDto`) — tylko zadania zalogowanego użytkownika.
  - `GET /api/tasks/{id}` — pobranie zadania (weryfikacja właściciela).
  - `GET /api/tasks/upcoming?limit={n}` — nadchodzące zadania.
  - `POST /api/tasks` — tworzenie zadania (przypisane do aktualnego użytkownika).
  - `PUT /api/tasks/{id}` — pełna aktualizacja.
  - `PATCH /api/tasks/{id}/status` — aktualizacja statusu.
  - `PATCH /api/tasks/{id}/complete` — oznaczenie jako `Done`.
  - `DELETE /api/tasks/{id}` — usunięcie.
- `NotesController` (`/api/notes`)
  - `GET /api/notes` — zwraca wszystkie notatki (uwaga: brak ograniczenia po użytkowniku).
  - `GET /api/notes/{id}` — pobranie notatki właściciela.
  - `POST /api/notes` — tworzenie notatki (przypisana do użytkownika).
  - `PUT /api/notes/{id}` — edycja (warunkowo `Title` / `Content`).
  - `DELETE /api/notes/{id}` — usunięcie.

## Uwierzytelnianie i autoryzacja

- Token JWT generowany w `AuthController` z konfiguracji `Jwt:Key`.
- Kontrolery używają metody pomocniczej `GetCurrentUserId()` (czyta `ClaimTypes.NameIdentifier`) do identyfikacji użytkownika.

## Testy

- Projekt testów: `BackendTest`.
- Przykład: `BackendTest/Mappers/TaskMapperTests.cs` sprawdza poprawność mapowania `UserTask` → `TaskDto`.
- Istnieją testy kontrolerów: `NotesControllerTests`, `AuthControllerTests`, `TasksControllerTests`.

## Szybkie przykłady

- Rejestracja (cURL):
  - curl -X POST -H "Content-Type: application/json" -d '{"username":"u","email":"e@e.pl","password":"pass"}' http://localhost:5000/api/auth/register
