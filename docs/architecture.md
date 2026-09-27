# 5. Архитектурная схема

Для проекта выбрана модульная архитектура.

![Архитектурная схема ИС управления гостиницей](img/architecture.png)

Диаграмма построена по коду Mermaid на сайте https://mermaid.live.
Исходный код диаграммы приведён ниже и хранится в этом отчёте.

```mermaid
flowchart TB
    Guest(["Гость"])
    Admin(["Администратор"])
    ClientApp["Клиентское приложение<br/>(браузер)"]

    subgraph Host ["ASP.NET Core API Host"]
        Api["ApiHost"]

        subgraph Modules ["Бизнес-модули"]
            Booking["BookingModule<br/>бронирование"]
            Room["RoomModule<br/>номерной фонд"]
            GuestM["GuestModule<br/>гости"]
            Service["ServiceModule<br/>доп. услуги"]
        end

        Db[("SQLite / EF Core")]
    end

    Guest -->|HTTPS| ClientApp
    Admin -->|HTTPS| ClientApp
    ClientApp -->|"REST / JSON"| Api

    Api --> Booking
    Api --> Room
    Api --> GuestM
    Api --> Service

    Booking -->|"резервирование номера"| Room
    Booking -->|"добавление услуги"| Service
    Booking -->|"данные гостя"| GuestM

    Booking --> Db
    Room --> Db
    GuestM --> Db
    Service --> Db
```
