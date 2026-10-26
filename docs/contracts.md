# 10. Описание API

Клиентское приложение взаимодействует с системой через HTTP API.

Предполагаются следующие основные точки входа.

| Метод | Адрес | Назначение |
|-------|-------|-----------|
| GET | /api/rooms | Получение списка номеров |
| GET | /api/rooms/{id} | Получение номера |
| POST | /api/bookings | Создание бронирования |
| GET | /api/bookings/{id} | Получение бронирования |
| POST | /api/bookings/{id}/cancel | Отмена бронирования |
| POST | /api/bookings/{id}/check-in | Заселение гостя |
| POST | /api/bookings/{id}/check-out | Выселение гостя |
| POST | /api/bookings/{id}/services | Добавление услуги |
| GET | /api/guests/{id} | Получение данных гостя |
| GET | /api/services | Список дополнительных услуг |

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
например дата передана не в формате `yyyy-MM-dd`. HTTP-статус — `400`.

`BookingModule` использует оба контракта: сначала получает номер
через `IRoomCatalogService` (для расчёта стоимости), затем
резервирует его через `IRoomReservationService`.
