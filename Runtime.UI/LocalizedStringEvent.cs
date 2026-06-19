using UnityEngine;
using UnityEngine.Events;

namespace Dreamy.Localization.UI
{
    [DisallowMultipleComponent]
    public sealed class LocalizedStringEvent : LocalizedStringBinding
    {
        [SerializeField] private UnityEvent<string> valueChanged = new();

        protected override void Apply(string value)
        {
            this.valueChanged.Invoke(value);
        }
    }
}
