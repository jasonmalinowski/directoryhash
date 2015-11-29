using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryHash.Tests
{
    internal static class TestUtilities
    {
        public static void Run(this TemporaryDirectory directory, params string[] args)
        {
            Assert.Equal(0, Program.MainCore(directory.Directory, args));
        }
    }
}
