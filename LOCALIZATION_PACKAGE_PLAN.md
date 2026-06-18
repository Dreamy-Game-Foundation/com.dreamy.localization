# Dreamy Localization Package - Implementation Plan

## 1. Document Status

| Field | Value |
|---|---|
| Package | `com.dreamy.localization` |
| Target | DreamyBase projects |
| Unity baseline | Unity 6 |
| Data source | CSV-first |
| Localization engine | Unity Localization package |
| Planned Unity Localization line | `1.5.x`, verify and pin the latest Unity 6-compatible patch during Phase 0 |
| Plan status | Ready for implementation |
| Reference repository | [tranvietanh0/UnityLocalization](https://github.com/tranvietanh0/UnityLocalization) |

## 2. Objective

Build a reusable Unity Package Manager package that standardizes localization across DreamyBase games while keeping Unity Localization as the underlying table, locale, Smart String, and Addressables implementation.

The package must provide:

- CSV as the source of truth for localized strings and localized asset mappings.
- Safe import, validation, preview, merge, export, and round-trip workflows.
- Runtime APIs for text, TextMeshPro, UGUI, Smart Strings, plurals, generic assets, sprites, textures, audio, and locale switching.
- Editor tooling to bootstrap projects, scan localization gaps, import data, validate content, and report actionable errors.
- CI/batchmode validation suitable for release gates.
- Extensibility without forcing DreamyBase games to depend directly on editor implementation details.

## 3. Reference Repository Assessment

The reference repository provides a useful minimum implementation:

- CSV parser and schema validation.
- CSV import into Unity String Table Collections.
- Locale and settings bootstrap.
- Hardcoded TMP and source-literal scanning.
- Batchmode validation.
- A runtime `LocalizedTmpText` component.
- Basic EditMode parser tests.

The Dreamy package should reuse the concepts, not copy the constraints.

### Gaps to address

| Area | Reference behavior | Dreamy requirement |
|---|---|---|
| Configuration | Hard-coded paths, `en`, `vi`, and `UI` | Project-level profile asset with configurable locales, paths, tables, policies, and fallback |
| CSV lifecycle | Import-only custom schema | Versioned schema, dry-run, merge/replace policy, export, deterministic round-trip, migration |
| Asset localization | Not covered | Asset manifest import into Asset Tables and typed runtime bindings |
| UI support | TMP helper only | TMP, UGUI `Text`, generic callback/event binding, built-in Unity components interoperability |
| Smart Strings | Boolean import flag | Argument APIs, persistent variables interoperability, plural/select validation |
| Locale control | Not exposed as service | Async initialization, selection, persistence, fallback, events, and cancellation |
| Scanner | Project-specific path and regex | Configurable scopes, prefab/scene/code analysis, suppressions, stable findings |
| Import safety | Writes directly after validation | Plan/diff/apply transaction, backups or rollback boundary, deterministic report |
| Validation | Basic critical/warning list | Rule registry, severity policy, machine-readable report, CI exit codes |
| Tests | Parser-focused | Runtime, editor integration, import idempotency, asset mapping, locale switching, PlayMode |
| Package quality | Minimal package metadata | Samples, documentation, changelog, licenses, API docs, package validation |

## 4. Scope

### 4.1 Required runtime capabilities

- Initialize localization explicitly or through an optional bootstrap component.
- Read the current locale and available locales.
- Select locale by locale code.
- Persist the selected locale using an injectable preference store.
- Apply configured fallback behavior when a locale or entry is unavailable.
- Notify listeners when initialization or locale changes complete.
- Resolve localized strings synchronously only when Unity has already loaded the required data.
- Resolve localized strings asynchronously as the default safe API.
- Pass positional or named Smart String arguments.
- Support plural, select, conditional, list, date, number, currency, and custom Smart String formatters provided by Unity Localization.
- Bind localized strings to:
  - `TMP_Text`.
  - `UnityEngine.UI.Text`.
  - `UnityEvent<string>`.
  - A runtime callback or interface for custom UI.
- Bind localized assets to:
  - `Sprite`.
  - `Texture`.
  - `Texture2D`.
  - `AudioClip`.
  - `Font`.
  - `TMP_FontAsset`.
  - Generic `UnityEngine.Object` assets through `LocalizedAsset<T>`.
- Refresh active bindings when the locale changes.
- Avoid event leaks when components are disabled, destroyed, or reconfigured.
- Expose loading/failure state without silently swallowing missing-table or missing-entry failures.

### 4.2 Required editor capabilities

- Create or repair Unity Localization settings.
- Create configured locale assets.
- Configure startup locale selectors and fallback locale behavior.
- Create String Table and Asset Table Collections.
- Import all configured CSV files.
- Export tables to deterministic CSV.
- Preview an import diff before applying changes.
- Support merge and authoritative replace modes.
- Scan scenes, prefabs, ScriptableObjects, and configurable source folders.
- Detect missing references, duplicate keys, invalid locale columns, missing translations, orphaned entries, and likely hardcoded UI text.
- Validate Smart String syntax and representative argument contracts.
- Validate asset GUID/path/type mappings.
- Produce human-readable and JSON validation reports.
- Run all validation from Unity batchmode.

### 4.3 Supported interoperability

- Unity Localization native `LocalizedString`.
- Unity Localization native `LocalizedAsset<T>`.
- Unity `LocalizeStringEvent`.
- Unity `LocalizeAssetEvent<TObject, TReference, TEvent>`.
- Addressables behavior used internally by Unity Localization asset tables.
- Existing projects that already contain Unity Localization settings and tables.

### 4.4 Explicit non-goals for version 1

- A hosted translation management service.
- Google Sheets as a source of truth.
- Machine translation.
- Automatic rewriting of C# source code.
- Automatic modification of prefab text without an explicit user action and preview.
- A replacement for Unity Localization or Addressables.
- Runtime download of arbitrary CSV from a server.
- Right-to-left shaping implementation. The package will expose extension points and validation hooks, but language-specific shaping requires a separate integration.

## 5. Architecture

### 5.1 Assembly boundaries

| Assembly | Responsibility | Allowed dependencies |
|---|---|---|
| `Dreamy.Localization.Runtime` | Public runtime contracts, service, keys, locale selection, string and asset resolution | Unity Localization, Unity Addressables transitively |
| `Dreamy.Localization.Runtime.UI` | TMP and UGUI binding components | Runtime, TMP, UGUI |
| `Dreamy.Localization.Editor` | Setup, CSV workflow, scanner, validators, editor window, CLI | Runtime, UnityEditor, Unity Localization Editor |
| `Dreamy.Localization.Tests.EditMode` | Pure parser, schema, validation, and editor integration tests | Runtime, Editor, NUnit |
| `Dreamy.Localization.Tests.PlayMode` | Initialization, locale switching, binding, and localized asset tests | Runtime, Runtime.UI, Unity Test Framework |

Keep editor types out of runtime assemblies. Keep TMP and UGUI adapters in a separate runtime assembly so non-UI consumers can reference the core runtime without UI dependencies.

### 5.2 Proposed package layout

```text
com.dreamy.localization/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── Third Party Notices.md
├── Documentation~/
│   ├── index.md
│   ├── csv-schema.md
│   ├── runtime-api.md
│   ├── editor-workflow.md
│   ├── ci-validation.md
│   └── migration.md
├── Runtime/
│   ├── Dreamy.Localization.Runtime.asmdef
│   ├── Configuration/
│   ├── Contracts/
│   ├── Keys/
│   ├── Locale/
│   ├── Services/
│   ├── Strings/
│   └── Assets/
├── Runtime.UI/
│   ├── Dreamy.Localization.Runtime.UI.asmdef
│   ├── LocalizedTmpText.cs
│   ├── LocalizedUguiText.cs
│   ├── LocalizedStringEvent.cs
│   ├── LocalizedImage.cs
│   ├── LocalizedRawImage.cs
│   ├── LocalizedAudioSource.cs
│   └── LocalizedAssetEvent.cs
├── Editor/
│   ├── Dreamy.Localization.Editor.asmdef
│   ├── Configuration/
│   ├── Setup/
│   ├── Csv/
│   ├── Import/
│   ├── Export/
│   ├── Scanning/
│   ├── Validation/
│   ├── Reporting/
│   ├── Migration/
│   └── Window/
├── Tests/
│   ├── EditMode/
│   └── PlayMode/
└── Samples~/
    ├── Basic Text Localization/
    ├── Smart Strings and Plurals/
    ├── Localized Assets/
    └── Locale Selection UI/
```

### 5.3 Runtime design

#### Core contracts

```csharp
public interface ILocalizationService
{
    bool IsInitialized { get; }
    LocaleIdentifier CurrentLocale { get; }
    IReadOnlyList<LocaleIdentifier> AvailableLocales { get; }

    event Action<LocaleIdentifier> LocaleChanged;

    AsyncOperationHandle InitializeAsync();
    AsyncOperationHandle SetLocaleAsync(string localeCode);
    AsyncOperationHandle<string> GetStringAsync(LocalizationKey key, params object[] arguments);
    AsyncOperationHandle<T> GetAssetAsync<T>(LocalizationKey key) where T : UnityEngine.Object;
}
```

The exact public async abstraction must be decided in Phase 1 after testing Unity 6 APIs. Prefer Unity `AsyncOperationHandle` for zero-wrapper interoperability unless DreamyBase already has a standard async abstraction. Do not expose editor-only table types through runtime contracts.

#### Value objects

- `LocalizationKey`: serialized table plus entry key, with equality and validation.
- `LocaleIdentifier`: stable locale code abstraction or direct use of Unity's identifier if it remains serialization-safe.
- `LocalizationResult<T>`: optional structured result for non-component APIs, containing value, requested locale, resolved locale, and failure code.
- `LocalizationError`: stable error codes for missing table, entry, locale, invalid Smart String, load failure, and cancellation.

#### Service lifecycle

1. Initialize Unity Localization once.
2. Read configured locales.
3. Resolve the initial locale in this order:
   - Explicit session override.
   - Persisted user selection.
   - Unity startup selector result.
   - Configured default locale.
   - First available locale.
4. Apply locale and await completion.
5. Raise one initialization event.
6. On locale selection, serialize concurrent requests so the last valid request wins.
7. Persist only after a successful switch.
8. Raise `LocaleChanged` after selected locale and required preloads are ready.

#### Dependency injection

Provide:

- A plain C# `LocalizationService`.
- A default `DreamyLocalization` static facade for simple projects.
- An optional `LocalizationBootstrap` MonoBehaviour.
- Interfaces for preference storage, logging, and initialization policy.

The static facade must delegate to the service rather than contain separate logic.

### 5.4 Configuration design

Create a project asset such as `Assets/Settings/DreamyLocalizationProfile.asset`.

Suggested fields:

- Schema version.
- Default locale code.
- Supported locale definitions.
- Fallback locale code.
- Locale selection policy.
- Persisted preference key.
- String CSV source paths.
- Asset CSV source paths.
- Generated localization asset root.
- Default String Table Collection.
- Default Asset Table Collection.
- Import mode.
- Missing translation policy.
- Orphan handling policy.
- Preload table configuration.
- Scan include/exclude paths.
- Validation severity overrides.
- Pseudo-locale settings.
- Optional custom key naming strategy.

Do not store project-generated configuration assets inside the package folder.

## 6. CSV Contract

### 6.1 String CSV

Use a versioned, table-aware schema.

```csv
schema,table,key,id,source,en,vi,status,smart,shared_comment,en_comment,vi_comment,tags,context
1,UI,home.title,0,Home,Home,Trang chu,approved,false,Main menu title,,,ui;home,Home screen header
1,UI,coins.count,0,Coin count,"{count:plural:1 coin|{} coins}","{count:plural:1 xu|{} xu}",approved,true,,,,ui;economy,Coin counter
```

#### Required columns

- `schema`
- `table`
- `key`
- At least one configured locale column

#### Standard optional columns

- `id`
- `source`
- `status`
- `smart`
- `shared_comment`
- `<locale>_comment`
- `tags`
- `context`

#### Status values

- `draft`
- `translated`
- `review`
- `approved`
- `deprecated`
- `ignore`

Status enforcement must be configurable. Production validation should normally require `approved` for required locales.

### 6.2 Asset CSV

```csv
schema,table,key,id,asset_type,en,vi,status,preload,shared_comment,tags
1,LocalizedAssets,logo,0,UnityEngine.Sprite,Assets/Art/en/logo.png,Assets/Art/vi/logo.png,approved,true,Main logo,branding
1,LocalizedAssets,voice.welcome,0,UnityEngine.AudioClip,Assets/Audio/en/welcome.wav,Assets/Audio/vi/welcome.wav,approved,false,Welcome voice,voice
```

Asset values may accept project-relative asset paths or GUID syntax. The importer resolves them to GUID-backed Unity asset references and validates assignability to `asset_type`.

### 6.3 Import policies

#### Merge mode

- Create missing tables, locale tables, and entries.
- Update entries present in CSV.
- Preserve Unity entries absent from CSV.
- Report orphaned Unity entries.

#### Authoritative replace mode

- Create and update entries present in CSV.
- Mark or remove entries absent from CSV according to profile policy.
- Require explicit preview and confirmation in the editor.
- Require an explicit CLI flag in batchmode.

#### Dry-run

Every import first creates an immutable `LocalizationImportPlan`:

- Locales to create.
- Collections to create.
- Entries to add.
- Entries to update.
- Entries unchanged.
- Entries to deprecate or remove.
- Metadata changes.
- Asset reference changes.
- Blocking errors and warnings.

Only a valid plan can be applied.

### 6.4 Deterministic export

- UTF-8 with a documented BOM policy.
- RFC 4180-compatible quoting.
- Stable column order.
- Stable row sort by table then key.
- Stable newline policy.
- Preserve Unity key IDs when available.
- Preserve comments and package-owned metadata.
- No timestamp or machine-specific path in file content.

## 7. Validation Model

### 7.1 Severity

- `Info`: useful observation, never blocks import or CI.
- `Warning`: suspicious condition; CI behavior configurable.
- `Error`: row or table cannot be processed safely.
- `Critical`: project setup or data integrity failure; always blocks apply and release validation.

### 7.2 Core rules

#### CSV structure

- Missing or unsupported schema version.
- Duplicate headers.
- Missing table or key.
- Invalid locale code.
- Duplicate table/key identity.
- Duplicate Unity key ID.
- Invalid status.
- Malformed quoted fields.
- Invalid encoding or unreadable file.
- Unexpected locale column.

#### Translation content

- Missing required locale value.
- Source changed while translation is still approved.
- Placeholder mismatch between source and translation.
- Smart String syntax error.
- Plural/select branch mismatch.
- Rich text tag mismatch.
- Leading/trailing whitespace mismatch.
- Unsupported control characters.
- Suspicious untranslated value.

#### Unity project

- Missing Localization Settings.
- Missing configured locale.
- Missing String or Asset Table Collection.
- Missing locale-specific table.
- Missing entry.
- Orphaned table entry.
- Duplicate generated asset location.
- Invalid preload configuration.
- Addressables/localized asset load failure in integration tests.

#### Asset rows

- Missing asset.
- Asset outside allowed roots when restricted.
- Type mismatch.
- Missing localized variant.
- Editor-only asset referenced by runtime table.
- Scene object used where a project asset is required.

#### Scan findings

- Non-empty static TMP or UGUI text without localization binding.
- Serialized user-facing string fields marked by configured attributes.
- Hardcoded string passed to configured UI APIs.
- Missing table/key referenced by a component.
- Binding component with no target.
- Invalid Smart String argument binding.

### 7.3 Suppression

Support explicit suppression rather than heuristic silence:

- Component-level ignore marker.
- Asset path exclusions.
- Source-code suppression comment or attribute.
- Rule-specific profile suppression.
- Machine-readable reason stored with the suppression.

## 8. Editor UX

Create `Tools/Dreamy/Localization`.

### Tabs

#### Overview

- Initialization and settings state.
- Locale list.
- CSV source status.
- Table status.
- Last import summary.
- Validation summary.

#### Setup

- Create/repair project profile.
- Create/repair Unity Localization Settings.
- Create missing locales and collections.
- Configure default/fallback locale.
- Create pseudo-locale.

#### Import/Export

- Select configured source.
- Parse and validate.
- Show diff grouped by table and locale.
- Filter add/update/remove/error rows.
- Apply valid plan.
- Export selected or all tables.

#### Scan

- Choose configured scope.
- Scan scenes, prefabs, ScriptableObjects, and code.
- Filter by severity, type, asset, or rule.
- Ping/open the affected asset.
- Export scan report.

#### Validate

- Run profile, CSV, table, reference, Smart String, and asset rules.
- Group issues by rule and asset.
- Copy stable issue ID.
- Export JSON report.

### Safety requirements

- No destructive action from the first button click.
- Display exact add/update/remove counts.
- Disable apply when blocking issues exist.
- Use `AssetDatabase.StartAssetEditing` only around a prepared plan and always stop it in `finally`.
- Use Undo where Unity asset APIs support it.
- Save and refresh once per applied plan, not once per row.
- Keep an import report containing input hash, package version, result, and counts.

## 9. CI Contract

Provide public batchmode entry points:

```text
Dreamy.Localization.Editor.Cli.LocalizationCli.Validate
Dreamy.Localization.Editor.Cli.LocalizationCli.Import
Dreamy.Localization.Editor.Cli.LocalizationCli.Export
```

### Arguments

- `-dreamyLocalizationProfile <asset-path>`
- `-dreamyLocalizationReport <json-path>`
- `-dreamyLocalizationMode merge|replace`
- `-dreamyLocalizationFailOn warning|error|critical`
- `-dreamyLocalizationSource <csv-path>` as an optional override

### Exit codes

| Code | Meaning |
|---|---|
| `0` | Success |
| `1` | Validation failed |
| `2` | Invalid CLI arguments or missing profile |
| `3` | Import/export operation failed |
| `4` | Unexpected internal error |

CI output must include stable rule codes and a JSON report. Avoid relying only on Unity console text.

## 10. Implementation Phases

## Phase 0 - Compatibility and Package Baseline

### Work

- Confirm the exact Unity 6 editor version used by DreamyBase.
- Verify the latest compatible `com.unity.localization` `1.5.x` patch.
- Verify TMP and UGUI dependency declarations for that Unity version.
- Create package metadata, assembly definitions, documentation skeleton, test assemblies, and samples folders.
- Create a minimal Unity fixture project or CI project that consumes the local package.
- Record API compatibility decisions in an Architecture Decision Record.

### Deliverables

- Valid UPM package recognized by Unity 6.
- Runtime, UI, Editor, EditMode, and PlayMode assemblies compile.
- Package Validation Suite baseline report.
- Dependency versions pinned intentionally.

### Acceptance criteria

- Package installs into a clean Unity 6 project with no compiler errors.
- Runtime assembly has no `UnityEditor` dependency.
- Empty EditMode and PlayMode test suites execute in batchmode.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Unity 6 package API differs from reference repository | 3 | 4 | 12 | Complete API spike before public contracts are finalized |
| TMP/UGUI dependency declarations cause package conflicts | 2 | 4 | 8 | Test clean install and existing DreamyBase install |
| Package layout is invalid for UPM | 2 | 3 | 6 | Run Package Validation Suite in this phase |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 0 | S, about 1 day | Blocker for all later phases |

## Phase 1 - Runtime Core and Locale Lifecycle

### Work

- Implement configuration contracts and runtime-safe profile representation.
- Implement `ILocalizationService`.
- Implement initialization state machine.
- Implement locale discovery, selection, persistence, fallback, and events.
- Implement string and generic asset resolution.
- Add structured errors and logging hooks.
- Add static facade and optional bootstrap component.
- Define behavior for concurrent locale changes and repeated initialization.

### Deliverables

- Stable public runtime API.
- Locale selection service.
- String and asset lookup.
- Preference store abstraction with PlayerPrefs default.

### Acceptance criteria

- Initialization is idempotent.
- Selected locale survives a simulated application restart.
- Invalid locale selection returns a defined failure and does not corrupt current state.
- Two rapid locale requests produce deterministic final state.
- Missing keys produce logged, testable failures according to policy.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Async API leaks Unity implementation details excessively | 3 | 4 | 12 | Prototype both direct handle and wrapper approaches before freezing API |
| Locale race conditions leave mixed-language UI | 3 | 5 | 15 | Serialize switches and add PlayMode concurrency tests before continuing |
| Static facade creates duplicate service state | 2 | 5 | 10 | Make facade a thin delegate to one configured service |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 1 | M, about 3 days | Critical path |

## Phase 2 - UI and Localized Asset Bindings

### Work

- Implement TMP and UGUI text components.
- Implement generic string event binding.
- Implement Image, RawImage, AudioSource, and generic asset event bindings.
- Support serialized table/key references and runtime reassignment.
- Support Smart String arguments and refresh.
- Handle enable, disable, destroy, and locale-change lifecycles.
- Ensure built-in Unity localization components remain usable alongside Dreamy components.

### Deliverables

- Runtime UI assembly.
- Inspector-friendly bindings.
- Basic text, Smart String, and localized asset samples.

### Acceptance criteria

- Active components refresh once after locale switch.
- Disabled components do not receive updates.
- Re-enabled components show current locale content.
- Runtime key reassignment unsubscribes old references.
- Sprite, texture, and audio localization work through Asset Tables.
- No retained event subscriptions after scene unload.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Event subscription leaks | 3 | 5 | 15 | Centralize binding lifecycle and add enable/disable/destroy tests |
| Asset updates complete out of order | 3 | 4 | 12 | Track request version and discard stale callbacks |
| Duplicate functionality diverges from Unity components | 2 | 4 | 8 | Use adapters and document when native components are preferable |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 2 | M, about 3 days | Depends on Phase 1 API |

## Phase 3 - CSV Domain, Parser, Validation, and Migration

### Work

- Implement RFC 4180-compatible parser and writer.
- Implement versioned string and asset documents.
- Implement schema migration interfaces.
- Implement row identity and stable ordering.
- Implement validation rule registry.
- Implement Smart String placeholder and syntax validation.
- Implement asset path/GUID/type validation.
- Implement content hashing for import reports.

### Deliverables

- Pure C# CSV domain layer.
- Version 1 string and asset schemas.
- Migration framework.
- Deterministic writer.
- Rule-based validation results.

### Acceptance criteria

- Correctly parses commas, quotes, CRLF/LF, embedded newlines, Unicode, and empty trailing fields.
- Parse then write then parse preserves semantic data.
- Duplicate headers and identities are rejected.
- Unknown future schema versions fail without modifying assets.
- Smart String placeholder mismatches are reported with table, key, and locale.
- Parser and validator do not require Unity asset APIs where avoidable.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Custom CSV implementation misses edge cases | 3 | 5 | 15 | Build corpus tests and consider a package-compatible parser dependency only if necessary |
| Schema becomes difficult to evolve | 3 | 4 | 12 | Require schema column and explicit migrator chain from version 1 |
| Placeholder validation rejects valid custom formatters | 3 | 3 | 9 | Separate syntax errors from contract warnings and allow rule customization |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 3 | L, about 1 week | Can overlap partially with Phase 2 after Phase 1 contracts stabilize |

## Phase 4 - Setup, Import Plan, Apply, and Export

### Work

- Implement project profile creation and repair.
- Implement locale/settings/table bootstrap.
- Convert CSV documents into immutable import plans.
- Implement string table apply logic.
- Implement asset table apply logic.
- Implement merge and authoritative replace policies.
- Import comments, Smart flags, IDs, tags, and package-owned metadata.
- Implement deterministic export from Unity tables.
- Generate import reports.

### Deliverables

- End-to-end string and asset CSV workflow.
- Dry-run diff model.
- Idempotent apply operation.
- Deterministic exporter.

### Acceptance criteria

- Applying the same CSV twice produces no changes on the second run.
- A failed plan validation writes no table changes.
- Merge mode preserves entries absent from CSV.
- Replace mode reports removals before apply.
- Existing Unity key IDs survive export/import.
- Asset rows resolve to correct typed references.
- Exported CSV is stable in source control when data has not changed.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Partial asset writes corrupt table state | 3 | 5 | 15 | Block apply on errors, prepare complete plan, minimize save boundaries, test injected failures |
| Replace mode removes intentional manual data | 3 | 5 | 15 | Require preview, explicit policy, backup guidance, and CLI opt-in |
| Asset tables create Addressables side effects | 3 | 4 | 12 | Use Unity Localization editor APIs and verify generated Addressables state in fixture project |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 4 | L, about 1 week | Critical path for production use |

## Phase 5 - Editor Window and Project Scanner

### Work

- Implement the editor window tabs described above.
- Implement setup status and repair actions.
- Implement import diff visualization.
- Implement scene and prefab scanning for TMP, UGUI, and Dreamy/native localization bindings.
- Implement serialized field scanning through configurable markers.
- Implement conservative source-code findings.
- Implement suppressions and report navigation.
- Keep scanner rules independent and individually testable.

### Deliverables

- `Tools/Dreamy/Localization` window.
- Configurable scanner.
- Actionable validation and scan reports.

### Acceptance criteria

- Scanner finds known fixture violations with stable rule IDs.
- Excluded paths and explicit suppressions are respected.
- Results can ping assets and identify component hierarchy paths.
- Source scan findings are warnings unless a high-confidence rule applies.
- No automatic prefab or source edits occur during scan.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Regex source scan produces excessive false positives | 4 | 3 | 12 | Limit to configured APIs/paths, classify confidence, and support suppressions |
| Large projects scan too slowly | 4 | 4 | 16 | Cache asset hashes, support scoped scans, show progress, and make full scan a CI task |
| Editor UI becomes tightly coupled to import logic | 3 | 3 | 9 | UI consumes immutable plans and reports from editor services |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 5 | L, about 1 week | Performance mitigation required before release |

## Phase 6 - CLI and Release Gates

### Work

- Implement validate/import/export entry points.
- Implement argument parsing and exit codes.
- Write JSON report schema.
- Add package CI scripts or documented commands.
- Add validation profiles for developer and release modes.
- Ensure logs include package version, profile, source hash, and rule summary.

### Deliverables

- Batchmode CLI.
- JSON reports.
- CI usage documentation.
- Release validation preset.

### Acceptance criteria

- Clean fixture exits `0`.
- Invalid translation fixture exits `1`.
- Invalid arguments exit `2`.
- Reports are produced even when validation fails.
- CLI import is idempotent.
- CI can fail on configurable minimum severity.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Unity batchmode exits before report flush | 2 | 4 | 8 | Write report before requesting editor exit and test process exit behavior |
| Local and CI behavior differ due to project paths | 3 | 4 | 12 | Normalize paths and use project-relative profile configuration |
| Warning policy blocks teams unexpectedly | 2 | 3 | 6 | Provide explicit developer and release presets |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 6 | M, about 3 days | Depends on validation and import services |

## Phase 7 - Test Hardening and Performance

### Work

- Complete EditMode unit and integration tests.
- Complete PlayMode locale and binding tests.
- Build test fixtures for strings, Smart Strings, plurals, assets, missing values, and migrations.
- Measure import and scan behavior on representative DreamyBase project sizes.
- Add allocation and repeated-refresh checks for runtime bindings.
- Add domain reload and scene transition tests.

### Test matrix

| Area | EditMode | PlayMode |
|---|---|---|
| CSV parse/write | Required | Not needed |
| Schema migration | Required | Not needed |
| Rule validation | Required | Selected integration cases |
| String table import/export | Required | Smoke test |
| Asset table import | Required | Load and switch locale |
| Locale persistence | Unit test abstraction | Required |
| TMP/UGUI updates | Serialization checks | Required |
| Smart String/plural | Syntax and contract | Required |
| Concurrent locale changes | Limited | Required |
| Event cleanup | Limited | Required |
| Scanner | Fixture-based | Not needed |
| CLI | Editor process integration | Not needed |

### Acceptance criteria

- Critical domain services have meaningful branch and edge-case coverage.
- All public runtime behavior is covered by PlayMode tests.
- Import handles a representative large CSV without per-row asset refresh.
- Full scan has measured performance and documented scope recommendations.
- No known event leaks or stale async updates remain.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Unity integration tests are flaky | 3 | 4 | 12 | Isolate generated assets, wait on handles explicitly, clean fixtures deterministically |
| Performance regressions appear only in real games | 3 | 4 | 12 | Benchmark with at least one representative DreamyBase project before release |
| Test asset cleanup damages unrelated assets | 2 | 5 | 10 | Restrict all generated fixtures to a dedicated verified root |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 7 | L, about 1 week | Runs continuously; final hardening after Phase 6 |

## Phase 8 - Documentation, Samples, Migration, and Release

### Work

- Write installation and quick-start documentation.
- Document both CSV schemas and policies.
- Document runtime API and component workflows.
- Add four focused samples.
- Document CI integration.
- Add migration guide from direct Unity Localization usage and from the reference package style.
- Run Package Validation Suite.
- Define semantic versioning and changelog process.
- Publish `1.0.0` release candidate, integrate into one DreamyBase pilot project, then release.

### Deliverables

- Production documentation.
- Samples.
- Migration guide.
- Release candidate.
- Pilot integration report.

### Acceptance criteria

- A developer can install, bootstrap locales, import CSV, bind UI, switch locale, and run CI using documentation only.
- Samples import without modifying package contents.
- Existing Unity Localization tables can be adopted without re-keying.
- Package validation has no unresolved errors.
- Pilot project passes release validation.

### Risk assessment

| Risk | Likelihood | Impact | Score | Mitigation |
|---|---:|---:|---:|---|
| Package works in fixture but not existing DreamyBase projects | 3 | 5 | 15 | Require pilot migration before `1.0.0` |
| Public API changes after adoption | 3 | 4 | 12 | Release candidate period and API review before stable release |
| Documentation omits operational failure cases | 3 | 3 | 9 | Document recovery for invalid CSV, missing assets, and failed locale loads |

### Effort

| Item | Estimate | Notes |
|---|---|---|
| Phase 8 | M, about 3 days plus pilot feedback | Final release gate |

## 11. Timeline Summary

| Phase | Estimate | Dependency |
|---|---|---|
| Phase 0 | S, about 1 day | None |
| Phase 1 | M, about 3 days | Phase 0 |
| Phase 2 | M, about 3 days | Phase 1 |
| Phase 3 | L, about 1 week | Phase 0; partial parallel work possible |
| Phase 4 | L, about 1 week | Phases 1 and 3 |
| Phase 5 | L, about 1 week | Phases 3 and 4 |
| Phase 6 | M, about 3 days | Phases 3 and 4 |
| Phase 7 | L, about 1 week | Continuous, final pass after Phase 6 |
| Phase 8 | M, about 3 days plus pilot | All prior phases |
| Total | Approximately 6-8 engineering weeks | Critical path: 0, 1, 3, 4, 6, 7, 8 |

Parallelization can reduce calendar time, but the runtime API, CSV schema, and import-plan model must be stabilized before editor UI work is allowed to depend on them.

## 12. Definition of Done

The package is complete when:

- It installs cleanly in the agreed Unity 6 baseline.
- CSV string and asset data can be validated, previewed, imported, and exported deterministically.
- Import is idempotent and blocks unsafe writes.
- Locale selection is async-safe, persistent, and testable.
- TMP, UGUI, Smart Strings, plurals, and localized assets work in PlayMode.
- Scanner and validators produce stable actionable findings.
- Batchmode validation returns documented exit codes and JSON reports.
- EditMode and PlayMode suites pass in CI.
- Package Validation Suite passes.
- Documentation and samples cover the primary workflows.
- One real DreamyBase project completes pilot integration without package-local patches.

## 13. Implementation Guardrails

- Treat CSV schema and public runtime APIs as versioned contracts.
- Do not write Unity assets until a complete import plan passes validation.
- Do not use hard-coded project paths or locale lists.
- Do not place generated project assets inside `Packages/com.dreamy.localization`.
- Do not make runtime assemblies reference editor assemblies.
- Do not perform blocking waits on localization or Addressables handles in gameplay code.
- Do not silently fall back without reporting requested and resolved locale.
- Do not delete orphaned entries by default.
- Do not auto-edit source code or prefabs from scanner results.
- Do not release `1.0.0` before a real DreamyBase pilot.

## 14. Recommended Execution Order

1. Execute Phase 0 and record exact Unity/package versions.
2. Freeze the Phase 1 runtime contract through API review.
3. Implement Phase 3 CSV contracts in parallel with Phase 2 bindings.
4. Build Phase 4 on immutable import plans.
5. Add editor UI only after import and validation services work headlessly.
6. Make CLI call the same services as the editor window.
7. Complete performance and pilot gates before stable release.

Implementation handoff:

```text
omg-cook LOCALIZATION_PACKAGE_PLAN.md
```
