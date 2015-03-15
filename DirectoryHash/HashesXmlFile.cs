using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DirectoryHash
{
    internal sealed class HashesXmlFile
    {
        private readonly HashedDirectory _rootDirectory;

        private HashesXmlFile(HashedDirectory rootDirectory)
        {
            _rootDirectory = rootDirectory;
            TouchUpdateTime();
        }

        public HashedDirectory RootDirectory { get { return _rootDirectory; } }

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

        public static HashesXmlFile CreateNew()
        {
            return new HashesXmlFile(new HashedDirectory());
        }

        public static HashesXmlFile ReadFrom(string filename)
        {
            using (var xmlReader = XmlReader.Create(filename, new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                xmlReader.ReadToDescendant("hashes");

                xmlReader.MoveToAttribute("updateTime");
                var updateTime = DateTime.ParseExact(xmlReader.Value, "O", CultureInfo.InvariantCulture);
                xmlReader.Read();

                var directory = HashedDirectory.ReadFrom(xmlReader);
                var xmlFile = new HashesXmlFile(directory);
                xmlFile.UpdateTime = updateTime;
                return xmlFile;
            }
        }

        public void WriteTo(string fileName)
        {
            var writerSettings = new XmlWriterSettings { Indent = true };

            using (var xmlWriter = XmlWriter.Create(fileName, writerSettings))
            {
                xmlWriter.WriteStartElement("hashes");
                xmlWriter.WriteAttributeString("updateTime", UpdateTime.ToString("O", CultureInfo.InvariantCulture));
                _rootDirectory.WriteTo(xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }
    }
}
