# Runtime API

## Configuration

`DreamyLocalizationProfile` stores:

- Default and fallback locale codes.
- PlayerPrefs key used for locale persistence.
- Locale culture aliases.
- Planned table loading policies.

## Service

`ILocalizationService` is the testable runtime boundary. The default
`LocalizationService` delegates storage and loading to Unity Localization.

`DreamyLocalization` is a convenience facade. It must be configured before use,
either manually or by `LocalizationBootstrap`.

## Bindings

Current bindings:

- `LocalizedTmpText`
- `LocalizedUguiText`
- `LocalizedStringEvent`
- `LocalizedImage`
- `LocalizedSpriteRenderer`
- `LocalizedAudioSource`

String bindings support runtime table/entry reassignment and Smart String
arguments. All bindings subscribe on enable and unsubscribe on disable.

## Reactive variables

`LocalizationVariable<T>` and `LocalizationVariableRegistry` provide the
runtime notification and lookup contract for named variables. The Unity Smart
Format persistent-variable adapter is intentionally deferred until the table
import and Smart String integration phase.
