# 7. Внутренние зависимости

Внутренние зависимости определяют взаимодействие проектов внутри решения.

```
Api
  ↓
 Booking
  ↓
 Contracts

Api
  ↓
 Rooms
  ↓
 Contracts

Api
  ↓
 Services
  ↓
 Contracts

Api
  ↓
 Guests
  ↓
 Contracts
```

При этом бизнес-модули не должны напрямую обращаться к внутренним
классам друг друга. Взаимодействие выполняется через публичные контракты.

Например:

```
BookingModule
      ↓
IRoomReservationService
      ↓
RoomModule
```

Это позволяет уменьшить связанность модулей.

---

# 8. Внешние зависимости

| Зависимость | Версия | Где используется | Назначение |
|-------------|--------|------------------|-----------|
| .NET | 9 | все проекты | Платформа выполнения |
| ASP.NET Core | 9 | `HotelManagement.Api` | Серверное API |
| Microsoft.Extensions.Logging.Abstractions | 9.0.0 | `Rooms`, `Booking` | Журналирование через `ILogger<T>` |
| xUnit, xunit.runner.visualstudio | 2.9.2, 2.8.2 | `tests/*` | Модульные и интеграционные тесты |
| Microsoft.NET.Test.Sdk | 17.12.0 | `tests/*` | Запуск тестов командой `dotnet test` |
| coverlet.collector | 6.0.2 | `tests/*` | Сбор покрытия кода (добавлен шаблоном) |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.0 | `IntegrationTests` | Тесты API через `WebApplicationFactory` |
| Git | — | — | Контроль версий |

Версии пакетов зафиксированы в файлах `.csproj`.

Данные сейчас хранятся в памяти приложения (`InMemoryRoomStore`,
`InMemoryBookingStore`). Entity Framework Core и SQLite, указанные
на этапе проектирования, будут подключены при переходе к хранению
данных в базе.

