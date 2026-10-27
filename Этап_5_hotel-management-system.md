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

---

# 4. Результаты по пунктам проверки

| Пункт | Результат | Замечания |
|---|---|---|
| Понятность имён | Имена отражают назначение и предметную область | — |
| Ответственность классов и методов | Модули разделены; `Program.cs` совмещает регистрацию, обработку ошибок и точки входа | 22 |
| Направление зависимостей | `Api → модули → Contracts`, модули не ссылаются друг на друга | — |
| Дублирование логики | Код `INTERNAL_ERROR` задан строкой; конфигурация журналирования повторена в двух файлах | 5, 12 |
| Обработка `null` | Подавление `null` через `!` в коде и тестах | 3, 4 |
| Валидация входных данных | Обязательные поля и период проверяются; срок бронирования не ограничен | 20 |
| Обработка исключений | Обработчик заменяет статус любого `BadHttpRequestException` на `400` | 6 |
| `CancellationToken` | Передаётся от точки входа до `RoomModule`; в каталоге номеров не используется | 25 |
| Конфигурация и секреты | Секретов нет; отключение журнала `ExceptionHandlerMiddleware`; `launchBrowser` открывает несуществующий адрес | 12, 13 |
| Корректность внешних контрактов | Разный формат ответа 404; документация расходится с кодом; комментарий не возвращается | 7, 14–19, 23 |
| Наличие тестов | Не покрыты `RoomCatalogService`, коды ответов API, граничные даты, регистрация хранилищ | 10, 11 |
| Мёртвый и закомментированный код | Закомментированного кода нет; пустые проекты `Services`, `Guests`; неиспользуемый сеттер `Status` | 8, 26 |
| Соответствие `.editorconfig` | 12 нарушений именования, 4 нарушения правила `var`, файлы с BOM | 1, 2, 27 |
| Предупреждения компилятора | 0 предупреждений | — |

---

# 5. Таблица замечаний

## 5.1. Код

| № | Место | Проблема | Риск | Рекомендация | Статус |
|---:|---|---|---|---|---|
| 1 | `tests/` (12 методов в 4 файлах) | IDE1006: асинхронные тестовые методы без суффикса `Async` | `dotnet format --verify-no-changes` не проходит | Тесты названы по схеме `Метод_Сценарий_Результат`, суффикс для них не нужен — отключить правило для `tests/` в `.editorconfig` | Исправлено |
| 2 | `BookingService.cs:35,71`, `InMemoryRoomStore.cs:40`, `BookingApiTests.cs:25` | `var` для `string?`, `int`, `bool`, `string` при правиле `csharp_style_var_for_built_in_types = false` | Неединообразный стиль; правило уровня suggestion `dotnet format` не показывает | Явные типы | Исправлено |
| 3 | `BookingService.cs:68` | `reserveResult.ErrorCode!` — подавление `null` | При отказе резерва без кода клиент получит `400` с `errorCode: null` | Отказ без кода считать нарушением контракта: исключение → `500 INTERNAL_ERROR` | Исправлено |
| 4 | `Program.cs:59`, `BookingServiceTests.cs:22`, `BookingRoomIntegrationTests.cs:49`, `BookingApiTests.cs:41` | `!` после проверки `Success` или чтения ответа | Компилятор не проверяет, что значение не `null` | Атрибут `[MemberNotNullWhen(true, nameof(Booking))]` у `CreateBookingResult.Success`, в тестах `Assert.NotNull` | Исправлено |
| 5 | `Program.cs:45` | Код `"INTERNAL_ERROR"` задан строкой | Расхождение с документацией при изменении | Константа `ApiErrorCodes.InternalError` | Исправлено |
| 6 | `Program.cs:37` | Для любого `BadHttpRequestException` статус заменяется на `400` | Например, `413` от Kestrel при слишком большом теле запроса клиент получит как `400` | Использовать `badRequest.StatusCode` | Исправлено |
| 7 | `Program.cs:78` | `GET /api/bookings/{id}` возвращает `404` без тела, остальные ошибки — с `errorCode` | Клиенту нужно обрабатывать два формата ошибок | Код `BOOKING_NOT_FOUND` и тело `{ errorCode }` | Исправлено |
| 8 | `HotelBooking.cs:21` | У `Status` публичный `set`, который не используется | Статус можно изменить в обход сервиса | `init` до появления операций заселения и отмены | Исправлено |
| 9 | `RoomCatalogService.cs:19` | Сортировка по строке `Number` | Номер «1001» окажется раньше «201» | Сортировать по этажу, затем по номеру | Исправлено |
| 10 | `RoomCatalogService` | Нет модульных тестов | Ошибка в поиске номера или в `IsAvailable` не будет обнаружена | Тесты на поиск, `IsAvailable` в обе стороны и сортировку | Исправлено: 5 тестов |
| 11 | `tests/` | Не покрыты: заезд в день выезда, модульный тест одинаковых дат для `RoomModule` (есть только интеграционный), повторное бронирование через API (дефект `Scoped` из этапа 3), ответы `201`/`404`/`409`/`415` | Повтор уже найденных ошибок не будет обнаружен тестами | Добавить тесты | Исправлено: 2 модульных, 4 теста API |
| 12 | `appsettings.Development.json:6` | Отключение журнала `ExceptionHandlerMiddleware` повторено в двух файлах | Две копии одной настройки | Удалить строку из `appsettings.Development.json`, значение берётся из `appsettings.json` | Исправлено |
| 13 | `launchSettings.json:7,16` | `launchBrowser` без `launchUrl` открывает `/` | При запуске из IDE открывается страница `404` | `launchUrl: api/rooms`; файл сохранён в UTF-8 без BOM | Исправлено |

## 5.2. Документация (корректность внешних контрактов)

| № | Место | Проблема | Риск | Рекомендация | Статус |
|---:|---|---|---|---|---|
| 14 | `docs/contracts.md` | Не указано, какие точки входа реализованы; нет соответствия кодов ошибок и HTTP-статусов | Клиент не знает фактический контракт API | Добавить столбец «Состояние» и таблицу ответов | Исправлено |
| 15 | `docs/dependencies.md:58`, `docs/architecture.md:26` | Указаны EF Core и SQLite, которых нет в проекте; реальные пакеты не указаны | Документация не соответствует сборке | Указать фактические пакеты и хранение в памяти | Исправлено |
| 16 | `docs/testing.md:27` | Интеграционный сценарий с `ServiceModule`, который не реализован; не описаны тесты API | Тестовая стратегия не соответствует тестам | Обновить сценарии и перечень тестов | Исправлено |
| 17 | `docs/protocol-room-reservation.md:99` | Порядок проверок «номер → период», в коде наоборот; таймаут и проверка прав не реализованы | Неверное описание протокола | Исправить порядок, отметить нереализованное | Исправлено |
| 18 | `docs/protocol-service-booking.md` | Протокол описан как действующий, модуль не реализован | Читатель ищет несуществующий код | Отметить, что протокол не реализован | Исправлено |
| 19 | `docs/architecture.md:5`, `docs/version-control.md:186`, `README.md:1` | Неверный путь к схеме из `docs/`; в таблице тегов только `v0.2.0`; заголовок README с номером раздела отчёта | Схема не отображается, документация неполная | Исправить путь, дополнить теги, описание проекта и адрес API в README | Исправлено |

## 5.3. Отложенные и принятые замечания

| № | Место | Проблема | Риск | Рекомендация | Статус |
|---:|---|---|---|---|---|
| 20 | `BookingService`, `RoomReservationService` | Срок бронирования не ограничен: принимается заезд в прошлом и период в тысячи лет | Номер может быть занят навсегда | Ввести правило предметной области (бронирование не раньше текущей даты, максимальный срок) и описать в протоколе | Отложено: правило нужно согласовать |
| 21 | `RoomCatalogService.cs:16,29` | `IsAvailable` вычисляется по `DateTime.Today` | Результат зависит от системных часов | Передавать `TimeProvider` через DI | Отложено |
| 22 | `Program.cs` | Регистрация сервисов, обработчик ошибок и точки входа в одном файле | Файл будет расти с каждым модулем | Вынести точки входа в методы расширения по модулям | Отложено: до подключения следующих модулей |
| 23 | `CreateBookingRequest` / `BookingDto` | `Comment` сохраняется, но не возвращается | Клиент не может получить свой комментарий | Добавить `Comment` в `BookingDto` | Отложено: изменение внешнего контракта |
| 24 | `BookingService.cs:100` | `GuestId` проверяется только на пустое значение | Бронирование на несуществующего гостя | Проверять через `GuestModule` | Отложено: модуль не реализован |
| 25 | `RoomCatalogService.cs:14,26` | `CancellationToken` не используется | — | Операции синхронные в памяти; при переходе на базу данных передавать токен в запросы | Принято без изменений |
| 26 | `HotelManagement.Services`, `HotelManagement.Guests` | Пустые проекты | Выглядят как мёртвый код | Проекты соответствуют модулям этапа 1 и будут заполнены | Принято без изменений |
| 27 | `*.csproj`, `HotelManagement.sln` | Файлы с BOM при `charset = utf-8` | Лишние изменения в diff при сохранении в редакторе | Файлы созданы шаблонами `dotnet new`, `dotnet format` их не проверяет | Принято без изменений |
| 28 | `appsettings.json:6` | Отключена категория журнала `ExceptionHandlerMiddleware` | Не попадут служебные сообщения этой категории | Исключения журналируются в обработчике `Program.cs`; отключение убирает дублирование записей | Принято без изменений |
| 29 | `*.csproj` | Не включено `EnforceCodeStyleInBuild` | Нарушения стиля не видны при `dotnet build` | Проверка `dotnet format --verify-no-changes` добавлена в правила слияния (`docs/version-control.md`) | Принято без изменений |

---

# 6. Исправления

**Замечание 1.** В конец `.editorconfig` добавлено:

```ini
# Тесты: имена по схеме Метод_Сценарий_Результат, суффикс Async не требуется
[tests/**.cs]
dotnet_naming_rule.async_methods_end_with_async.severity = none
```

**Замечания 3, 4.**

```csharp
string errorCode = reserveResult.ErrorCode
    ?? throw new InvalidOperationException("Reservation failed without error code");
```

```csharp
[MemberNotNullWhen(true, nameof(Booking))]
public bool Success => Booking is not null;
```

**Замечания 6, 7.**

```csharp
context.Response.StatusCode = badRequest.StatusCode;
```

```csharp
return booking is null
    ? Results.NotFound(new { errorCode = BookingErrorCodes.BookingNotFound })
    : Results.Ok(booking);
```

**Замечание 9.**

```csharp
.OrderBy(room => room.Floor)
.ThenBy(room => room.Number, StringComparer.Ordinal)
```

**Замечания 10, 11.** Добавлены тесты:

| Класс | Тест |
|---|---|
| `RoomCatalogServiceTests` | `GetByIdAsync_UnknownRoom_ReturnsNull` |
| | `GetByIdAsync_ExistingRoom_ReturnsPricePerNight` |
| | `GetByIdAsync_RoomWithoutReservations_IsAvailable` |
| | `GetByIdAsync_RoomReservedAroundToday_IsNotAvailable` |
| | `GetAllAsync_RoomsOnDifferentFloors_OrderedByFloorThenNumber` |
| `RoomReservationServiceTests` | `ReserveAsync_SameCheckInAndCheckOut_ReturnsInvalidBookingPeriod` |
| | `ReserveAsync_CheckInOnPreviousCheckOutDay_ReturnsSuccess` |
| `BookingApiTests` | `PostBooking_TextPlainContentType_ReturnsUnsupportedMediaType` |
| | `PostBooking_SameRoomTwice_ReturnsCreatedThenConflict` |
| | `PostBooking_UnknownRoom_ReturnsNotFound` |
| | `GetBooking_UnknownId_ReturnsNotFoundWithErrorCode` |

В тесте `IsAvailable` резерв захватывает соседние дни, чтобы смена даты
во время выполнения не влияла на результат.

Новые тесты запущены на коде до исправлений. Не прошли два —
они подтверждают замечания 7 и 9:

```text
[FAIL] RoomCatalogServiceTests.GetAllAsync_RoomsOnDifferentFloors_OrderedByFloorThenNumber
  Expected: ["101", "201", "1001"]
  Actual:   ["1001", "101", "201"]
[FAIL] BookingApiTests.GetBooking_UnknownId_ReturnsNotFoundWithErrorCode
  System.Text.Json.JsonException : The input does not contain any JSON tokens.
```

Остальные новые тесты прошли сразу — они закрепляют уже правильное поведение.

---

# 7. Проверка после исправлений

```bash
dotnet format --verify-no-changes
```

Команда завершилась без замечаний и без изменения файлов.

```bash
dotnet build
dotnet test
```

```text
Предупреждений: 0
Ошибок: 0
```

```text
Пройден! : не пройдено 0, пройдено 15, всего 15 - HotelManagement.UnitTests.dll (net9.0)
Пройден! : не пройдено 0, пройдено 8, всего 8 - HotelManagement.IntegrationTests.dll (net9.0)
```

---

# 8. Проверка выполнения этапа

| Требование | Результат |
|---|---|
| Проверены все пункты из задания | Выполнено: раздел 4 |
| `dotnet format --verify-no-changes` | Выполнено: до и после исправлений |
| `dotnet build` | Выполнено: 0 предупреждений, 0 ошибок |
| `dotnet test` | Выполнено: 23 теста проходят |
| Таблица замечаний: место — проблема — риск — рекомендация — статус | Выполнено: раздел 5, 29 замечаний |

---

# 9. Оставшиеся риски

**Риск 1.** Отложенные замечания 20–24 нужно выполнить при доработке
бронирования и подключении модулей гостей и услуг.

**Риск 2.** Правило `var` имеет уровень suggestion и не проверяется
`dotnet format`. Новые нарушения можно найти только при просмотре кода.

**Риск 3.** Тест `IsAvailable` зависит от системной даты до выполнения
замечания 21.

---

# 10. Вывод

Проведена инспекция кода, конфигурации и документации проекта
по 14 пунктам задания и инструментальная проверка `dotnet format`,
`dotnet build`, `dotnet test`.

Найдено 29 замечаний: 19 исправлено, 5 отложено с указанием причины,
5 принято без изменений. Добавлено 11 тестов, два из них не проходили
на коде до исправлений. После исправлений `dotnet format` не находит
нарушений, сборка проходит без предупреждений, все 23 теста проходят.
