using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace DirectoryHash
{
    /// <summary>
    /// Stores the hashes and attributes of a file. Immutable.
    /// </summary>
    internal sealed class HashedFile
    {
        private readonly ImmutableArray<byte> _sha1Hash;
        private readonly ImmutableArray<byte> _sha256Hash;
        private readonly long _fileSize;
        private readonly DateTime _fileCreatedUtc;
        private readonly DateTime _fileModifiedUtc;

        public HashedFile(ImmutableArray<byte> sha1Hash, ImmutableArray<byte> sha256Hash, long fileSize, DateTime fileCreatedUtc, DateTime fileModifiedUtc)
        {
            Debug.Assert(sha1Hash.Length == 20);
            Debug.Assert(sha256Hash.Length == 32);

            _sha1Hash = sha1Hash;
            _sha256Hash = sha256Hash;
            _fileSize = fileSize;
            _fileCreatedUtc = fileCreatedUtc;
            _fileModifiedUtc = fileModifiedUtc;
        }

        public static HashedFile FromFile(FileInfo file)
        {
            // We'll compute two different hashes simultaneously for convenience and to limit
            // any chances of cryptographic trickery
            var sha1Algorithm = SHA1.Create();
            var sha256Algorithm = SHA256.Create();
            var hashAlgorithms = new List<HashAlgorithm> { sha1Algorithm, sha256Algorithm };

            foreach (var hashAlgorithm in hashAlgorithms)
            {
                hashAlgorithm.Initialize();
            }

            var buffer = new byte[4096];

            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    foreach (var hashAlgorithm in hashAlgorithms)
                    {
                        hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }
                }
            }

            foreach (var hashAlgorithm in hashAlgorithms)
            {
                hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
            }

            return new HashedFile(
                sha1Hash: sha1Algorithm.Hash.ToImmutableArray(),
                sha256Hash: sha256Algorithm.Hash.ToImmutableArray(),
                fileSize: file.Length,
                fileCreatedUtc: file.CreationTimeUtc,
                fileModifiedUtc: file.LastWriteTimeUtc);
        }

        public ImmutableArray<byte> Sha1Hash { get { return _sha1Hash; } }
        public ImmutableArray<byte> Sha256Hash { get { return _sha256Hash; } }
    }
}
