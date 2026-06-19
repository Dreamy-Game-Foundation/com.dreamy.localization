using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Localization
{
    public static class DreamyLocalization
    {
        private static ILocalizationService service;

        public static bool IsConfigured => service != null;

        public static bool IsInitialized => service != null && service.IsInitialized;

        public static string CurrentLocaleCode =>
            service != null ? service.CurrentLocaleCode : string.Empty;

        public static IReadOnlyList<string> AvailableLocaleCodes =>
            service != null ? service.AvailableLocaleCodes : Array.Empty<string>();

        public static event Action<string> LocaleChanged
        {
            add => RequireService().LocaleChanged += value;
            remove
            {
                if (service != null)
                {
                    service.LocaleChanged -= value;
                }
            }
        }

        public static void Configure(ILocalizationService localizationService)
        {
            service = localizationService ??
                throw new ArgumentNullException(nameof(localizationService));
        }

        public static Task InitializeAsync()
        {
            return RequireService().InitializeAsync();
        }

        public static Task<bool> SetLocaleAsync(string localeCode)
        {
            return RequireService().SetLocaleAsync(localeCode);
        }

        public static async Task<string> GetStringAsync(
            LocalizationKey key,
            string fallback = null,
            params object[] arguments)
        {
            try
            {
                return await RequireService().GetStringAsync(key, arguments);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to localize string '{key}': {exception.Message}");
                return fallback;
            }
        }

        public static async Task<T> GetAssetAsync<T>(
            LocalizationKey key,
            T fallback = null)
            where T : UnityEngine.Object
        {
            try
            {
                return await RequireService().GetAssetAsync<T>(key);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to localize asset '{key}': {exception.Message}");
                return fallback;
            }
        }

        private static ILocalizationService RequireService()
        {
            return service ?? throw new InvalidOperationException(
                "DreamyLocalization is not configured. Configure a service or add LocalizationBootstrap.");
        }
    }
}
