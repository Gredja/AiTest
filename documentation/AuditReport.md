# Полный аудит проекта Gredja — 2026-10-09

**Метод:** 3 параллельных explore-агента (Rules → код/доки → находки). Охват: ~200 `.cs` (без bin/obj) в Api/, E2E/, Ui/, Core/, AllureAdapter/ + AGENTS.md, Rules/*.md, documentation/*, .mimocode/skills/*, backups/SETUP.md, testsettings*.json, .gitignore.
**Статус:** только находки, фиксы НЕ применялись — начать следующую сессию с этого файла.
**Итого:** ~73 major, ~38 minor, ~13 style (строки-находки; одна major-тема «status-first» занимает 35 файлов).

## Приоритеты для следующей сессии

1. **Старая модель UI-авторизации** (major, 4 файла): `[Explicit] StorageStateBootstrap` и «тесты никогда не логинятся» устарели — фактически есть автотест `LoginSignInTests`. Затронуты: `AGENTS.md`, `Rules/ui-testing.md`, `.mimocode/skills/ui-test-gen/SKILL.md`, `documentation/GitHubUiObservableBehaviour.md` (What AI does #7). Плюс в Rules/skills описан cleanup через `CleanupIssueAsync` в Ui-фикстуре, которого нет.
2. **Status-first перед `.Data`** (major, 35 файлов / ~100 методов): разыменование `response.Data` без предварительного `ShouldHaveStatusCode` — при 500 будет NRE вместо понятного падения. Один паттерн, массово.
3. **Устаревший гайденс скиллов-генераторов** (major): `api-test-gen` — несуществующие пути/неймспейсы и суффиксы `Model`/`Request` вместо `ModelResponse`/`ModelRequest`; `e2e-test-gen` — per-test `[TearDown]`-cleanup вместо fixture-registry и хелперы в тестовом классе; `review` — несуществующие пути в списке файлов; `audit` — порог magic strings 3+ вместо 2+.
4. **Шаблон UI-теста в ui-test-gen не компилируется** (major): вызывает `Get<>`/`ShouldHaveStatusCode`/`CleanupIssueAsync`, которых нет в `GitHubUiTestBase`.
5. **Противоречие Rules**: `models.md:82` («без validation-атрибутов») ↔ `assertions.md` (ContractCheck живёт на этих атрибутах) + массовое использование в `Core/Models/**`.
6. **Нумерация `[Description]` ↔ OB** (major): GitHub API — почти все группы не совпадают с нумерацией секций OB; коллизия «12.x» у Auth и Contributors; JP — три разные нумерации; E2E-«E2E-N» нигде не описаны.
7. **Код-стиль находки**: длинные методы (>30), магическое `8` в `RandomString(8)` (16 мест), `"probe"`/`"audit-"`, булевы без `Is/Has` (`UiHeadless`, модельные `Admin/Push/...`), `null!` в Ui-фикстуре, голый `catch`.
8. **Доки/структура**: пропуски в `FILE_STRUCTURE.md` (ui-testing.md, ui-test-gen, GitHubUi OB, install-playwright.ps1, несуществующий `BaseHelpers/`), мусорный корневой `TestAdapter/`, дубль `testsettings.json` в `.gitignore`.

Полные таблицы находок — ниже, по трём направлениям аудита.

---

# I. Код: naming / code-style / code-principles (180 .cs)

Критерии: **major** — нарушение явного правила/риск бага; **minor** — субоптимально/граниченная трактовка; **style** — косметика. Учтены исключения: ключи словарей в `AllureTestResultBuilder`, `[Category(...)]`, fallback-значения (`"Tests"`, `"Unknown"`, `"-"`).

## Major

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| major | Core\Helpers\GitHub\GitHubTestBase.cs:209 | code-style.md → Error handling («Catch specific exceptions») | Голый `catch { ... throw; }` в `CreateScratchPullRequestAsync` — перехватывает любые исключения, не конкретизирован тип | Перечислить конкретные (`HttpRequestException`, `TaskCanceledException` и т.п.) либо `catch (Exception) when (...)` с явным комментарием-обоснованием |
| major | Ui\Helper\GitHubUiTestBase.cs:17, 18, 19, 82 | code-style.md → Null safety («Avoid null!») | 4 присваивания `null!` для `Context`, `Page`, `Browser` (+ `null!` при сбросе в `CloseContextAsync`) — обход проверки null-безопасности, NRE при изменении lifecycle | Инициализировать в `OpenContextAsync` до использования, держать nullable и проверять `is null` (guard уже есть в `CloseContextAsync`), либо бросать ранний `InvalidOperationException` |
| major | Core\Helpers\Waiting\WaitHelper.cs:7, 8 | code.md → Access modifiers («narrowest possible») | `public static readonly TimeSpan DefaultTimeout/DefaultInterval` используются только внутри `WaitHelper` (строки 16–17) | Сделать `private static readonly` |
| major | Core\Helpers\GitHub\GitHubTestBase.cs:122 | code.md → Methods (max ~30 lines) | `CreateScratchPullRequestAsync` ≈103 строки (122–225) — blob→tree→commit→ref→PR в одном методе, включая try/catch-cleanup | Выделить шаги в отдельные приватные методы (`CreateScratchCommitAsync`, `CreateScratchRefsAsync`, …) |
| major | Core\Reporting\TestRunReportGenerator.cs:30 | code.md → Methods | `Generate()` = 43 строки (30–73) | Вынести сборку секций отчёта и запись в отдельные хелперы (частично уже есть — `BuildHeader/BuildTests/...`) |
| major | Core\Helpers\Assertions\AssertHelper.cs:75 | code.md → Methods | `ShouldMatchRequest` ≈33 строки (75–109) | Вынести сравнение пары свойств в отдельный метод |
| major | E2E\GitHub\Tests\IssueStateMachineTests.cs:23 (90), IssueLifecycleTests.cs:24 (52), CollaboratorPermissionTests.cs:36 (62), FailedWriteTests.cs:34 (44), GitRefLifecycleTests.cs:34 (41), CommentChainTests.cs:26 (40), RateLimitDrainTests.cs:25 (35) | code.md → Methods (max ~30) | 7 E2E-тестов длиннее 30 строк (цифры — длина тела) | Разбить цепочки на именованные шаги-хелперы в базовом классе (create/verify/cleanup) |
| major | CreateRefTests.cs:38,53,70,92,114,131,153,175,186; DeleteRefTests.cs:37,55; CreatePullRequestTests.cs:180; GitHubTestBase.cs:124,125; FailedWriteTests.cs:36; GitRefLifecycleTests.cs:36 (16 мест) | code-principles.md → «No magic numbers» | Магическое число `8` в `DataGenerator.RandomString(8)` в 16 местах (тот же смысл — длина случайного суффикса); в проекте уже есть пример констаны `TitleRandomLength = 8` | `private const int RandomNameLength = 8;` в базовых классах/тестах (или общий константный хелпер) |
| major | Api\GitHub\Tests\PullRequests\CreatePullRequestTests.cs:82,105,125,126,143,198,216; Api\GitHub\Tests\Issues\CreateIssueNegativeTests.cs:39,56 | code-style.md → Magic strings (2+ в коде, один смысл) | Литерал `"probe"` ×7 и `"Probe"` ×4 инлайново как тестовые данные (один и тот же заголовок-заглушка) | `private const string ProbeTitle = "probe";` в каждом тестовом классе (или общий константный класс) |
| major | AllureAdapter\Helpers\AllureTestResultBuilder.cs:91 | code-style.md → Loops (sync side-effect над `List<T>` → `ForEach`) | `foreach (var category in resultParams.Categories)` с телом-побочным эффектом `labels.Add(...)`, источник — `List<string>?`, тело в одну инструкцию | `resultParams.Categories.ForEach(category => labels.Add(new() { [NameKey] = "tag", [ValueKey] = category }));` |
| major | Core\Config\TestConfig.cs:41, 58; Core\Logging\TestRunWorkspace.cs:35 | code.md → Naming (bool: `Is`/`Has`/`Can`/`Should`) | Булевы члены без префикса: свойство `UiHeadless` (41), метод `GetRequiredBool` (58), метод `TryOwnRun` (35) | `IsUiHeadless`; для `GetRequiredBool`/`TryOwnRun` — осознанное исключение или переименование (для `Try`-паттерна зафиксировать исключение в Rules) |
| major | Core\Models\GitHub\Responses\InvitationModelResponse.cs:20; Core\Models\GitHub\CollaboratorPermissions.cs:5,7,9,11,13 | code.md → Naming (bool prefix) | Булевы свойства-контракт без `Is/Has/...`: `Expired`, `Admin`, `Maintain`, `Push`, `Triage`, `Pull` | Переименовать (`IsExpired` и т.п.) + `[JsonPropertyName]` из `JsonFields`; если контракт важнее — зафиксировать исключение в Rules |

## Minor

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| minor | AllureNUnitAttribute.cs:134; AllureSkippedTestWriter.cs:63; GetReleasesTests.cs:42; GetTagsTests.cs:41; TestRunReportGenerator.cs:151 (+ `.Count == 0`: TestRunReportGenerator.cs:48,241,289; GetIssueCommentsTests.cs:32) | code-style.md → LINQ («Prefer `Any()` over `Count() > 0`») | Пустота проверяется через `Count`-свойство `List/Queue`. Буквально правило нацелено на метод `Count()`, для коллекций `Count` O(1) — трактовка пограничная | Для консистентности — `Any()` / `!...Any()`; либо зафиксировать в Rules, что `Count`-свойство допустимо |
| minor | AllureAdapter\Helpers\AllureNUnitAttribute.cs:130; AllureSkippedTestWriter.cs:59 | code-style.md → Strings (IsNullOrEmpty/IsNullOrWhiteSpace) | Проверка строк через `value.Length > 0` / `category.Length > 0` вместо `!string.IsNullOrWhiteSpace(...)` | Заменить на `!string.IsNullOrWhiteSpace(...)` в лямбдах `.Where(...)` |
| minor | Core\Helpers\GitHub\GitHubTestBase.cs:124,125; Api\GitHub\Tests\PullRequests\CreatePullRequestTests.cs:180 | code-style.md → Magic strings (2+ в коде, один смысл) | Литерал `"audit-"` (префикс scratch-ветки) повторяется в 2+ файлах с одним смыслом; при этом `RefsHeadsPrefix`/`HeadsPrefix` уже вынесены в константы | `private const string ScratchBranchPrefix = "audit-";` в `GitHubTestBase` |
| minor | Группово: ~293 использования `response.Data!` (все Api/E2E-тесты, GitHubTestBase.cs:130,146,154,166,181,187) | code-style.md → Null safety (дух правила) | Null-forgiving `!` после `ShouldHaveStatusCode` вместо явной проверки `Data is not null`; безопасно только пока статус-ассерт бросает исключение | Оставить как есть, но зафиксировать паттерн в Rules, либо добавить `Data.Should().NotBeNull()` после 2xx-ассертов |
| minor | Api\GitHub\Tests\Auth\AuthNegativeTests.cs и др.: `private const ... = "abc"` (15 файлов: GetCommitsTests:18, GetIssueCommentsTests:149, GetIssueByIdTests:19, GetIssuesTests:191, GetLanguagesTests:18, GetPublicReposTests:65, GetReleasesTests:17, GetUserTests:18, GetAlbumByIdTests:20, GetCommentByIdTests:20, GetPhotoByIdTests:20, DeletePostTests:19, GetPostByIdTests:20, UpdatePostTests:24, GetUserByIdTests:20) | code-style.md → Magic strings (2+ в коде, один смысл) | Одно и то же невалидное значение `"abc"` вынесено в отдельную константу в каждом файле с разными именами (`InvalidPerPage`/`NonNumericId`/`InvalidQueryValue`) — погранично с правилом «2+ раз в кодеbase» против «single-use — в своём классе» | Определиться в Rules: либо общий `InvalidSegment` в одном месте, либо явно закрепить «single-use per class» |

## Style

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| style | AllureAdapter\Helpers\AllureNUnitAttribute.cs:134; Core\Helpers\Assertions\AssertHelper.cs:118; Ui\Helper\GitHubUiTestBase.cs:111 | code-principles.md → «Empty line before return» | `return` без пустой строки после обычной инструкции (`var distinct = ...; return ...`, `...Be(...); return;`, `var statePath = ...; return ...`) | Добавить пустую строку перед `return` |
| style | Api\TestReportSetup.cs:7–8; E2E\TestReportSetup.cs:7–8 | code.md → File Layout («One blank line between members») | Двойная пустая строка после `namespace ...;` перед комментарием | Оставить одну пустую строку |
| style | Core\Helpers\Http\GitHubRequestHelper.cs:5–8; Core\Helpers\Http\JsonPlaceholderRequestHelper.cs:5–8; E2E\GitHub\Tests\CollaboratorPermissionTests.cs:101–104; Ui\Helper\UiHelper\CommonPages\LoginPage.cs:12–15 | code-principles.md → Expression-bodied members | Однострочные тела в фигурных скобках: конструкторы с единственным вызовом, `OneTimeTearDown` с единственным `await`, `OpenAsync` с единственным `await` | `public GitHubRequestHelper() => UseGitHub();`, `public async Task OneTimeTearDown() => await CleanupInvitationAsync(_createdInvitationId);`, `internal async Task OpenAsync() => await _page.GotoAsync(...);` |
| style | CreateRefTests.cs:202–204; CreateCommentTests.cs:111–112; CreateIssueVisibilityTests.cs:50–51; GetIssuesTests.cs:173–174; CreatePullRequestTests.cs:240–241; MergePullRequestTests.cs:68–69; CreatePostTests.cs:208–209; E2E: FailedWriteTests.cs:53–54, GitRefLifecycleTests.cs:53–54, IssueStateMachineTests.cs:42–43 (итого 10 файлов) | code-style.md → Strings (concatenation) | Сообщения Because-ассертов склеиваются из интерполяций через `+` на нескольких строках (`$"..." +$"..." +$"..."`) — пограничный случай: интерполяция есть, конкатенация — тоже | Один многострочный `$"..."` (C# 11 raw string `"""..."""`) или собрать шаблон в приватный хелпер |
| style | Core\Helpers\GitHub\GitHubTestBase.cs (EOF) | code.md → File Layout («No trailing whitespace»/чистота файла) | Файл не заканчивается переводом строки (единственный такой файл из 180) | Добавить `\n` в конец файла |

## Что проверено и нарушений не найдено (сводка I)

- **Async**: только `ExecuteAsync`/`ExecuteAsync<T>`; нет `Execute(`, `async void`, `.Result`/`.Wait()`/`GetAwaiter().GetResult()` синхронизации.
- **Error handling**: нет `catch (Exception)`; `try/catch` в TestRunReportGenerator/TestRunWorkspace/RunCleanupAsync используют конкретные типы и логирование; исключения не используются для flow control; `nameof`-нарушений нет.
- **LINQ**: нет `Count() > 0`, `Where(...).FirstOrDefault()`; `.ToList()` в цепочках — только материализация перед повторным использованием — допустимо.
- **Loops**: все `foreach` по `List<T>` в синхронных телах либо async-teardown, либо итераторы/`yield`, либо lazy-источники — по исключениям правила; единственный вынос — AllureTestResultBuilder:91 (major выше).
- **Regex**: все паттерны в `private static readonly Regex` c комментарием-описанием, `@"..."` и `RegexOptions.Compiled`; инлайновых `Regex.IsMatch/Match/Replace` нет.
- **Structure/naming**: file-scoped namespaces везде; по одному типу на файл; нет `var`-заменяемых явных типов; нет одно-буквенных лямбд; нет аббревиатур; нет голых `if/foreach/while` без `{}`; нет вложенных тернарников; `using NUnit.Framework` отсутствует в Api/E2E (глобальный импорт); trailing whitespace отсутствует; `private`-поля в `_camelCase`, константы — PascalCase; параметров >5 нет; guard clauses применены; SOLID-нарушений по placement не выявлено.
- **Magic numbers**: остальные числовые литералы либо в `const`-полях, либо в `[Description]`/`HttpStatusCode` — кроме `RandomString(8)` (major выше).

_Примечание: проверка мёртвых `using` выполнена эвристически; итог по IDE0005 даёт только `dotnet format --verify-no-changes`._

---

# II. Код: models / assertions / test-practices / categories / comments (~200 .cs)

**Метод:** правила прочитаны целиком; просмотрены все `.cs` комбинированным сканом (скрипты по атрибутам/методам/паттернам) + ручная верификация выборочных находок чтением файлов.

## MAJOR

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| major | `Api/GitHub/Tests/Issues/CreateIssueVisibilityTests.cs:33`; `Api/GitHub/Tests/IssueComments/CreateCommentTests.cs:67-83` | test-practices → Read-after-write ("Mandatory for every POST flow", "Baseline GET + POST go in [OneTimeSetUp]; tests only read and assert") | Обязательная каноническая пара `*_ReturnsCreatedRecord` (Smoke) + `*_CreatedRecordMatchesRequest` (Regression) с baseline в `[OneTimeSetUp]` отсутствует для GitHub POST-флоу (CreateIssue, CreateRef, CreatePullRequest); в CreateComment baseline GET (67) + POST (71) выполняются внутри `[Test]`, а `MatchesRequest` (48) — Smoke и по POST-ответу, не GET read-back. Частично покрыто E2E-цепочками (CommentChainTests.cs:62 HaveCount, IssueLifecycle, PullRequestFlow) иными именами/категориями; для JSONPlaceholder исключение задокументировано (JP-002) | Либо добавить недостающие тесты в канонической форме (baseline+POST в OneTimeSetUp, разбивка на Smoke/Regression), либо зафиксировать в Rules/test-practices.md отклонение "паттерн применяется в E2E-цепочках" (правило сейчас противоречит практике проекта) |
| major | см. таблицу ниже (35 файлов, ~100 методов — ОДНА тема) | test-practices: "Status check before every `.Data` dereference" (стр. 12) | Тестовые тела разыменовывают `response.Data` (в т.ч. `Data!`, `.Should()`, агрегаты) до любого `ShouldHaveStatusCode`/`ShouldHaveError`; при 500 падение будет NRE/невнятным сообщением вместо "expected 200, got 500". `RequestHelper` статус сам не проверяет. Скан подтверждён ручными чтениями (CreatePostTests.cs:55, GetRateLimitTests.cs:45, GetIssuesTests.cs:73, GetAlbumsByUserTests.cs:50 и др.) | Перед первым обращением к `.Data` добавить `response.ShouldHaveStatusCode(...)` (для негативов — `ShouldHaveError`) |

### Файлы темы «status-first» (major, один паттерн)

| File:line |
|---|
| `Api/GitHub/Tests/AuthRepos/GetAuthenticatedUserReposTests.cs:46` |
| `Api/GitHub/Tests/Branches/GetBranchByNameTests.cs:67,78` |
| `Api/GitHub/Tests/Branches/GetBranchesTests.cs:51,110,122` |
| `Api/GitHub/Tests/IssueComments/GetIssueCommentsTests.cs:105,116` |
| `Api/GitHub/Tests/Issues/GetIssueByIdTests.cs:74,125,138` |
| `Api/GitHub/Tests/Issues/GetIssuesTests.cs:73,84,96` |
| `Api/GitHub/Tests/PublicRepos/GetPublicReposTests.cs:49` |
| `Api/GitHub/Tests/PullRequests/GetPullRequestCommitsTests.cs:67,80,114` |
| `Api/GitHub/Tests/PullRequests/GetPullRequestFilesTests.cs:68,107,118` |
| `Api/GitHub/Tests/PullRequests/GetPullRequestsTests.cs:51,62,74` |
| `Api/GitHub/Tests/RateLimit/GetRateLimitTests.cs:45,55,78,92` |
| `Api/GitHub/Tests/Repos/GetRepositoryTests.cs:54,114,128` |
| `Api/GitHub/Tests/UserRepos/GetUserReposTests.cs:51` |
| `Api/GitHub/Tests/Users/GetUserTests.cs:48,92,103` |
| `Api/JsonPlaceholder/Tests/Albums/GetAlbumByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Albums/GetAlbumsByUserTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Albums/GetAllAlbumsTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Comments/GetAllCommentsTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Comments/GetCommentByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Comments/GetCommentsByPostTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Photos/GetAllPhotosTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Photos/GetPhotoByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Photos/GetPhotosByAlbumTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Posts/CreatePostTests.cs:45,55,67,77,87` |
| `Api/JsonPlaceholder/Tests/Posts/DeletePostTests.cs:41` |
| `Api/JsonPlaceholder/Tests/Posts/GetAllPostsTests.cs:85,118,129` |
| `Api/JsonPlaceholder/Tests/Posts/GetPostByIdTests.cs:81,92,169` |
| `Api/JsonPlaceholder/Tests/Posts/UpdatePostTests.cs:52,74,85,96,119` |
| `Api/JsonPlaceholder/Tests/Todos/GetAllTodosTests.cs:85,118` |
| `Api/JsonPlaceholder/Tests/Todos/GetTodosByUserIdTests.cs:69,80,91,122` |
| `Api/JsonPlaceholder/Tests/Users/GetAllUsersTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Users/GetUserAlbumsTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Users/GetUserByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Users/GetUserPostsTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Users/GetUserTodosTests.cs:50,61,72` |

*Примечание: `E2E/GitHub/Tests/CommentChainTests.cs:36` (`_createdIssueNumber = issue.Data.Number` в `if (Data is not null)`) — это предписанное register-then-assert, НЕ нарушение. E2E и Ui в остальном чисты по этому правилу.*

## MINOR

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| minor | `Core/Helpers/GitHub/GitHubParamHelper.cs:52` | models.md → JSON field names (wire-ключи тестовых хелперов используют JsonFields; копии запрещены) | `ParamHelper.Query("state", ...)` — инлайн-литерал, при этом `JsonFields.State` уже существует | Использовать `JsonFields.State` |
| minor | `Core/Helpers/GitHub/GitHubParamHelper.cs:29,34`; `Api/JsonPlaceholder/Helpers/JsonPlaceholderParamHelper.cs:13,16` | models.md → JSON field names ("New wire name → add the constant first") | Wire-query-ключи `"page"`, `"per_page"`, `"postId"`, `"albumId"` заданы инлайн; констант в JsonFields нет | Добавить константы в соответствующие `JsonFields.cs`, заменить в хелперах |
| minor | `Core/Helpers/Params/ParamHelper.cs:7`; `Core/Helpers/GitHub/GitHubTestBase.cs:18` | models.md → "`TitleKey`-style aliases are banned; the constant IS the key" | Алиасы `IdKey = "id"` и `SinceParamKey = "since"` живут в хелперах; `id`/`since` — wire-имена | Перенести `"id"` (Generic) и `"since"` (GitHub) в `JsonFields`, использовать напрямую (как уже сделано для `UserIdParam`) |
| minor | `Api/GitHub/Tests/Issues/CreateIssueNegativeTests.cs:17-18` | assertions.md: "message constants live in `Core/Config/GitHubErrors.cs`" | Константы документированных ошибок (`MissingTitleErrorMessage`, `LabelsNullErrorMessage`) объявлены локально в тест-классе | Перенести в `Core/Config/GitHubErrors.cs` |
| minor | `Api/GitHub/Tests/GitRefs/CreateRefTests.cs:51`; `Api/GitHub/Tests/Languages/GetLanguagesTests.cs:34` | categories.md + assertions.md (ContractCheck → `ShouldHaveValidContract()`) | Тесты с `[Category("ContractCheck")]` не вызывают `ShouldHaveValidContract()` — ручные сравнения/свой round-trip | Для CreateRef: сменить категорию либо добавить `ShouldHaveValidContract`; для Languages — обоснование (Dictionary) формализовать в правиле или использовать helper |
| minor | `Api/GitHub/Tests/IssueComments/CreateCommentTests.cs:48`; `Api/GitHub/Tests/GitRefs/CreateRefTests.cs:51` | categories.md → Category-to-test mapping / Boundary (ShouldMatchRequest-инвариант → Regression) | `CreateComment_MatchesRequest` — `Smoke`; `CreateRef_MatchesRequest` — `ContractCheck`. В проекте аналоги (`CreatePost_ShouldMatchRequest`, `UpdatePost_Put_ShouldMatchRequest`) — `Regression` | Переклассифицировать оба в `Regression` |
| minor | `Api/GitHub/Tests/GitRefs/CreateRefTests.cs:77`; `E2E/GitHub/Tests/CollaboratorPermissionTests.cs:46,51,81` | test-practices Step 3 / code: magic strings → `{Service}Endpoints.cs` | Инлайн-литералы в assertion-значениях: `"commit"`, `"admin"`, `"read"` (аналоги `StateOpen`/`StateClosed` уже вынесены) | Вынести в `GitHubEndpoints` (или `GitHubErrors`) |
| minor | `Api/GitHub/Tests/PullRequests/GetPullRequestsTests.cs:30` | test-practices → Guarantee Data ("If POST is unavailable — use [Ignore]") | `[Ignore("Creating PR requires branch + commit — too complex for Setup")]` — причина устарела: `GitHubTestBase.CreateScratchPullRequestAsync` существует и используется соседними тестами; активные соседи (`HasValidFields`:51 и др.) на данные полагаются | Снять Ignore и включить scratch-pipeline в `[OneTimeSetUp]` (guarantee data), либо обновить причину фактической ссылкой на ограничение |
| minor | `Api/GitHub/Tests/IssueComments/CreateCommentTests.cs:21-24` | test-practices → Test data for write ("Vary ≥2 dimensions … If one dataset is intentionally enough, document why") | Один `static readonly _testComment` — единый body для всех write-тестов класса; комментария/документации, почему достаточно одного набора, нет (варьируется только per-run `RandomString`) | Варьировать body по тестам (или зафиксировать в классе обоснование) |

## STYLE

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| style | `Ui/Helper/BrowserHelpers/BrowserPw.cs:6-8` | comments.md (по умолчанию без комментариев; только regex/TODO/non-obvious WHY) | Заголовок описывает структуру/что делает код ("every page-object receives the wrapper in its constructor; pages are exposed here as properties…") — пограничен с запретом "what the code does" | Сократить до WHY (зачем паттерн нужен) или удалить |
| style | `Rules/models.md:82` ↔ `Rules/assertions.md:18-19` + все `Core/Models/**` | models.md "Don't add validation attributes" vs assertions.md, полагающийся на `[RequiredField]`/`[PositiveId]`/`[ValueRange]` | Прямое противоречие правил: модели массово используют validation-атрибуты (ContractCheck-инфраструктура), что запрещено models.md | Синхронизировать models.md (разрешить атрибуты contract-check) — правка документа, не кода |

## Проверено — нарушений не найдено (сводка II)

- **models.md:** суффиксы/лейаут (Requests/Responses/root, Generic) — чисто; конструкторы/методы/`init`/inline `[JsonPropertyName]`/атрибуты на одной строке — 0 (скан всех 60 файлов `Core/Models/**`); неймспейсы `Core.Models.{Service}` — ок.
- **assertions.md:** NUnit `Assert.*`/`Assert.Multiple` — 0; `.Should().Be(HttpStatusCode…)` вместо `ShouldHaveStatusCode` — только внутри `AssertHelper` (легитимно); `JsonElement`/`TryGetProperty` в тестах — 0; `ShouldMatchRequest` применён в PATCH-эхо и create-comment; исключения nested-shape учтены; guard `if (Count > 0)` только у documented-empty c Smoke-тестами пустого состояния; aggregate-правило — ок.
- **categories.md:** скриптовая проверка всех `*Tests.cs`: ровно одна service-категория на класс, ровно одна check-type на `[Test]` — 0 отклонений; `[Ignore]`-тесты категоризированы.
- **test-practices.md:** `[OneTimeTearDown]` — последний член во всех 23 классах с teardown; в teardown нет status-assert и `.Data`; вспомогательных методов в `*Tests.cs` — 0; `finally`-cleanup — 0; все `[OneTimeSetUp]` — status immediately after call; register-then-assert соблюдён; динамические несуществующие ID: GitHub → `maxId + NonExistentIdOffset` (100), JP → `maxId + 1`, статических 999/1000 — 0; WaitHelper — только где нужен, диагностика в сообщениях есть; fake-API тесты `[Ignore]` с ссылками на JP-001/002/004; write-данные — вымышленные, через `DataGenerator.RandomString`; cleanup-вызовы только в OTD/setup.
- **comments.md:** 65 однострочных комментариев проверены — regex-объяснения и non-obvious WHY допустимы; `// ====`, AAA-разметки, `TODO`/`FIXME`, `/* */`, `///` — 0. Единственная пограничная — BrowserPw (см. STYLE).

---

# III. Консистентность документов и конфигурации

## 1. AGENTS.md ↔ Rules/*.md

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `AGENTS.md:9` (Project Structure → Ui/) ↔ `Rules/ui-testing.md:34-36` | Утверждение «авторизация через storageState (bootstrap = `[Explicit]`-тест)» не соответствует коду: в `Ui/` нет ни одного `[Explicit]`-атрибута и нет теста `StorageStateBootstrap` (grep по репо — 0 совпадений в коде). Фактически автоматический Smoke-тест входа `Ui/Test/GitHubStream/Login/LoginSignInTests.cs` (`IsStorageStateEnabled => false`, `SaveStorageStateAsync()` после assert). Две локально-«старые» формулировки в AGENTS и Rules конфликтуют с реальностью | Переформулировать в обоих местах: bootstrap state обновляется автотестом `LoginSignInTests.Login_SignIn_SavesStorageState` (либо вернуть `[Explicit]`-тест в код и оставить текст) |
| major | `AGENTS.md` (Git: «Перед коммитом: `dotnet format --verify-no-changes` + `dotnet test`») ↔ `AGENTS.md` (AI Onboarding: «`/commit` — форматирование + HealthCheck + коммит») ↔ `.mimocode/skills/commit/SKILL.md:53-59` | Внутри AGENTS две разные формулировки pre-commit гейта: полный `dotnet test` vs только `--filter Category=HealthCheck` (skill фактически запускает HealthCheck). Читатель решит, что перед коммитом проходит весь сьют | Зафиксировать один вариант (например «format + HealthCheck-гейт, полный прогон — `/test`») в AGENTS Git-секции и Rules |
| minor | `AGENTS.md:17` («0 и -1 — безопасные static IDs») ↔ `Rules/test-practices.md:29` («Never hardcode 999, 1000, or **any static number**») | Абсолютная формулировка правила конфликтует с разрешёнными статическими невалидными значениями 0/-1 (сам же `test-practices.md:31` разрешает явные 0/negative) — повод для взаимно-противоположных правок при ревью | Уточнить п.29: запрещены статически заданные *потенциально существующие* ID; 0/-1 — исключение |

## 2. Rules/ui-testing.md и ui-test-gen/SKILL.md ↔ реальность Ui/

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `Rules/ui-testing.md:34` | «Tests … **never log in** or log out» — `LoginSignInTests` логинится парой `TestConfig.UiLogin/UiPassword` в каждом запуске | Описать фактическую модель: обычные тесты читают storageState, отдельный login-тест работает в анонимном контексте |
| major | `Rules/ui-testing.md:35` | «The one-time **manual** login is the `[Explicit]` bootstrap test (`dotnet test --filter StorageStateBootstrap`)» — такого теста в проекте нет; фильтр ничего не найдёт | Заменить на фактический путь обновления state (`dotnet test --filter LoginSignInTests`) |
| major | `Rules/ui-testing.md:36` | «`GITHUB_UI_PASSWORD` lives in `.env` **only if a password flow is ever added**» — password flow уже добавлен (`LoginPage.SignInAsync`, обязательный env `GITHUB_UI_PASSWORD` в `TestConfig.cs:44-45`, требование в `backups/SETUP.md` Ч.4) | Убрать «if ever added», прямо требовать `GITHUB_UI_EMAIL`/`GITHUB_UI_PASSWORD` |
| major | `Rules/ui-testing.md:46-47` | Cleanup «через Core API helpers (`CleanupIssueAsync`, `CleanupCommentAsync` → `RunCleanupAsync`)» — в `Ui/` этих членов нет: `GitHubUiTestBase` не наследует `GitHubTestBase`/`RequestHelper` (grep по `Ui/` — 0 совпадений). Правило описывает несуществующую проводку | Документировать реальный механизм (наследование от `GitHubTestBase` или композиция) либо добавить wiring в фикстуру |
| major | `.mimocode/skills/ui-test-gen/SKILL.md:17` | Hard rule 2: «No login/logout in tests … login is the `[Explicit]` bootstrap test only» — неверно по той же причине | Обновить под `LoginSignInTests` |
| major | `.mimocode/skills/ui-test-gen/SKILL.md:205,207` (Troubleshooting) | «run `[Explicit]` bootstrap test», «`dotnet test --filter StorageStateBootstrap`» — фильтр не находит тестов; агент будет водить юзера по несуществующей команде | Заменить на фактический тест входа |
| major | `.mimocode/skills/ui-test-gen/SKILL.md:136-158` (Step 4 template) | Шаблон UI-теста вызывает `Get<List<IssueModelResponse>>(...)`, `ShouldHaveStatusCode`, `TestRepoParam()`, `CleanupIssueAsync(id)` — ни одного из этих членов в Ui-фикстуре нет → сгенерированный код не скомпилируется | Либо подключить API-инфраструктуру в `GitHubUiTestBase`, либо переписать шаблон под реальные члены |
| minor | `Rules/ui-testing.md:15`, `documentation/FILE_STRUCTURE.md:326` | Секция структуры перечисляет `Ui/Helper/UiHelper/BaseHelpers/` — каталога на диске нет (есть только `CommonPages/`, `GitHubStream/` с `.gitkeep`) | Убрать строку или создать каталог |
| minor | `Rules/ui-testing.md:60`, `ui-test-gen/SKILL.md:72` | Примеры `[Description("UI-3.2 Create issue via UI")]` — в `GitHubUiObservableBehaviour.md` нет секции UI-3.x; нумерована только UI-1, остальные страницы без номеров | Ввести сквозную нумерацию UI-2… или поправить примеры на существующие секции |
| minor | `ui-test-gen/SKILL.md:54` | Preconditions ссылаются на `.mimocode/plans/ui-playwright-github.md` — `plans/` gitignored, в клоне ссылка мертва | Ссылаться на трекаемый документ или убрать путь |

Без находок: файловые конвенции skill (`Ui/Test/GitHubStream/{area}/{area}{flow}Tests.cs`, namespace `Ui.Test.GitHubStream.Login`, `[Category("GitHubUi")]`) соответствуют диску; `UiTimeoutMs=15s`, `TestResults/ui-artifacts/{name}.png|.zip`, `CommonPages/{LoginPage,HeaderPage}` — совпадают.

## 3. documentation/FILE_STRUCTURE.md ↔ дерево файлов

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| minor | `documentation/FILE_STRUCTURE.md:335-347` (Rules/) | Отсутствует запись `ui-testing.md` (12 файлов на диске, 11 в дереве) | Добавить строку |
| minor | `FILE_STRUCTURE.md:38-50` (.mimocode/skills/) | Отсутствует `ui-test-gen/SKILL.md` (13 скиллов на диске, 12 в дереве) | Добавить строку |
| minor | `FILE_STRUCTURE.md:352-378` (documentation/) | Отсутствует `GitHubUiObservableBehaviour.md` — основной OB для UI-тестов не отражён | Добавить строку |
| minor | `FILE_STRUCTURE.md:51-54` (Scripts/) | Отсутствует `install-playwright.ps1` (есть на диске; упоминается в ui-test-gen Preconditions) | Добавить строку |
| minor | `FILE_STRUCTURE.md:315-330` (Ui/) | Перечислен несуществующий `BaseHelpers/`; фактические папки `CommonPages/` (2 файла) и `GitHubStream/` (только `.gitkeep`) описаны как «page-objects: {Area}/{Area}Page.cs» — в GitHubStream page-objects пока нет | Привести дерево к факту |
| minor | `FILE_STRUCTURE.md` (Root) | Не отражены корневые `TestAdapter/` (на диске только `obj/`, без `.csproj` — мусорный каталог), `.auth/`, `.playwright-cli/` (при этом `allure-results/`, `TestResults/` перечислены) | Задокументировать или удалить `TestAdapter/` (правило workflow #6 про удаления gitignored-файлов) |
| style | `FILE_STRUCTURE.md:26-37` (.mimocode/) | `hooks/safety-commit.ts`, `package.json`, `package-lock.json`, `.mimocode/.gitignore` показаны обычными записями, хотя не отслеживаются git (проверено `git ls-files`: их нет; игнор — `.mimocode/.gitignore`); маркер «not tracked» стоит только у `node_modules`, `plans`, `reviews` | Проставить «not tracked» согласованно |

Совпадает точно: секции Core/, Api/, E2E/, AllureAdapter/, Bugs/, Katas/ — полное соответствие диску.

## 4. documentation/*ObservableBehaviour.md ↔ тесты

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `documentation/GitHubUiObservableBehaviour.md:25` (What AI does #7) | «Never logs in/out inside tests — storageState only; login = `[Explicit]` bootstrap» противоречит §«Page: Login (UI-1)» того же файла («the login test overrides `IsStorageStateEnabled => false`») и самому `LoginSignInTests` — внутренняя двусмысленность OB | Оставить одну фактическую формулировку |
| major | `GitHubObservableBehaviour.md` ↔ `Api/GitHub/Tests/**` | Нумерация `[Description]` не соответствует нумерации секций OB: `GetIssuesTests` «3.x» vs §6; `GetIssueByIdTests` «10.x» vs §7; `GetIssueCommentsTests` «11.x» vs §8; `GetBranchesTests` «4.x» vs §10; `GetPullRequestsTests` «5.x» vs §9; `GetAuthenticatedUserReposTests` «9.x» vs §5; `GetUserTests` «2.x» vs §3; `GetPublicReposTests` «8.x» vs §2; `GetUserReposTests` «7.x» vs §4; `GetRateLimitTests` «6.x» vs §16 (совпадают только §1/1a/3a/11/12-15b и write-§17-26) | Перенумеровать Descriptions по OB (или зафиксировать каноническую нумерацию в Rules/test-practices) |
| major | `Api/GitHub/Tests/Auth/AuthNegativeTests.cs` ↔ `Api/GitHub/Tests/Contributors/GetContributorsTests.cs` | Коллизия: обе используют префикс «12.x» — `12.1` означает то «GET /user/repos invalid token 401», то «GET /contributors 200» (OB §12 = contributors) — ссылки в Description неоднозначны | Перенумеровать одну из групп |
| minor | `GitHubUiObservableBehaviour.md:40-54` | Секции «Repository landing», «Issues list», «Issue detail», «Issue create form» не пронумерованы (нет UI-2…UI-5), хотя Rules/skills требуют ссылок вида `UI-N` | Пронумеровать секции при добавлении верификации |
| minor | `GitHubUiObservableBehaviour.md:7`, `GitHubTestingStructure.md:23` | Ссылки на `.mimocode/plans/*` — каталог gitignored, в чистом клоне ссылки мертвают | Ссылаться на трекаемые доки |
| minor | `E2E/GitHub/Tests/*` (`E2E-7`, `E2E-8`, `E2E-9`) | Префиксы `[Description("E2E-N …")]` не описаны ни в одном документе (grep `E2E-\d` по documentation/ — 0) — нумерация в никуда | Добавить секции E2E-N в документ или изменить формат |
| minor | `JsonPlaceholderObservableBehaviour.md` ↔ `Api/JsonPlaceholder/Tests/**` | Три несовпадающие нумерации: Descriptions рестартуют в каждой группе (`GetAllComments` «1.1», `CreatePost` «4.1», `GetAllTodos` «7.1»), JP OB — глобальные §1-22 (POST /posts = §19), TestPlan inventory — 1.1-6.5 (POST /posts = 1.3) | Зафиксировать канон (например: Description = номер секции OB) |

Без находок: статусы `draft — no verified locators yet` стоят у всех непроверенных секций UI OB; маркеры `verified {date}` есть у всех локаторов UI-1; `[Description("UI-1 Ordinary login…")]` в `LoginSignInTests.cs:19` корректно ссылается на секцию «Page: Login (UI-1)».

## 5. testsettings.example.json ↔ testsettings.json

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| — | `testsettings*.json` | **Находок нет**: ключи всех секций (`MaxResponseTimeMs`, `SkipSslValidation`, `JsonPlaceholder`×6, `GitHub`×5, `Ui`×4) идентичны, секция `Ui` присутствует в обоих | — |

## 6. backups/SETUP.md ↔ реальность

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| minor | `backups/SETUP.md` Ч.6 (дерево `Scripts/`) | «Скрипты (allure-report.ps1, test-coverage.ps1)» — не упомянуты `install-playwright.ps1` и `normalize-eol.ps1` (последний обязателен по `/commit` Step 7) | Дописать оба скрипта |
| minor | `backups/SETUP.md` Ч.6 (дерево `Core/Config/`) | Перечислены только `JsonPlaceholderEndpoints.cs`, `GitHubEndpoints.cs` — нет `TestConfig.cs` (сам ссылается в Ч.4) и `GitHubErrors.cs` | Дописать |
| minor | `backups/SETUP.md` Ч.7.2 (таблица скиллов) | 11 из 13 скиллов: нет `/audit` и `/review` | Добавить строки |
| style | `backups/SETUP.md` Ч.6 (дерево `documentation/`) | Нет `Katas/`, `token-budget.md`; дерево корня без `backups/`, `TestResults/` | Дополнить дерево |
| style | `backups/SETUP.md` Ч.3.2 | «В папке проекта **уже есть** `.mimocode/mimocode.jsonc`» — файл отслеживается git, но `hooks/`, `package.json`, `package-lock.json` рядом — untracked; формулировка «уже есть» вводит в заблуждение при клоне | Уточнить, что трекается и что восстанавливается из `backups/` |

Без находок: переменные окружения `GITHUB_PAT`, `GITHUB_UI_EMAIL`, `GITHUB_UI_PASSWORD` упоминаются и соответствуют `Core/Config/TestConfig.cs:32,44,45`; порядок чтения токена (JSON → env → `.env`) совпадает с кодом; `GitHubUiObservableBehaviour.md` присутствует и в дереве Ч.6, и в перечислении OB Ч.12 (синхронизировано в коммите 504ef48); команды (`dotnet test`, фильтр HealthCheck, `/test`, `/commit`) соответствуют скиллам.

## 7. .mimocode/skills/* ↔ Rules (One canonical source, `Rules/test-practices.md:84`)

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `.mimocode/skills/audit/SKILL.md:40` | «Magic strings (**3+** occurrences → const)» противоречит `Rules/code-style.md:38` («**2+** times») — аудит будет пропускать нарушения | Ссылаться на правило, не дублировать порог |
| major | `.mimocode/skills/e2e-test-gen/SKILL.md:126-170` | Обязательный шаблон с `[SetUp]`/`[TearDown]` на каждый тест и cleanup в per-test `TearDown` противоречит `Rules/test-practices.md:44-48` и AGENTS (cleanup только в `[OneTimeTearDown]` последним членом, fixture-registry, register-then-assert); фактический код E2E (`IssueLifecycleTests`) использует `[OneTimeTearDown]` | Переписать шаблон под fixture-registry |
| major | `.mimocode/skills/e2e-test-gen/SKILL.md:174-187` (Step 5) | «add helper methods (`CleanupComments`)» в тестовом классе противоречит `Rules/test-practices.md:17` («No helper methods inside `*Tests.cs`») и hard rule 5 самого ui-test-gen | Убрать или перенести в base/helpers |
| major | `.mimocode/skills/api-test-gen/SKILL.md:48,120` | Путь param helper `Api/Helpers/{Service}ParamHelper.cs` — каталога `Api/Helpers/` не существует (фактически `Api/JsonPlaceholder/Helpers/JsonPlaceholderParamHelper.cs`; для GitHub — `Core/Helpers/GitHub/GitHubParamHelper.cs`) | Исправить конвенцию путей |
| major | `.mimocode/skills/api-test-gen/SKILL.md:49,148` | Namespace `Api.{Service}.Tests` — фактические неймспейсы по подпапкам: `Api.JsonPlaceholder.Posts`, `Api.GitHub.Issues` и т.д. | Исправить на `Api.{Service}.{Category}` |
| major | `.mimocode/skills/api-test-gen/SKILL.md:81-83,131,283` | Суффиксы моделей «`Model` for response, `Request` for request» + пример `PostModel` против канона `ModelResponse`/`ModelRequest` (`Rules/models.md`, AGENTS, gredja-rules) — генерация сразу нарушит naming-правило | Привести к `ModelResponse`/`ModelRequest` |
| major | `.mimocode/skills/review/SKILL.md` (Step 2, список файлов) | Задача «Read ALL .cs files», но перечень содержит несуществующие `Ui/Tests/*.cs` и `Ui/AllureGlobalSetup.cs` (фактически `Ui/Test/**`, `Ui/TestReportSetup.cs`) и не содержит `Api/GitHub/Tests/**`, `E2E/**` — «полное ревью» молча пропускает половину репо | Исправить пути и дополнить список |
| minor | `.mimocode/skills/api-test-gen/SKILL.md:268-269` | «`[SetUp]`/`[TearDown]` are for E2E tests» — перекладывает противоречие с Rules (cleanup — только `[OneTimeTearDown]`) на e2e-test-gen | Убрать или сослаться на актуальный паттерн |
| minor | `.mimocode/skills/api-test-gen/SKILL.md:214,224` | `{Service}Endpoints.MaxResponseTimeMs` / `{Service}Endpoints.Expected{Endpoint}Count` — `MaxResponseTimeMs` живёт в `TestConfig` (testsettings), не в Endpoints-классах | Поправить ссылки |
| minor | `.mimocode/skills/api-test-gen/SKILL.md:47` | Tests path без подпапки категории: `Api/{Service}/Tests/{Endpoint}Tests.cs` vs реальные `Api/{Service}/Tests/{Category}/*.cs` | Уточнить паттерн |
| minor | `.mimocode/skills/e2e-test-gen/SKILL.md:216-217` | Таблица «api-test-gen (read-only) / **GET only**» устарела: `Api/` содержит write-тесты (CreatePost, CreateIssue*, UpdateIssue, Create/DeleteComment, PR-create/update/merge, git refs) | Убрать «GET only» |
| minor | `.mimocode/skills/gredja-rules/SKILL.md:17-28` | Таблица Rule Files пропускает `code-style.md`, `code-principles.md`, `test-practices.md`, `categories.md`, `ui-testing.md` — entry point ведёт не к полному набору правил | Дополнить таблицу |
| minor | `.mimocode/skills/gredja-rules/SKILL.md` (Key Rules) | «`Id` **always non-nullable**» vs `Rules/models.md:33-35` (value types — `?` если JSON-поле может быть null/absent) | Смягчить формулировку |
| minor | `.mimocode/skills/review-pr/SKILL.md:31` | «Models: Model/Request suffix» — размыто/устарело относительно `ModelResponse`/`ModelRequest` | Уточнить |
| style | `review/`, `review-commit/`, `audit/`, `gredja-rules/` | Инлайновые копии формулировок Rules внутри скиллов — дрейф уже случился (audit 3+ vs 2+); правило «One canonical source per guidance topic» (test-practices.md:84) нарушается вторым источником правил | Заменять копии ссылками `Rules/*.md` |

## 8. .gitignore ↔ артефакты в корне

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| style | `.gitignore:41` и `.gitignore:48` | `testsettings.json` продублирован: секция «Secrets» и отдельная строка `/testsettings.json` | Оставить одну запись |
| style | `.mimocode/.gitignore` | Игнорирует `hooks/`, `package.json`, `package-lock.json`, `.gitignore` — при этом `documentation/FILE_STRUCTURE.md` показывает их обычными записями (см. §3) | Синхронизировать пометки «not tracked» |

Проверено — **все целевые пути закрыты**: `.playwright-cli/` (стр.51), `.auth/` (54), `TestResults/` (17, включая `Api/TestResults`, `E2E/TestResults`), `.env` (33), `testsettings.json` — `git check-ignore` подтверждает; также закрыты `allure-results/`, `bin/obj`, `.mimocode/plans|reviews`, `.mimocode/.cron-lock`.

---

**Статистика раздела III:** 24 major, 24 minor, 6 style. Главный сквозной дефект — «старая» модель UI-авторизации (`[Explicit] StorageStateBootstrap`, «тесты никогда не логинятся»), застрявшая одновременно в `AGENTS.md`, `Rules/ui-testing.md`, `ui-test-gen/SKILL.md` и `GitHubUiObservableBehaviour.md`, при наличии на диске автоматического `LoginSignInTests`; второй по опасности кластер — устаревшие пути/неймспейсы/суффиксы в `api-test-gen` и пер-тестовый cleanup-шаблон в `e2e-test-gen`, противоречащие Rules и фактическому коду.
