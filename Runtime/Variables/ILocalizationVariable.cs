using System;

namespace Dreamy.Localization
{
    public interface ILocalizationVariable
    {
        object Value { get; }

        event Action ValueChanged;
    }
}
