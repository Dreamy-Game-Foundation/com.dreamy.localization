using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Dreamy.Localization
{
    public sealed class LocalizationService : ILocalizationService
    {
        private readonly DreamyLocalizationProfile profile;
        private readonly ILocalePreferenceStore preferenceStore;
        private readonly SemaphoreSlim localeChangeLock = new(1, 1);
        private readonly List<string> availableLocaleCodes = new();
        private Task initializationTask;

        public LocalizationService(
            DreamyLocalizationProfile profile,
            ILocalePreferenceStore preferenceStore = null)
        {
            this.profile = profile;
            this.preferenceStore = preferenceStore ??
                new PlayerPrefsLocalePreferenceStore(profile != null
                    ? profile.PreferenceKey
                    : null);
        }

        public bool IsInitialized { get; private set; }

        public string CurrentLocaleCode =>
            LocalizationSettings.SelectedLocale != null
                ? LocalizationSettings.SelectedLocale.Identifier.Code
                : string.Empty;

        public IReadOnlyList<string> AvailableLocaleCodes => this.availableLocaleCodes;

        public event Action<string> LocaleChanged;

        public Task InitializeAsync()
        {
            return this.initializationTask ??= this.InitializeInternalAsync();
        }

        public async Task<bool> SetLocaleAsync(string localeCode)
        {
            await this.InitializeAsync();
            await this.localeChangeLock.WaitAsync();

            try
            {
                var matchedCode = LocaleCodeMatcher.Match(
                    localeCode,
                    this.availableLocaleCodes,
                    this.profile != null ? this.profile.Locales : null);
                if (matchedCode == null)
                {
                    return false;
                }

                var locale = this.FindLocale(matchedCode);
                if (locale == null)
                {
                    return false;
                }

                if (LocalizationSettings.SelectedLocale == locale)
                {
                    return true;
                }

                LocalizationSettings.SelectedLocale = locale;
                this.preferenceStore.SetLocale(locale.Identifier.Code);
                return true;
            }
            finally
            {
                this.localeChangeLock.Release();
            }
        }

        public async Task<string> GetStringAsync(
            LocalizationKey key,
            params object[] arguments)
        {
            ValidateKey(key);
            await this.InitializeAsync();

            var localizedString = new LocalizedString
            {
                TableReference = key.Table,
                TableEntryReference = key.Entry,
                Arguments = arguments,
            };

            return await AsyncOperationTask.Await(
                localizedString.GetLocalizedStringAsync());
        }

        public async Task<T> GetAssetAsync<T>(LocalizationKey key)
            where T : UnityEngine.Object
        {
            ValidateKey(key);
            await this.InitializeAsync();

            var handle = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<T>(
                key.Table,
                key.Entry);
            return await AsyncOperationTask.Await(handle);
        }

        private async Task InitializeInternalAsync()
        {
            await AsyncOperationTask.Await(LocalizationSettings.InitializationOperation);
            this.availableLocaleCodes.Clear();

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                this.availableLocaleCodes.Add(locale.Identifier.Code);
            }

            var initialCode = this.ResolveInitialLocaleCode();
            var initialLocale = this.FindLocale(initialCode);
            if (initialLocale != null)
            {
                LocalizationSettings.SelectedLocale = initialLocale;
            }

            LocalizationSettings.SelectedLocaleChanged +=
                this.HandleSelectedLocaleChanged;
            this.IsInitialized = true;
        }

        private void HandleSelectedLocaleChanged(Locale locale)
        {
            if (locale == null)
            {
                return;
            }

            var localeCode = locale.Identifier.Code;
            this.preferenceStore.SetLocale(localeCode);
            this.LocaleChanged?.Invoke(localeCode);
        }

        private string ResolveInitialLocaleCode()
        {
            if (this.preferenceStore.TryGetLocale(out var persistedCode))
            {
                var persistedMatch = LocaleCodeMatcher.Match(
                    persistedCode,
                    this.availableLocaleCodes,
                    this.profile != null ? this.profile.Locales : null);
                if (persistedMatch != null)
                {
                    return persistedMatch;
                }
            }

            if (LocalizationSettings.SelectedLocale != null)
            {
                return LocalizationSettings.SelectedLocale.Identifier.Code;
            }

            if (this.profile != null)
            {
                var configuredMatch = LocaleCodeMatcher.Match(
                    this.profile.DefaultLocaleCode,
                    this.availableLocaleCodes,
                    this.profile.Locales);
                if (configuredMatch != null)
                {
                    return configuredMatch;
                }
            }

            return this.availableLocaleCodes.Count > 0
                ? this.availableLocaleCodes[0]
                : null;
        }

        private Locale FindLocale(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                return null;
            }

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                if (string.Equals(
                        locale.Identifier.Code,
                        localeCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return locale;
                }
            }

            return null;
        }

        private static void ValidateKey(LocalizationKey key)
        {
            if (!key.IsValid)
            {
                throw new ArgumentException(
                    "Localization key must contain both table and entry names.",
                    nameof(key));
            }
        }
    }
}
