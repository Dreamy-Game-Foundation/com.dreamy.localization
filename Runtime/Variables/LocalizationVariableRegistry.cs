using System;
using System.Collections.Generic;

namespace Dreamy.Localization
{
    public sealed class LocalizationVariableRegistry
    {
        private readonly Dictionary<string, ILocalizationVariable> variables =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Action> handlers =
            new(StringComparer.Ordinal);

        public event Action<string> VariableChanged;

        public void Register(string name, ILocalizationVariable variable)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Variable name cannot be empty.",
                    nameof(name));
            }

            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            this.Unregister(name);
            this.variables.Add(name, variable);
            Action handler = () => this.VariableChanged?.Invoke(name);
            this.handlers.Add(name, handler);
            variable.ValueChanged += handler;
        }

        public bool Unregister(string name)
        {
            if (!this.variables.TryGetValue(name, out var variable))
            {
                return false;
            }

            variable.ValueChanged -= this.handlers[name];
            this.handlers.Remove(name);
            return this.variables.Remove(name);
        }

        public bool TryGet(string name, out ILocalizationVariable variable)
        {
            return this.variables.TryGetValue(name, out variable);
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            if (this.variables.TryGetValue(name, out var variable) &&
                variable.Value is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}
