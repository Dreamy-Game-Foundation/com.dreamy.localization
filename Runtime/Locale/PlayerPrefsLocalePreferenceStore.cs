using UnityEngine;

namespace Dreamy.Localization
{
    public sealed class PlayerPrefsLocalePreferenceStore : ILocalePreferenceStore
    {
        private readonly string key;

        public PlayerPrefsLocalePreferenceStore(string key)
        {
            this.key = string.IsNullOrWhiteSpace(key)
                ? "dreamy.localization.locale"
                : key;
        }

        public bool TryGetLocale(out string localeCode)
        {
            localeCode = PlayerPrefs.GetString(this.key, string.Empty);
            return !string.IsNullOrWhiteSpace(localeCode);
        }

        public void SetLocale(string localeCode)
        {
            PlayerPrefs.SetString(this.key, localeCode);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(this.key);
        }
    }
}
