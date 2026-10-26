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

---

# 3. Дефект 2. Ответ 400 без кода ошибки

## 3.1. Симптом

Запрос с датами в формате «дата и время» возвращает `400` без JSON
с кодом ошибки. Вместо `{"errorCode": ...}` приходит текст исключения
со стеком вызовов. В журнале модулей бронирования записей о запросе нет.

## 3.2. Версия

Коммит `264f0c2`, тег `v0.3.0`.

## 3.3. Условия воспроизведения

Даты переданы так, как их отдаёт `Date.toISOString()` в JavaScript:

```json
{
  "guestId": "22222222-2222-2222-2222-222222222222",
  "roomId": "11111111-1111-1111-1111-111111111201",
  "checkInDate": "2026-12-10T00:00:00.000Z",
  "checkOutDate": "2026-12-12T00:00:00.000Z"
}
```

```bash
curl -X POST http://localhost:5282/api/bookings -H "Content-Type: application/json" -d @d2-datetime.json
```

## 3.4. Гипотезы

1. Исключение внутри `BookingService` или `RoomModule`.
2. Неверный заголовок `Content-Type`, сервер не распознал тело как JSON.
3. Ошибка чтения тела запроса до вызова точки входа.

## 3.5. Инструменты диагностики

- журнал `ILogger` — проверка, дошёл ли запрос до `BookingService`;
- стек вызовов исключения из журнала ASP.NET Core;
- регрессионный тест через `WebApplicationFactory`
  (пакет `Microsoft.AspNetCore.Mvc.Testing` 9.0.0).

## 3.6. Наблюдаемые факты

Гипотеза 1 опровергнута — в журнале нет записи `Create booking`,
то есть `BookingService.CreateAsync` не вызывался.

Гипотеза 2 опровергнута — в ответе перечислены заголовки запроса,
`Content-Type: application/json` указан верно.

Гипотеза 3 подтверждена стеком вызовов:

```text
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      Microsoft.AspNetCore.Http.BadHttpRequestException: Failed to read parameter "CreateBookingRequest request" from the request body as JSON.
       ---> System.Text.Json.JsonException: The JSON value could not be converted to HotelManagement.Contracts.CreateBookingRequest. Path: $.checkInDate | LineNumber: 0 | BytePositionInLine: 138.
       ---> System.FormatException: The JSON value is not in a supported DateOnly format.
```

**Первопричина:** тип `DateOnly` принимает только формат `yyyy-MM-dd`.
Ошибка возникает при чтении тела запроса, до точки входа. В API не было
единой обработки таких ошибок: в окружении Development ответ формирует
страница исключений разработчика (текст со стеком), в Production
возвращается пустой ответ `400`.

## 3.7. Исправление

В `HotelManagement.Api`:

- `RouteHandlerOptions.ThrowOnBadRequest = true` — ошибка чтения тела
  запроса во всех окружениях приводит к исключению;
- добавлен `UseExceptionHandler`: `BadHttpRequestException` → `400`
  с телом `{"errorCode":"INVALID_REQUEST"}` и предупреждением в журнале
  с причиной; прочие исключения → `500` с `INTERNAL_ERROR`;
- в `appsettings.json` отключена стандартная запись
  `ExceptionHandlerMiddleware`, так как исключение уже журналируется
  в обработчике.

Формат даты в контракте не менялся — по протоколу № 1 даты передаются
как `yyyy-MM-dd`, клиент получает понятный код ошибки.

## 3.8. Регрессионный тест

`BookingApiTests.PostBooking_DateWithTime_ReturnsBadRequestWithErrorCode` —
отправляет запрос через `WebApplicationFactory<Program>` и проверяет
статус `400` и код `INVALID_REQUEST`. Для теста в `Program.cs` добавлено
объявление `public partial class Program`.

До исправления тест не проходил — тело ответа не является JSON:

```text
[FAIL] BookingApiTests.PostBooking_DateWithTime_ReturnsBadRequestWithErrorCode
  System.Text.Json.JsonException : 'M' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.
Не пройден!: не пройдено 1, пройдено 3, всего 4 - HotelManagement.IntegrationTests.dll (net9.0)
```

## 3.9. Повторная сборка и тестирование

```text
Сборка успешно завершена.
    Предупреждений: 0
    Ошибок: 0
```

```text
Пройден! : не пройдено 0, пройдено 8, всего 8 - HotelManagement.UnitTests.dll (net9.0)
Пройден! : не пройдено 0, пройдено 4, всего 4 - HotelManagement.IntegrationTests.dll (net9.0)
```

Тот же запрос после исправления:

```text
{"errorCode":"INVALID_REQUEST"} HTTP 400
```

```text
warn: HotelManagement.Api[0]
      Bad request POST /api/bookings: The JSON value could not be converted to HotelManagement.Contracts.CreateBookingRequest. Path: $.checkInDate | LineNumber: 0 | BytePositionInLine: 138.
```

## 3.10. Почему симптом отличался от первопричины

Ответ выглядел как сбой сервера, а журнал модулей был пустым, поэтому
сначала казалось, что запрос вообще не обрабатывается. На самом деле
запрос отклонялся на этапе привязки параметров ASP.NET Core, ещё до кода
проекта, из-за формата одного поля. Без единого обработчика ошибок
причина была видна только в стеке вызовов.

---

# 4. Проверка после исправлений

После слияния веток в `develop`:

```bash
dotnet build
dotnet test
```

```text
Предупреждений: 0
Ошибок: 0
```

```text
Пройден! : не пройдено 0, пройдено 8, всего 8 - HotelManagement.UnitTests.dll (net9.0)
Пройден! : не пройдено 0, пройдено 4, всего 4 - HotelManagement.IntegrationTests.dll (net9.0)
```

Запросы к API:

```text
--- d1-room_id
{"errorCode":"INVALID_REQUEST"} HTTP 400
--- d1-nodates
{"errorCode":"INVALID_REQUEST"} HTTP 400
--- d2-datetime
{"errorCode":"INVALID_REQUEST"} HTTP 400
--- ok
{"id":"3ca9a395-...","roomId":"11111111-1111-1111-1111-111111111201","checkInDate":"2026-12-10","checkOutDate":"2026-12-12","totalPrice":14000,"status":"Created"} HTTP 201
```

---

# 5. Проверка выполнения этапа

| Требование | Дефект 1 | Дефект 2 |
|---|---|---|
| Симптом | 2.1 | 3.1 |
| Версия / коммит | 2.2 | 3.2 |
| Условия воспроизведения | 2.3 | 3.3 |
| Минимум две гипотезы | 2.4 — три | 3.4 — три |
| Инструмент диагностики | 2.5 | 3.5 |
| Факт, подтверждающий первопричину | 2.6 | 3.6 |
| Исправление | 2.7 | 3.7 |
| Регрессионный тест | 2.8 | 3.8 |
| Повторная сборка и тестирование | 2.9 | 3.9 |
| Почему симптом отличался от причины | 2.10 | 3.10 |

---

# 6. Оставшиеся риски

**Риск 1. Проверка только на пустые значения.**
Проверяется, что поля заполнены. Существование гостя не проверяется,
так как `GuestModule` ещё не реализован.

**Риск 2. Клиенты с другим форматом даты.**
Клиент, передающий дату со временем, получит `INVALID_REQUEST`.
Формат `yyyy-MM-dd` нужно соблюдать на стороне клиента.

**Риск 3. Отключённая запись ExceptionHandlerMiddleware.**
Если обработчик ошибок будет удалён, исключения перестанут попадать
в журнал. При изменении `Program.cs` нужно сохранять журналирование
в обработчике.

---

# 7. Вывод

В рамках четвёртого этапа проведена отладка двух дефектов точки входа
`POST /api/bookings`. Для каждого описаны симптом, условия воспроизведения,
гипотезы, использованные инструменты и наблюдения, подтвердившие
первопричину.

Оба дефекта исправлены, на каждый добавлен регрессионный тест, который
не проходил до исправления. После исправлений сборка выполняется
без ошибок, все 12 тестов проходят.

В обоих случаях симптом проявлялся не там, где находилась причина:
неверный запрос клиента выглядел как отсутствие номера в гостинице
или как сбой сервера.
