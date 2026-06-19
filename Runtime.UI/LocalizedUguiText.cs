using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Localization.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedUguiText : LocalizedStringBinding
    {
        [SerializeField] private Text target;

        private void Awake()
        {
            this.target ??= this.GetComponent<Text>();
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
