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

        [Fact]
        public void PurgeDeletesEmptyDirectories()
        {
            var fileToPurge = directoryToPurge.CreateFileWithContent(@"SubDirectory1\SubDirectory2\Hello", new byte[] { 42 });
            var fileCopy = directoryWithCopies.CreateFileWithContent("Goodbye", new byte[] { 42 });

            directoryToPurge.Run("recompute");
            directoryWithCopies.Run("recompute");

            directoryToPurge.Run("purge", directoryWithCopies.Directory.FullName);

            Assert.Empty(directoryToPurge.Directory.GetDirectories());
        }

        [Fact]
        public void PurgeLeavesUnrelatedEmptyDirectories()
        {
            var fileToPurge = directoryToPurge.CreateFileWithContent(@"SubDirectory1\SubDirectory2\Hello", new byte[] { 42 });
            var fileCopy = directoryWithCopies.CreateFileWithContent("Goodbye", new byte[] { 42 });
            var directoryToLeave = directoryToPurge.Directory.CreateSubdirectory("UnrelatedSubDirectory");

            directoryToPurge.Run("recompute");
            directoryWithCopies.Run("recompute");

            directoryToPurge.Run("purge", directoryWithCopies.Directory.FullName);

            Assert.Equal(directoryToLeave.FullName, directoryToPurge.Directory.GetDirectories().Single().FullName);
        }
    }
}
