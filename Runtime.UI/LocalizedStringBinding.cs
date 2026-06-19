using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Dreamy.Localization.UI
{
    public abstract class LocalizedStringBinding : MonoBehaviour, ILocalizationBinding
    {
        [SerializeField] private LocalizedString localizedString = new();
        [SerializeField] private string fallback;

        protected LocalizedString Reference => this.localizedString;

        protected virtual void OnEnable()
        {
            this.localizedString.StringChanged += this.HandleStringChanged;
        }

        protected virtual void OnDisable()
        {
            this.localizedString.StringChanged -= this.HandleStringChanged;
        }

        public void SetReference(string table, string entry)
        {
            this.localizedString.TableReference = table;
            this.localizedString.TableEntryReference = entry;
            this.Refresh();
        }

        public void SetArguments(params object[] arguments)
        {
            this.localizedString.Arguments = arguments;
            this.Refresh();
        }

        public void Refresh()
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }

            this.localizedString.RefreshString();
        }

        protected abstract void Apply(string value);

        private void HandleStringChanged(string value)
        {
            this.Apply(string.IsNullOrEmpty(value) ? this.fallback : value);
        }
    }
}
