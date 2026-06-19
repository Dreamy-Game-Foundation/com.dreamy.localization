using UnityEngine;
using UnityEngine.Localization;

namespace Dreamy.Localization.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class LocalizedAudioSource : MonoBehaviour, ILocalizationBinding
    {
        [SerializeField] private AudioSource target;
        [SerializeField] private LocalizedAudioClip localizedClip = new();
        [SerializeField] private AudioClip fallback;

        private void Awake()
        {
            this.target ??= this.GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            this.localizedClip.AssetChanged += this.Apply;
        }

        private void OnDisable()
        {
            this.localizedClip.AssetChanged -= this.Apply;
        }

        public void Refresh()
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }

            this.localizedClip.AssetChanged -= this.Apply;
            this.localizedClip.AssetChanged += this.Apply;
        }

        private void Apply(AudioClip value)
        {
            if (this.target != null)
            {
                this.target.clip = value != null ? value : this.fallback;
            }
        }
    }
}
