# Курсовое проектирование. Этап 2  
## Настройка работы системы контроля версий: типы импортируемых файлов, пути, фильтры и параметры импорта в репозиторий

**Тема курсового проекта:** «Разработка информационной системы управления гостиницей»

**МДК:** 02.02 «Инструментальные средства разработки программного обеспечения»  
**Стек:** C# / .NET 9, Git  
**Тип приложения:** веб-приложение с серверной частью на ASP.NET Core

---

# 1. Исходное состояние проекта

Исходное состояние проверено командами:

```bash
git status
git log -1 --oneline
```

Текущая ветка — `main`, последний коммит — `4b501ab` (завершение этапа 1).

В репозитории на этот момент были только документы этапа 1.
Отсутствовали `.gitignore`, `.editorconfig`, решение и проекты,
а также ветка `develop`, описанная в правилах репозитория.

Для выполнения этапа созданы ветки:

```bash
git checkout -b develop
git checkout -b chore/repository-setup
```

---

# 2. Настройка .gitignore

Файл создан из стандартного шаблона .NET:

```bash
dotnet new gitignore
```

Шаблон исключает каталоги `bin/` и `obj/`, настройки Visual Studio
и Rider (`.vs/`, `.idea/`, `*.user`), результаты тестов, логи
и временные файлы.

В конец файла добавлены правила проекта:

```gitignore
# Локальная база данных SQLite
*.db
*.db-shm
*.db-wal

# Локальные настройки и секреты
.env.*
appsettings.Local.json
appsettings.*.Local.json
secrets.json
*.key
*.pem
*.p12

# Локальные настройки IDE
.vscode/
```

Файлы базы SQLite создаются у каждого разработчика при запуске,
поэтому не должны храниться в репозитории. Остальные правила
исключают файлы, которые могут содержать пароли и ключи.

---

# 3. Настройка .gitattributes

Файл задаёт, как Git обрабатывает разные типы файлов:

```gitattributes
* text=auto

*.cs        text diff=csharp
*.sln       text eol=crlf
*.sh        text eol=lf

*.png       binary
*.db        binary
```

- `text=auto` — переводы строк в репозитории хранятся в едином
  виде, чтобы файлы не менялись из-за разных настроек Windows и Linux;
- `binary` — изображения и база данных не сравниваются и не
  объединяются как текст.

---

# 4. Настройка .editorconfig

Файл задаёт единые правила оформления кода:

- кодировка UTF-8, отступ 4 пробела (для `.csproj` и `.json` — 2);
- пространства имён в стиле файла;
- интерфейсы начинаются с `I`;
- асинхронные методы заканчиваются на `Async`;
- закрытые поля называются `_camelCase`.

Правила именования соответствуют контрактам этапа 1
(`IRoomReservationService.ReserveAsync`).

---

# 5. Создание решения

Чтобы проверить `.gitignore` на результатах сборки, создано решение
по структуре из этапа 1:

```bash
dotnet new sln -n HotelManagement

dotnet new classlib -n HotelManagement.Contracts -o src/HotelManagement.Contracts -f net9.0
dotnet new classlib -n HotelManagement.Rooms -o src/HotelManagement.Rooms -f net9.0
dotnet new classlib -n HotelManagement.Booking -o src/HotelManagement.Booking -f net9.0
dotnet new classlib -n HotelManagement.Services -o src/HotelManagement.Services -f net9.0
dotnet new classlib -n HotelManagement.Guests -o src/HotelManagement.Guests -f net9.0
dotnet new web -n HotelManagement.Api -o src/HotelManagement.Api -f net9.0

dotnet new xunit -n HotelManagement.UnitTests -o tests/HotelManagement.UnitTests -f net9.0
dotnet new xunit -n HotelManagement.IntegrationTests -o tests/HotelManagement.IntegrationTests -f net9.0
```

Проекты добавлены в решение командой `dotnet sln add`. Ссылки между
проектами заданы по схеме зависимостей этапа 1: модули ссылаются
на `Contracts`, `Api` — на модули.

Проверка:

```bash
dotnet build
dotnet test
```

Сборка выполнена без ошибок, тесты пройдены.
