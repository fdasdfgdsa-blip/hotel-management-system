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
