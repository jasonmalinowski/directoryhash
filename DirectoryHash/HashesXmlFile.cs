using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DirectoryHash
{
    internal sealed class HashesXmlFile
    {
        private readonly HashedDirectory _hashedDirectory;
        private readonly DirectoryInfo _rootDirectory;
        private readonly string _hashesXmlFileName;

        private HashesXmlFile(HashedDirectory hashedDirectory, DirectoryInfo rootDirectory)
        {
            _hashedDirectory = hashedDirectory;
            _rootDirectory = rootDirectory;
            _hashesXmlFileName = GetHashesXmlFileName(rootDirectory);
            TouchUpdateTime();
        }

        private static string GetHashesXmlFileName(DirectoryInfo directory)
        {
            return Path.Combine(directory.FullName, "Hashes.xml");
        }

        public HashedDirectory HashedDirectory { get { return _hashedDirectory; } }
        public string FullName { get { return _hashesXmlFileName; } }

        /// <summary>
        /// The update timestamp of the file. This is set to the time any update to the file
        /// began. This allows us to detect any changes to files we already hashed during a long
        /// hashing session.
        /// </summary>
        public DateTime UpdateTime { get; private set; }

        public void TouchUpdateTime()
        {
            this.UpdateTime = DateTime.UtcNow;
        }

        public static HashesXmlFile CreateNew(DirectoryInfo rootDirectory)
        {
            return new HashesXmlFile(new HashedDirectory(), rootDirectory);
        }

        public static HashesXmlFile ReadFrom(DirectoryInfo rootDirectory)
        {
            using (var xmlReader = XmlReader.Create(GetHashesXmlFileName(rootDirectory), new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                xmlReader.ReadToDescendant("hashes");

                xmlReader.MoveToAttribute("updateTime");
                var updateTime = DateTime.ParseExact(xmlReader.Value, "O", CultureInfo.InvariantCulture);
                xmlReader.Read();

                var directory = HashedDirectory.ReadFrom(xmlReader);
                var xmlFile = new HashesXmlFile(directory, rootDirectory);
                xmlFile.UpdateTime = updateTime;
                return xmlFile;
            }
        }

        public void WriteToHashesXml()
        {
            var writerSettings = new XmlWriterSettings { Indent = true };

            using (var xmlWriter = XmlWriter.Create(GetHashesXmlFileName(_rootDirectory), writerSettings))
            {
                xmlWriter.WriteStartElement("hashes");
                xmlWriter.WriteAttributeString("updateTime", UpdateTime.ToString("O", CultureInfo.InvariantCulture));
                _hashedDirectory.WriteTo(xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }
    }
}
