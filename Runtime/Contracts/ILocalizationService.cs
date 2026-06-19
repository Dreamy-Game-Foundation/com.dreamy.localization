using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Localization
{
    public interface ILocalizationService
    {
        bool IsInitialized { get; }

        string CurrentLocaleCode { get; }

        IReadOnlyList<string> AvailableLocaleCodes { get; }

        event Action<string> LocaleChanged;

        Task InitializeAsync();

        Task<bool> SetLocaleAsync(string localeCode);

        Task<string> GetStringAsync(LocalizationKey key, params object[] arguments);

        Task<T> GetAssetAsync<T>(LocalizationKey key) where T : UnityEngine.Object;
    }
}
