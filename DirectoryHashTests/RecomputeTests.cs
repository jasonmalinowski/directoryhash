using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryHash.Tests
{
    public class RecomputeTests
    {
        [Fact]
        public void RecomputeOnEmptyDirectory()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                temporaryDirectory.Run("recompute");

                var hashes = HashesXmlFile.ReadFrom(temporaryDirectory.Directory);

                Assert.Empty(hashes.HashedDirectory.Directories);
                Assert.Empty(hashes.HashedDirectory.Files);
            }
        }

        [Fact]
        public void RecomputeUpdatesTimeStamp()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var timeRange = DateTimeRange.CreateSurrounding(
                    () => temporaryDirectory.Run("recompute"));

                var hashes = HashesXmlFile.ReadFrom(temporaryDirectory.Directory);

                timeRange.AssertContains(hashes.UpdateTime);
            }
        }

        [Fact]
        public void RecomputeWithFile()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var file = temporaryDirectory.CreateFileWithContent("Fox", Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog."));
                temporaryDirectory.Run("recompute");

                var hashes = HashesXmlFile.ReadFrom(temporaryDirectory.Directory);
                var rehashedFile = HashedFile.FromFile(file);

                Assert.Equal(rehashedFile, hashes.HashedDirectory.Files["Fox"]);
            }
        }
    }
}
