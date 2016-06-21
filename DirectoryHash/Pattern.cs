using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirectoryHash
{
    /// <summary>
    /// Represents a file glob that can be matched against.
    /// </summary>
    internal sealed class Pattern
    {
        private readonly Regex _pattern;

        public Pattern(string pattern)
        {
            // Ensure everything "special" is escaped
            pattern = Regex.Escape(pattern);

            // But now undo the conversion for * to what we want
            pattern = pattern.Replace(@"\*", ".*");

            _pattern = new Regex("^" + pattern + "$");
        }

        public bool NameMatchesPattern(string name)
        {
            return _pattern.IsMatch(name);
        }
    }
}
