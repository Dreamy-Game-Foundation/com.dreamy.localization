namespace Dreamy.Localization
{
    public interface ILocalePreferenceStore
    {
        bool TryGetLocale(out string localeCode);

        void SetLocale(string localeCode);

        void Clear();
    }
}
