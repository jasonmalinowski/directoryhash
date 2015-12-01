using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryHash.Tests
{
    public class PurgeTests : IDisposable
    {
        private readonly TemporaryDirectory directoryToPurge = new TemporaryDirectory();
        private readonly TemporaryDirectory directoryWithCopies = new TemporaryDirectory();

        void IDisposable.Dispose()
        {
            directoryToPurge.Dispose();
            directoryWithCopies.Dispose();
        }

        [Fact]
        public void PurgeDeletesMatchingFileWithSameFileName()
        {
            var fileToPurge = directoryToPurge.CreateFileWithContent("Hello", new byte[] { 42 });
            var fileCopy = directoryWithCopies.CreateFileWithContent("Hello", new byte[] { 42 });

            directoryToPurge.Run("recompute");
            directoryWithCopies.Run("recompute");

            directoryToPurge.Run("purge", directoryWithCopies.Directory.FullName);

            Assert.False(fileToPurge.Exists);
            Assert.True(fileCopy.Exists);
        }

        [Fact]
        public void PurgeDeletesMatchingFileWithDifferentFileName()
        {
            var fileToPurge = directoryToPurge.CreateFileWithContent("Hello", new byte[] { 42 });
            var fileCopy = directoryWithCopies.CreateFileWithContent("Goodbye", new byte[] { 42 });

            directoryToPurge.Run("recompute");
            directoryWithCopies.Run("recompute");

            directoryToPurge.Run("purge", directoryWithCopies.Directory.FullName);

            Assert.False(fileToPurge.Exists);
            Assert.True(fileCopy.Exists);
        }

        [Fact]
        public void PurgeLeavesFileWithSameNameButDifferentContents()
        {
            var fileToPurge = directoryToPurge.CreateFileWithContent("Hello", new byte[] { 42 });
            var fileCopy = directoryWithCopies.CreateFileWithContent("Goodbye", new byte[] { 42 });

            directoryToPurge.Run("recompute");
            directoryWithCopies.Run("recompute");

            directoryToPurge.Run("purge", directoryWithCopies.Directory.FullName);

            Assert.False(fileToPurge.Exists);
            Assert.True(fileCopy.Exists);
        }
    }
}
