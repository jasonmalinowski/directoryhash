using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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

        public static bool IsHiddenAndSystem(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System);
        }
    }
}
