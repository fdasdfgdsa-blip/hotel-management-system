# Курсовое проектирование. Этап 3  
## Разработка и интеграция модулей проекта в режиме командной работы

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

Текущая ветка — `main`, последний коммит — `8777481` (завершение этапа 2,
тег `v0.2.0`). Проекты модулей на этот момент были пустыми.

Работа велась от ветки `develop`.

---

# 2. Модули этапа

Для этапа выбраны два модуля, которые взаимодействуют при создании
бронирования (протокол № 1 из этапа 1).

| Модуль | Проект | Ответственность |
|---|---|---|
| `RoomModule` | `HotelManagement.Rooms` | Хранение номеров, проверка свободных дат, резервирование номера |
| `BookingModule` | `HotelManagement.Booking` | Создание бронирования, расчёт стоимости проживания |

`BookingModule` не обращается к классам `RoomModule` напрямую,
а использует только интерфейсы из `HotelManagement.Contracts`.

Данные хранятся в памяти приложения. Подключение базы данных
в этап не входило.

---

# 3. Контракт взаимодействия

До разработки модулей в проект `HotelManagement.Contracts` добавлены:

| Тип | Назначение |
|---|---|
| `IRoomReservationService` | Резервирование номера — `ReserveAsync(ReserveRoomRequest, CancellationToken)` |
| `IRoomCatalogService` | Получение номера, в том числе цены за ночь |
| `ReserveRoomRequest`, `ReserveRoomResult` | Запрос и результат резервирования |
| `RoomErrorCodes` | Коды ошибок: `ROOM_NOT_FOUND`, `ROOM_NOT_AVAILABLE`, `INVALID_BOOKING_PERIOD` |
| `RoomDto` | Данные номера |
| `CreateBookingRequest`, `BookingDto`, `CreateBookingResult` | Данные бронирования |

Контракт соответствует протоколу № 1 этапа 1 (`RoomReservationContract v1`).
Интерфейс `IRoomCatalogService` добавлен, потому что для расчёта стоимости
`BookingModule` нужна цена номера, а результат резервирования её не содержит.

Контракт описан в `docs/contracts.md` и слит в `develop` отдельной веткой
до начала работы над модулями.

---

# 4. Работа в ветках

Командная работа смоделирована отдельными ветками от `develop`:

| Ветка | Содержание |
|---|---|
| `feature/booking-contracts` | Контракт: интерфейсы, DTO, коды ошибок, документация |
| `feature/room-module` | Реализация `RoomModule` и его модульные тесты |
| `feature/booking-module` | Реализация `BookingModule` и его модульные тесты |
| `feature/booking-api` | Подключение модулей к API, интеграционные тесты, исправления |

Ветки `feature/room-module` и `feature/booking-module` создавались
от одного и того же состояния `develop` (после слияния контракта)
и разрабатывались параллельно. Все ветки слиты в `develop` с `--no-ff`.

---

# 5. RoomModule

- `Room` — данные номера;
- `RoomReservation` — резерв номера на период, проверка пересечения дат;
- `InMemoryRoomStore` — номера и резервы в памяти, добавление резерва
  выполняется под блокировкой, чтобы два запроса не заняли номер одновременно;
- `RoomCatalogService` — реализация `IRoomCatalogService`;
- `RoomReservationService` — реализация `IRoomReservationService`.

Порядок проверок при резервировании:

1. Период: дата выселения должна быть позже даты заселения,
   иначе `INVALID_BOOKING_PERIOD`.
2. Номер существует, иначе `ROOM_NOT_FOUND`.
3. Номер свободен на период, иначе `ROOM_NOT_AVAILABLE`.

---

# 6. BookingModule

- `HotelBooking` — бронирование;
- `InMemoryBookingStore` — бронирования в памяти;
- `BookingService.CreateAsync` — создание бронирования.

Порядок создания бронирования:

1. Получение номера через `IRoomCatalogService`.
2. Резервирование через `IRoomReservationService`.
3. При ошибке резервирования бронирование не сохраняется,
   код ошибки возвращается вызывающей стороне.
4. Расчёт стоимости: количество ночей × цена за ночь.
5. Сохранение бронирования со статусом `Created`.

---

# 7. Подключение к API

В `HotelManagement.Api` модули зарегистрированы через Dependency Injection,
добавлены точки входа:

| Метод | Адрес | Ответ |
|---|---|---|
| `GET` | `/api/rooms` | Список номеров |
| `POST` | `/api/bookings` | `201` — бронирование создано, `404` — `ROOM_NOT_FOUND`, `409` — `ROOM_NOT_AVAILABLE`, `400` — `INVALID_BOOKING_PERIOD` |
| `GET` | `/api/bookings/{id}` | Бронирование |

Для проверки в `SeedData` добавлены три номера: 101, 102, 201.

---

# 8. Схема потока данных

```text
Клиент
  │  POST /api/bookings
  │  CreateBookingRequest (guestId, roomId, checkInDate, checkOutDate)
  ▼
ApiHost
  │
  ▼
BookingModule: BookingService.CreateAsync
  │
  ├──► IRoomCatalogService.GetByIdAsync(roomId)
  │        RoomModule ──► RoomDto (pricePerNight)
  │        null ──► ROOM_NOT_FOUND
  │
  ├──► IRoomReservationService.ReserveAsync(ReserveRoomRequest)
  │        RoomModule: проверка периода ──► INVALID_BOOKING_PERIOD
  │                    проверка номера  ──► ROOM_NOT_FOUND
  │                    проверка дат     ──► ROOM_NOT_AVAILABLE
  │                    создание резерва ──► ReserveRoomResult(success)
  │
  ├── расчёт стоимости: ночи × pricePerNight
  ├── сохранение HotelBooking
  ▼
CreateBookingResult ──► ApiHost ──► 201 BookingDto / 4xx { errorCode }
```

---

# 9. Журналирование

В модулях используется `ILogger<T>`
(пакет `Microsoft.Extensions.Logging.Abstractions` 9.0.0).
В каждое сообщение передаётся контекст операции: идентификаторы
бронирования, номера и гостя, период, код ошибки.

Фрагмент журнала при запуске API (`dotnet run --project src/HotelManagement.Api`):

```text
info: HotelManagement.Booking.BookingService[0]
      Create booking 0c9f0fc4-cae0-4bed-a371-d806585d61df: guest 22222222-2222-2222-2222-222222222222, room 11111111-1111-1111-1111-111111111201, period 12/10/2026 - 12/12/2026
info: HotelManagement.Rooms.RoomReservationService[0]
      Reserve room 11111111-1111-1111-1111-111111111201 for booking 0c9f0fc4-cae0-4bed-a371-d806585d61df, period 12/10/2026 - 12/12/2026
info: HotelManagement.Rooms.RoomReservationService[0]
      Room 11111111-1111-1111-1111-111111111201 reserved for booking 0c9f0fc4-cae0-4bed-a371-d806585d61df
info: HotelManagement.Booking.BookingService[0]
      Booking 0c9f0fc4-cae0-4bed-a371-d806585d61df created: 2 nights, total price 14000
info: HotelManagement.Booking.BookingService[0]
      Create booking 58fe7092-f60d-4189-8450-1fa28e5aee2d: guest 22222222-2222-2222-2222-222222222222, room 11111111-1111-1111-1111-111111111201, period 12/11/2026 - 12/13/2026
info: HotelManagement.Rooms.RoomReservationService[0]
      Reserve room 11111111-1111-1111-1111-111111111201 for booking 58fe7092-f60d-4189-8450-1fa28e5aee2d, period 12/11/2026 - 12/13/2026
warn: HotelManagement.Rooms.RoomReservationService[0]
      Reservation rejected: ROOM_NOT_AVAILABLE, room 11111111-1111-1111-1111-111111111201, booking 58fe7092-f60d-4189-8450-1fa28e5aee2d, period 12/11/2026 - 12/13/2026
warn: HotelManagement.Booking.BookingService[0]
      Booking 58fe7092-f60d-4189-8450-1fa28e5aee2d not created: ROOM_NOT_AVAILABLE
```

По идентификатору бронирования видно, как один запрос проходит
через оба модуля.

---

# 10. Тесты

## 10.1. Модульные тесты

`tests/HotelManagement.UnitTests`:

| Тест | Проверка |
|---|---|
| `ReserveAsync_FreeRoom_ReturnsSuccess` | Свободный номер резервируется |
| `ReserveAsync_OverlappingPeriod_ReturnsRoomNotAvailable` | Пересечение дат — `ROOM_NOT_AVAILABLE` |
| `ReserveAsync_CheckOutBeforeCheckIn_ReturnsInvalidBookingPeriod` | Выселение раньше заселения — `INVALID_BOOKING_PERIOD` |
| `ReserveAsync_UnknownRoom_ReturnsRoomNotFound` | Несуществующий номер — `ROOM_NOT_FOUND` |
| `CreateAsync_ThreeNights_CalculatesTotalPriceByNights` | Стоимость = ночи × цена |
| `CreateAsync_ReservationRejected_DoesNotSaveBooking` | При отказе в резерве бронирование не сохраняется |

В тестах `BookingService` модуль номеров заменён заглушками,
реализующими интерфейсы контракта.

## 10.2. Интеграционные тесты

`tests/HotelManagement.IntegrationTests` — `BookingService` работает
с настоящими `RoomCatalogService` и `RoomReservationService`:

| Тест | Сценарий |
|---|---|
| `CreateAsync_FreeRoom_CreatesBookingAndReservesRoom` | Успешный: бронирование создано, стоимость 14000 |
| `CreateAsync_RoomAlreadyBooked_ReturnsRoomNotAvailable` | Отрицательный: номер занят, второе бронирование не создано |
| `CreateAsync_SameCheckInAndCheckOut_ReturnsInvalidBookingPeriod` | Отрицательный: даты заселения и выселения совпадают |

## 10.3. Проверка через API

```bash
curl -X POST http://localhost:5282/api/bookings -H "Content-Type: application/json" -d @ok.json
```

```text
{"id":"0c9f0fc4-...","roomId":"11111111-1111-1111-1111-111111111201","checkInDate":"2026-12-10","checkOutDate":"2026-12-12","totalPrice":14000,"status":"Created"} HTTP 201
{"errorCode":"ROOM_NOT_AVAILABLE"} HTTP 409
{"errorCode":"INVALID_BOOKING_PERIOD"} HTTP 400
{"errorCode":"ROOM_NOT_FOUND"} HTTP 404
```
