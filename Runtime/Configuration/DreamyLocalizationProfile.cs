using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Localization
{
    [CreateAssetMenu(
        fileName = "DreamyLocalizationProfile",
        menuName = "Dreamy/Localization/Profile")]
    public sealed class DreamyLocalizationProfile : ScriptableObject
    {
        [SerializeField] private int schemaVersion = 1;
        [SerializeField] private string defaultLocaleCode = "en";
        [SerializeField] private string fallbackLocaleCode = "en";
        [SerializeField] private string preferenceKey = "dreamy.localization.locale";
        [SerializeField] private List<LocaleDefinition> locales = new();
        [SerializeField] private List<TableLoadDefinition> tables = new();

        public int SchemaVersion => this.schemaVersion;

        public string DefaultLocaleCode => this.defaultLocaleCode;

        public string FallbackLocaleCode => this.fallbackLocaleCode;

        public string PreferenceKey => this.preferenceKey;

        public IReadOnlyList<LocaleDefinition> Locales => this.locales;

        public IReadOnlyList<TableLoadDefinition> Tables => this.tables;
    }

    [Serializable]
    public sealed class LocaleDefinition
    {
        [SerializeField] private string localeCode = "en";
        [SerializeField] private List<string> cultureAliases = new();

        public string LocaleCode => this.localeCode;

        public IReadOnlyList<string> CultureAliases => this.cultureAliases;
    }

    [Serializable]
    public sealed class TableLoadDefinition
    {
        [SerializeField] private string table;
        [SerializeField] private LocalizationTableLoadPolicy policy;

        public string Table => this.table;

        public LocalizationTableLoadPolicy Policy => this.policy;
    }

    public enum LocalizationTableLoadPolicy
    {
        OnDemand = 0,
        Preload = 1,
        Retained = 2,
        ReleaseWhenUnused = 3,
    }
}
