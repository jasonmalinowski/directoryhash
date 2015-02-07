using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryHash.Tests
{
    public sealed class HashedFileTests
    {
        [Fact]
        public void HashEmptyFile()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var file = temporaryDirectory.CreateFileWithContent("Empty", new byte[] { });
                var hashedFile = HashedFile.FromFile(file);

                Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", hashedFile.Sha1Hash.ToHexString());
                Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hashedFile.Sha256Hash.ToHexString());
            }
        }

        [Fact]
        public void HashQuickBrownFox()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var file = temporaryDirectory.CreateFileWithContent("Fox", Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog."));
                var hashedFile = HashedFile.FromFile(file);

                Assert.Equal("408d94384216f890ff7a0c3528e8bed1e0b01621", hashedFile.Sha1Hash.ToHexString());
                Assert.Equal("ef537f25c895bfa782526529a9b63d97aa631564d5d789c2b765448c8635fb6c", hashedFile.Sha256Hash.ToHexString());
            }
        }

        [Fact]
        public void HashBigFile()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var file = temporaryDirectory.CreateFileWithContent("Big", Encoding.ASCII.GetBytes(new string('?', 10000)));
                var hashedFile = HashedFile.FromFile(file);

                Assert.Equal("bd863f7ad80cf80cd9a1e775ef6784771e0f6faa", hashedFile.Sha1Hash.ToHexString());
                Assert.Equal("0f4f2c1a00aeace453d8ac52859764112aaf28b3e35dc0d0e3948ba7f4819714", hashedFile.Sha256Hash.ToHexString());
            }
        }
    }
}
