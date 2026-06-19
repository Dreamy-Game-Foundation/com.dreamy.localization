using UnityEngine;

namespace Dreamy.Localization
{
    [DefaultExecutionOrder(-10000)]
    public sealed class LocalizationBootstrap : MonoBehaviour
    {
        [SerializeField] private DreamyLocalizationProfile profile;
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool persistAcrossScenes = true;

        private async void Awake()
        {
            if (this.persistAcrossScenes)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            if (!DreamyLocalization.IsConfigured)
            {
                DreamyLocalization.Configure(new LocalizationService(this.profile));
            }

            if (this.initializeOnAwake)
            {
                await DreamyLocalization.InitializeAsync();
            }
        }
    }
}
