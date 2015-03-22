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

        public static ImmutableArray<byte> FromHexString(this string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                 bytes[i] = byte.Parse(hexString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return ImmutableArray.Create(bytes);
        }

        public static bool IsHiddenAndSystem(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System);
        }

        public static bool IsModifiedAfter(this FileInfo file, DateTime dateTime)
        {
            return file.CreationTimeUtc > dateTime || file.LastWriteTimeUtc > dateTime;
        }

        internal static void WriteColoredConsoleLine(ConsoleColor color, string format, params object[] args)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            try
            {
                Console.WriteLine(format, args);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
    }
}
