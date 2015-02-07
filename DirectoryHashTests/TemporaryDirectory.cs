using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryHash.Tests
{
    internal sealed class TemporaryDirectory : IDisposable
    {
        private readonly DirectoryInfo _directory;

        public TemporaryDirectory()
        {
            _directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            _directory.Create();
        }

        public FileInfo CreateFileWithContent(string fileName, byte[] contents)
        {
            var file = new FileInfo(Path.Combine(_directory.FullName, fileName));

            using (var stream = file.OpenWrite())
            {
                stream.Write(contents, 0, contents.Length);
            }

            return file;
        }

        public void Dispose()
        {
            _directory.Delete(recursive: true);
        }
    }
}
