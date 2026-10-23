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

---

# 6. Проверка отслеживаемых файлов

```bash
git status --ignored
git check-ignore -v src/HotelManagement.Api/bin src/HotelManagement.Api/obj
```

После сборки каталоги `bin/` и `obj/` всех проектов отмечены
как игнорируемые и не попадают в `git status`.

Проверка истории на наличие секретов:

```bash
git log --all --oneline -i -E -G "password=|pwd=|api_?key|private key"
```

Результат пустой. Пароли, токены и ключи в репозиторий не попадали,
поэтому отзыв секретов и очистка истории не требуются.

---

# 7. Правила работы с репозиторием

Файл `docs/version-control.md` дополнен:

- веткой `chore/*` для технических изменений;
- правилами слияния: рабочие ветки создаются от `develop`, перед
  слиянием выполняются сборка и тесты, слияние выполняется с `--no-ff`;
- порядком разрешения конфликтов;
- правилами тегов;
- перечнем файлов, которые не хранятся в репозитории;
- правилами работы с секретами.

После этого ветка `chore/repository-setup` слита в `develop`:

```bash
git checkout develop
git merge --no-ff chore/repository-setup
```

---

# 8. Учебный конфликт

От `develop` созданы две ветки, в которых по-разному изменён
один и тот же фрагмент `README.md` — список требований:

- `docs/readme-ide` — добавлена Visual Studio 2022;
- `docs/readme-editors` — добавлены Rider и VS Code.

Первая ветка слилась без конфликта, при слиянии второй
возник конфликт:

```text
CONFLICT (content): Merge conflict in README.md
```

```text
- .NET SDK 9;
- Git;
<<<<<<< HEAD
- Visual Studio 2022 (версия 17.12 или новее).
=======
- JetBrains Rider 2024.3 или Visual Studio Code с расширением C# Dev Kit.
>>>>>>> docs/readme-editors
```

Оба изменения верные, поэтому они объединены:

```markdown
- .NET SDK 9;
- Git;
- одна из сред разработки:
  - Visual Studio 2022 (версия 17.12 или новее);
  - JetBrains Rider 2024.3 или новее;
  - Visual Studio Code с расширением C# Dev Kit.
```

```bash
git add README.md
git commit
```

---

# 9. Учебный тег

После слияния `develop` в `main` создан тег контрольного состояния:

```bash
git checkout main
git merge --no-ff develop
git tag -a v0.2.0 -m "Этап 2: настройка репозитория"
git push origin main develop --tags
```

---

# 10. Граф истории

```bash
git log --oneline --graph --decorate --all
```

```text
*   b0b4591 (HEAD -> main, tag: v0.2.0) merge: develop в main - этап 2
|\
| *   d75241f (develop) merge: docs/readme-editors в develop, разрешён конфликт в README.md
| |\
| | * 4a7ff7a docs: README - в требования добавлены Rider и VS Code
| * |   192cc19 merge: docs/readme-ide в develop
| |\ \
| | |/
| |/|
| | * 90267ae docs: README - в требования добавлена Visual Studio 2022
| |/
| * 985e047 merge: chore/repository-setup в develop
|/|
| * ff37467 docs: version-control.md - добавлены теги, неотслеживаемые файлы и работа с секретами
| * b331782 docs: version-control.md - добавлены правила слияния и разрешения конфликтов
| * ad0136d docs: version-control.md - добавлены ветка chore и правила именования веток
| * fbafc26 chore: настроены ссылки между проектами по схеме зависимостей
| * 8cb2c56 chore: добавлены проекты тестов UnitTests и IntegrationTests
| * 01615db chore: добавлен проект HotelManagement.Api
| * ca32f94 chore: добавлены проекты модулей Contracts, Rooms, Booking, Services, Guests
| * fd40aae chore: создан файл решения HotelManagement.sln
| * d5c5cd4 chore: добавлен .editorconfig
| * 6ad2ed9 chore: добавлен .gitattributes
| * 8f664e9 chore: в .gitignore добавлены правила для SQLite, секретов и VS Code
| * 5595be5 chore: добавлен .gitignore из шаблона .NET
|/
* 4b501ab (origin/main, origin/docs/project-artifacts, origin/HEAD, docs/project-artifacts) docs: исправлен раздел 21 - история Git дополнена коммитами по отдельным файлам артефактов и финальными правками отчёта
```

---

# 11. Проверка выполнения этапа

| Требование | Результат |
|---|---|
| Добавлен `.gitignore` для .NET | Выполнено |
| `bin`, `obj`, настройки IDE, временные файлы и секреты не отслеживаются | Выполнено |
| Добавлен `.editorconfig` | Выполнено |
| Создана рабочая ветка | Выполнено |
| Не менее пяти осмысленных коммитов | Выполнено |
| Создан учебный тег | Выполнено |
| Описаны правила ветвления, коммитов и слияния | Выполнено |
| Создан и разрешён учебный конфликт | Выполнено |
| Получен граф истории | Выполнено |
