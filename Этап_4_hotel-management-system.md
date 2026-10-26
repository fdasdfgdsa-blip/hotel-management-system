# Курсовое проектирование. Этап 4  
## Отладка отдельных модулей программного проекта

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

Текущая ветка — `main`, последний коммит — `264f0c2` (тег `v0.3.0`,
завершение этапа 3). Все 9 тестов проходят.

Отлаживалась точка входа `POST /api/bookings` и модуль `BookingModule`.
Для каждого дефекта создана отдельная ветка от `develop`:

- `fix/booking-request-validation` — дефект 1;
- `fix/bad-request-response` — дефект 2.

---

# 2. Дефект 1. «Номер не найден» для существующего номера

## 2.1. Симптом

Запрос на бронирование номера 201 возвращает `404 ROOM_NOT_FOUND`,
хотя номер 201 существует.

```text
{"errorCode":"ROOM_NOT_FOUND"} HTTP 404
```

Похожий случай: запрос без дат возвращает `400 INVALID_BOOKING_PERIOD`,
хотя период клиент просто не указал.

## 2.2. Версия

Коммит `264f0c2`, тег `v0.3.0`.

## 2.3. Условия воспроизведения

1. Запустить API: `dotnet run --project src/HotelManagement.Api`.
2. Отправить запрос, в котором поле номера названо `room_id` вместо `roomId`:

```json
{
  "guestId": "22222222-2222-2222-2222-222222222222",
  "room_id": "11111111-1111-1111-1111-111111111201",
  "checkInDate": "2026-12-10",
  "checkOutDate": "2026-12-12"
}
```

```bash
curl -X POST http://localhost:5282/api/bookings -H "Content-Type: application/json" -d @d1-room_id.json
```

## 2.4. Гипотезы

1. Номера 201 нет в начальных данных (`SeedData`) или у него другой идентификатор.
2. Ошибка поиска номера в `InMemoryRoomStore` / `RoomCatalogService`.
3. Идентификатор номера не дошёл до `BookingService`.

## 2.5. Инструменты диагностики

- `GET /api/rooms` — проверка наличия номера;
- журнал `ILogger` модуля `BookingModule` — в сообщении
  `Create booking` выводится полученный идентификатор номера;
- модульный тест, воспроизводящий ошибку без запуска API.

## 2.6. Наблюдаемые факты

Гипотеза 1 опровергнута — номер есть в списке:

```text
{"id":"11111111-1111-1111-1111-111111111201","number":"201","categoryName":"Люкс",...,"isAvailable":true}
```

Гипотеза 2 опровергнута — тот же номер с полем `roomId` бронируется
успешно (`201 Created`).

Гипотеза 3 подтверждена журналом — в `BookingService` пришёл пустой идентификатор:

```text
info: HotelManagement.Booking.BookingService[0]
      Create booking 6f01b624-...: guest 22222222-..., room 00000000-0000-0000-0000-000000000000, period 12/10/2026 - 12/12/2026
warn: HotelManagement.Booking.BookingService[0]
      Booking 6f01b624-... not created: room 00000000-0000-0000-0000-000000000000 not found
```

Для запроса без дат в журнале период `01/01/0001 - 01/01/0001`.

**Первопричина:** `System.Text.Json` пропускает неизвестные поля, а
отсутствующие поля получают значение по умолчанию (`Guid.Empty`,
`DateOnly.MinValue`). `BookingService` не проверял обязательные поля
и передавал пустые значения дальше.

## 2.7. Исправление

- в `Contracts` добавлен код ошибки `INVALID_REQUEST` (`BookingErrorCodes`);
- `BookingService.CreateAsync` до обращения к `RoomModule` проверяет,
  что `GuestId`, `RoomId`, `CheckInDate`, `CheckOutDate` заполнены;
  при ошибке возвращает `INVALID_REQUEST` и пишет в журнал имя поля;
- код описан в `docs/contracts.md`.

## 2.8. Регрессионные тесты

`BookingServiceTests`:

- `CreateAsync_EmptyRoomId_ReturnsInvalidRequest`;
- `CreateAsync_MissingDates_ReturnsInvalidRequest`.

До исправления оба теста не проходили:

```text
[FAIL] BookingServiceTests.CreateAsync_EmptyRoomId_ReturnsInvalidRequest
  Expected: "INVALID_REQUEST"
  Actual:   "ROOM_NOT_FOUND"
[FAIL] BookingServiceTests.CreateAsync_MissingDates_ReturnsInvalidRequest
  Assert.False() Failure
  Expected: False
  Actual:   True
Не пройден!: не пройдено 2, пройдено 6, всего 8 - HotelManagement.UnitTests.dll (net9.0)
```

Второй тест показал, что сам `BookingModule` без проверки в `RoomModule`
создал бы бронирование без дат.

## 2.9. Повторная сборка и тестирование

```text
Пройден! : не пройдено 0, пройдено 8, всего 8 - HotelManagement.UnitTests.dll (net9.0)
Пройден! : не пройдено 0, пройдено 3, всего 3 - HotelManagement.IntegrationTests.dll (net9.0)
```

Тот же запрос после исправления:

```text
{"errorCode":"INVALID_REQUEST"} HTTP 400
```

```text
warn: HotelManagement.Booking.BookingService[0]
      Booking 922127da-... not created: required field RoomId is empty
```

## 2.10. Почему симптом отличался от первопричины

Ошибка была в запросе клиента, а сообщение о ней выдал другой модуль.
Пустой идентификатор прошёл через `BookingModule` без проверки,
`RoomModule` честно не нашёл номер с идентификатором `Guid.Empty`
и вернул `ROOM_NOT_FOUND`. Клиент получил ответ о данных гостиницы,
хотя проблема была в формате его запроса.
