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
        public TemporaryDirectory()
        {
            Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.Create();
        }

        public FileInfo CreateFileWithContent(string fileName, byte[] contents)
        {
            var file = new FileInfo(Path.Combine(Directory.FullName, fileName));
            file.Directory.Create();

            using (var stream = file.OpenWrite())
            {
                stream.Write(contents, 0, contents.Length);
            }

            return file;
        }

        public DirectoryInfo Directory { get; }

        public void Dispose()
        {
            Directory.Delete(recursive: true);
        }
    }
}
