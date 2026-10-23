# 14. Инструкция сборки и запуска

## Требования

Для запуска проекта необходимы:

- .NET SDK 9;
- Git;
- JetBrains Rider 2024.3 или Visual Studio Code с расширением C# Dev Kit.

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

После запуска API становится доступно по адресу,
указанному приложением в консоли.
