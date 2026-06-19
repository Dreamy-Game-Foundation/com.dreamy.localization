using System;
using System.Collections.Generic;

namespace Dreamy.Localization
{
    public static class LocaleCodeMatcher
    {
        public static string Match(
            string requestedCode,
            IReadOnlyList<string> availableCodes,
            IReadOnlyList<LocaleDefinition> definitions)
        {
            if (string.IsNullOrWhiteSpace(requestedCode) || availableCodes == null)
            {
                return null;
            }

            var exact = FindAvailable(requestedCode, availableCodes);
            if (exact != null)
            {
                return exact;
            }

            if (definitions != null)
            {
                foreach (var definition in definitions)
                {
                    if (definition == null ||
                        !ContainsAlias(definition.CultureAliases, requestedCode))
                    {
                        continue;
                    }

                    var aliasMatch = FindAvailable(definition.LocaleCode, availableCodes);
                    if (aliasMatch != null)
                    {
                        return aliasMatch;
                    }
                }
            }

            var separator = requestedCode.IndexOf('-');
            if (separator < 0)
            {
                separator = requestedCode.IndexOf('_');
            }

            if (separator > 0)
            {
                return FindAvailable(requestedCode.Substring(0, separator), availableCodes);
            }

            return null;
        }

        private static string FindAvailable(
            string requestedCode,
            IReadOnlyList<string> availableCodes)
        {
            for (var index = 0; index < availableCodes.Count; index++)
            {
                if (string.Equals(
                        requestedCode,
                        availableCodes[index],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return availableCodes[index];
                }
            }

            return null;
        }

        private static bool ContainsAlias(
            IReadOnlyList<string> aliases,
            string requestedCode)
        {
            if (aliases == null)
            {
                return false;
            }

            for (var index = 0; index < aliases.Count; index++)
            {
                if (string.Equals(
                        aliases[index],
                        requestedCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
