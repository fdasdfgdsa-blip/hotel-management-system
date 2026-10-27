# 10. Описание API

Клиентское приложение взаимодействует с системой через HTTP API.

Предполагаются следующие основные точки входа.

| Метод | Адрес | Назначение | Состояние |
|-------|-------|-----------|-----------|
| GET | /api/rooms | Получение списка номеров | Реализовано |
| GET | /api/rooms/{id} | Получение номера | Не реализовано |
| POST | /api/bookings | Создание бронирования | Реализовано |
| GET | /api/bookings/{id} | Получение бронирования | Реализовано |
| POST | /api/bookings/{id}/cancel | Отмена бронирования | Не реализовано |
| POST | /api/bookings/{id}/check-in | Заселение гостя | Не реализовано |
| POST | /api/bookings/{id}/check-out | Выселение гостя | Не реализовано |
| POST | /api/bookings/{id}/services | Добавление услуги | Не реализовано |
| GET | /api/guests/{id} | Получение данных гостя | Не реализовано |
| GET | /api/services | Список дополнительных услуг | Не реализовано |

## Ответы реализованных точек входа

| Точка входа | Статус | Тело |
|---|---|---|
| `GET /api/rooms` | `200` | список `RoomDto` |
| `POST /api/bookings` | `201` | `BookingDto`, заголовок `Location` |
| | `400` | `{ "errorCode": "INVALID_REQUEST" }` или `{ "errorCode": "INVALID_BOOKING_PERIOD" }` |
| | `404` | `{ "errorCode": "ROOM_NOT_FOUND" }` |
| | `409` | `{ "errorCode": "ROOM_NOT_AVAILABLE" }` |
| | `415` | без тела — заголовок `Content-Type` не `application/json` |
| `GET /api/bookings/{id}` | `200` | `BookingDto` |
| | `404` | `{ "errorCode": "BOOKING_NOT_FOUND" }`; если `id` не является GUID — без тела |
| Любая | `500` | `{ "errorCode": "INTERNAL_ERROR" }` |

Контракт API на последующих этапах может быть дополнительно
описан средствами OpenAPI.


---

# Контракты модулей в коде

Контракты взаимодействия модулей находятся в проекте
`src/HotelManagement.Contracts`. Модули ссылаются только на этот проект
и не обращаются к классам друг друга напрямую.

| Контракт | Получатель | Назначение |
|----------|-----------|-----------|
| `IRoomReservationService` | RoomModule | Резервирование номера на период (протокол № 1) |
| `IRoomCatalogService` | RoomModule | Получение данных о номерах, в том числе цены за ночь |

DTO: `RoomDto`, `ReserveRoomRequest`, `ReserveRoomResult`,
`CreateBookingRequest`, `BookingDto`, `CreateBookingResult`.

Коды ошибок резервирования вынесены в `RoomErrorCodes`:
`ROOM_NOT_FOUND`, `ROOM_NOT_AVAILABLE`, `INVALID_BOOKING_PERIOD`.

Код `INVALID_REQUEST` (`BookingErrorCodes`) возвращается, если в запросе
на бронирование не заполнены обязательные поля (`guestId`, `roomId`,
`checkInDate`, `checkOutDate`) или тело запроса не удалось прочитать,
например дата передана не в формате `yyyy-MM-dd`.

Код `BOOKING_NOT_FOUND` (`BookingErrorCodes`) возвращается, если
бронирование с указанным идентификатором не найдено.

При непредвиденной ошибке сервера возвращается код `INTERNAL_ERROR`
(`ApiErrorCodes`).

`BookingModule` использует оба контракта: сначала получает номер
через `IRoomCatalogService` (для расчёта стоимости), затем
резервирует его через `IRoomReservationService`.
