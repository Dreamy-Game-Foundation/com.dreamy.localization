using System;
using UnityEngine;

namespace Dreamy.Localization
{
    [Serializable]
    public struct LocalizationKey : IEquatable<LocalizationKey>
    {
        [SerializeField] private string table;
        [SerializeField] private string entry;

        public LocalizationKey(string table, string entry)
        {
            this.table = table ?? string.Empty;
            this.entry = entry ?? string.Empty;
        }

        public string Table => this.table;

        public string Entry => this.entry;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(this.table) &&
            !string.IsNullOrWhiteSpace(this.entry);

        public bool Equals(LocalizationKey other)
        {
            return string.Equals(this.table, other.table, StringComparison.Ordinal) &&
                   string.Equals(this.entry, other.entry, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalizationKey other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.table != null ? this.table.GetHashCode() : 0) * 397) ^
                       (this.entry != null ? this.entry.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{this.table}/{this.entry}";
        }

        public static bool operator ==(LocalizationKey left, LocalizationKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocalizationKey left, LocalizationKey right)
        {
            return !left.Equals(right);
        }
    }
}
