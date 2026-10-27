# Курсовое проектирование. Этап 5  
## Инспекция кода модулей проекта

**Тема курсового проекта:** «Разработка информационной системы управления гостиницей»

**МДК:** 02.02 «Инструментальные средства разработки программного обеспечения»  
**Стек:** C# / .NET 9, Git  
**Тип приложения:** веб-приложение с серверной частью на ASP.NET Core

---

# 1. Исходное состояние проекта

```bash
git status
git log -1 --oneline
```

Текущая ветка — `main`, последний коммит — `4e8bf32` (тег `v0.4.0`,
завершение этапа 4). Для исправлений по итогам инспекции создана ветка
`fix/code-inspection` от `develop`.

Номера строк в таблице замечаний указаны по коммиту `4e8bf32`.

---

# 2. Объём и порядок инспекции

Проверены все файлы проекта:

| Область | Файлы |
|---|---|
| `HotelManagement.Contracts` | DTO, интерфейсы `IRoomReservationService`, `IRoomCatalogService`, коды ошибок |
| `HotelManagement.Rooms` | `Room`, `RoomReservation`, `InMemoryRoomStore`, `RoomCatalogService`, `RoomReservationService` |
| `HotelManagement.Booking` | `HotelBooking`, `BookingStatuses`, `InMemoryBookingStore`, `BookingService` |
| `HotelManagement.Api` | `Program.cs`, `SeedData.cs`, `appsettings*.json`, `launchSettings.json` |
| `tests/*` | модульные и интеграционные тесты |
| Конфигурация | `.editorconfig`, `.gitignore`, `.gitattributes`, `*.csproj`, `HotelManagement.sln` |
| Документация | `README.md`, `docs/*.md` |

Каждый файл просматривался по всем пунктам из задания (раздел 4).
Документация сверялась с кодом в рамках пункта «корректность внешних
контрактов». Найденные замечания занесены в таблицу (раздел 5).

---

# 3. Инструментальная проверка до исправлений

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test
```

**dotnet format** нашёл 12 нарушений правила именования из `.editorconfig`
в 4 файлах тестов:

```text
tests\HotelManagement.UnitTests\RoomReservationServiceTests.cs(32,23): warning IDE1006: Нарушение правила именования: Отсутствует суффикс: "Async"
tests\HotelManagement.UnitTests\BookingServiceTests.cs(12,23): warning IDE1006: Нарушение правила именования: Отсутствует суффикс: "Async"
tests\HotelManagement.IntegrationTests\BookingRoomIntegrationTests.cs(42,23): warning IDE1006: Нарушение правила именования: Отсутствует суффикс: "Async"
tests\HotelManagement.IntegrationTests\BookingApiTests.cs(23,23): warning IDE1006: Нарушение правила именования: Отсутствует суффикс: "Async"
...
Отформатировано файлов: 4 из 51.
```

**dotnet build:**

```text
Предупреждений: 0
Ошибок: 0
```

**dotnet test:**

```text
Пройден! : не пройдено 0, пройдено 8, всего 8 - HotelManagement.UnitTests.dll (net9.0)
Пройден! : не пройдено 0, пройдено 4, всего 4 - HotelManagement.IntegrationTests.dll (net9.0)
```

Правила `.editorconfig` с уровнем `warning` в проекте проверяются
только `dotnet format` и IDE: свойство `EnforceCodeStyleInBuild`
не включено, поэтому `dotnet build` их не показывает.
