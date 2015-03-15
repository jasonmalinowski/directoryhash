using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;

namespace DirectoryHash
{
    /// <summary>
    /// Stores the hashes and attributes of a file. Immutable.
    /// </summary>
    internal sealed class HashedFile
    {
        private readonly ImmutableArray<byte> _sha1Hash;
        private readonly ImmutableArray<byte> _sha256Hash;

        public HashedFile(ImmutableArray<byte> sha1Hash, ImmutableArray<byte> sha256Hash)
        {
            Debug.Assert(sha1Hash.Length == 20);
            Debug.Assert(sha256Hash.Length == 32);

            _sha1Hash = sha1Hash;
            _sha256Hash = sha256Hash;
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
                sha256Hash: sha256Algorithm.Hash.ToImmutableArray());
        }

        public ImmutableArray<byte> Sha1Hash { get { return _sha1Hash; } }
        public ImmutableArray<byte> Sha256Hash { get { return _sha256Hash; } }

        public static HashedFile ReadFrom(XmlReader reader)
        {
            if (!reader.IsStartElement("file"))
            {
                throw new Exception("Expected file element.");
            }

            var sha1Hash = ImmutableArray<byte>.Empty;
            var sha256Hash = ImmutableArray<byte>.Empty;

            reader.ReadToFollowing("hash");
            ReadHash(reader, ref sha1Hash, ref sha256Hash);
            ReadHash(reader, ref sha1Hash, ref sha256Hash);

            return new HashedFile(sha1Hash, sha256Hash);
        }

        private static void ReadHash(XmlReader reader, ref ImmutableArray<byte> sha1Hash, ref ImmutableArray<byte> sha256Hash)
        {
            reader.MoveToAttribute("algorithm");
            var algorithm = reader.Value;
            reader.MoveToContent();
            var hashString = reader.ReadElementContentAsString();

            if (algorithm == "sha1")
            {
                sha1Hash = hashString.FromHexString();
            }
            else if (algorithm == "sha256")
            {
                sha256Hash = hashString.FromHexString();
            }
        }

        public void WriteTo(XmlWriter writer, string fileName)
        {
            writer.WriteStartElement("file");
            writer.WriteAttributeString("name", fileName);

            writer.WriteStartElement("hash");
            writer.WriteAttributeString("algorithm", "sha1");
            writer.WriteValue(Sha1Hash.ToHexString());
            writer.WriteEndElement();

            writer.WriteStartElement("hash");
            writer.WriteAttributeString("algorithm", "sha256");
            writer.WriteValue(Sha256Hash.ToHexString());
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}
