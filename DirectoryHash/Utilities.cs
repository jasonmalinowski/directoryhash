using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryHash
{
    internal static class Utilities
    {
        public static string ToHexString(this ImmutableArray<byte> bytes)
        {
            return bytes.Aggregate("", (s, b) => s + b.ToString("x2"));
        }
    }
}
