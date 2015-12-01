using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryHash.Tests
{
    public class RecomputeTests : IDisposable
    {
        private readonly TemporaryDirectory temporaryDirectory = new TemporaryDirectory();

        void IDisposable.Dispose()
        {
            temporaryDirectory.Dispose();
        }

        [Fact]
        public void RecomputeOnEmptyDirectory()
        {
            temporaryDirectory.Run("recompute");

            var hashes = HashesXmlFile.ReadFrom(temporaryDirectory.Directory);

            Assert.Empty(hashes.HashedDirectory.Directories);
            Assert.Empty(hashes.HashedDirectory.Files);
        }

        [Fact]
        public void RecomputeUpdatesTimeStamp()
        {
            var timeRange = DateTimeRange.CreateSurrounding(
                () => temporaryDirectory.Run("recompute"));

            var hashes = HashesXmlFile.ReadFrom(temporaryDirectory.Directory);

            timeRange.AssertContains(hashes.UpdateTime);
        }

        [Fact]
        public void RecomputeWithFile()
        {
            var file = temporaryDirectory.CreateFileWithContent("Fox", Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog."));
            temporaryDirectory.Run("recompute");

            var hashes = HashesXmlFile.ReadFrom(temporaryDirectory.Directory);
            var rehashedFile = HashedFile.FromFile(file);

            Assert.Equal(rehashedFile, hashes.HashedDirectory.Files["Fox"]);
        }
    }
}
