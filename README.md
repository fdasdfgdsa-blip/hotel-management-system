# Информационная система управления гостиницей

Веб-API на ASP.NET Core (.NET 9) для бронирования номеров гостиницы.
Реализованы модули номерного фонда (`RoomModule`) и бронирования
(`BookingModule`). Описание точек входа — в `docs/contracts.md`.

# Инструкция сборки и запуска

## Требования

Для запуска проекта необходимы:

- .NET SDK 9;
- Git;
- одна из сред разработки:
  - Visual Studio 2022 (версия 17.12 или новее);
  - JetBrains Rider 2024.3 или новее;
  - Visual Studio Code с расширением C# Dev Kit.

Проверка версии .NET:

```bash
dotnet --version
```

## Получение проекта

```bash
git clone https://github.com/fdasdfgdsa-blip/hotel-management-system.git
cd hotel-management-system
```

## Восстановление зависимостей

```bash
dotnet restore
```

## Сборка

```bash
dotnet build
```

## Выполнение тестов

```bash
dotnet test
```

## Запуск

```bash
dotnet run --project src/HotelManagement.Api
```

После запуска API доступно по адресу `http://localhost:5282`,
список номеров: `http://localhost:5282/api/rooms`.
