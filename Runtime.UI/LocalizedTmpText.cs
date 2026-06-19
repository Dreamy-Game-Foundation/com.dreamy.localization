using TMPro;
using UnityEngine;

namespace Dreamy.Localization.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedTmpText : LocalizedStringBinding
    {
        [SerializeField] private TMP_Text target;

        private void Awake()
        {
            this.target ??= this.GetComponent<TMP_Text>();
        }

        protected override void Apply(string value)
        {
            if (this.target != null)
            {
                this.target.text = value;
            }
        }
    }
}
