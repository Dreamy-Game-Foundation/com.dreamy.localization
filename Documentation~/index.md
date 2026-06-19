# Dreamy Localization

Dreamy Localization is a CSV-first workflow built on Unity Localization.

## Initial workflow

1. Install the package in a Unity 6 project.
2. Create a `DreamyLocalizationProfile`.
3. Configure Unity Localization locales and tables.
4. Add a `LocalizationBootstrap` to the startup scene.
5. Add `LocalizedTmpText`, `LocalizedUguiText`, or an asset binding to UI.
6. Parse and validate localization CSV before importing it into Unity tables.

The editor setup/import window and CLI are scheduled after the runtime and CSV
foundation is verified.

## Runtime lookup

```csharp
var key = new LocalizationKey("UI", "home.title");
var title = await DreamyLocalization.GetStringAsync(key, "Home");
```

## Locale selection

```csharp
var changed = await DreamyLocalization.SetLocaleAsync("vi");
```

Locale codes are matched case-insensitively. The profile can also map device
culture aliases to supported locale codes.
