using UnityEngine;
using UnityEngine.Localization;

namespace Dreamy.Localization.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class LocalizedSpriteRenderer : MonoBehaviour, ILocalizationBinding
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private LocalizedSprite localizedSprite = new();
        [SerializeField] private Sprite fallback;

        private void Awake()
        {
            this.target ??= this.GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            this.localizedSprite.AssetChanged += this.Apply;
        }

        private void OnDisable()
        {
            this.localizedSprite.AssetChanged -= this.Apply;
        }

        public void Refresh()
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }

            this.localizedSprite.AssetChanged -= this.Apply;
            this.localizedSprite.AssetChanged += this.Apply;
        }

        private void Apply(Sprite value)
        {
            if (this.target != null)
            {
                this.target.sprite = value != null ? value : this.fallback;
            }
        }
    }
}
