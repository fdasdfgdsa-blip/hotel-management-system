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
