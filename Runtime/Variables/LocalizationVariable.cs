using System;
using System.Collections.Generic;

namespace Dreamy.Localization
{
    public sealed class LocalizationVariable<T> : ILocalizationVariable
    {
        private T value;

        public LocalizationVariable(T initialValue = default)
        {
            this.value = initialValue;
        }

        public T TypedValue
        {
            get => this.value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    return;
                }

                this.value = value;
                this.ValueChanged?.Invoke();
            }
        }

        public object Value => this.value;

        public event Action ValueChanged;
    }
}
