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

        /// <summary>
        /// Recursively enumerates all files in the directory, calling the appropriate function for files found.
        /// </summary>
        /// <param name="hashedFileAction">A function called with a file and it's file hashes, if the file hasn't been modified since
        /// <see cref="UpdateTime" />.</param>
        /// <param name="unhashedFileAction">A function called with a file if no valid hashes are available.</param>
        public void EnumerateFiles(Action<FileInfo, HashedFile> hashedFileAction, Action<FileInfo> unhashedFileAction)
        {
            EnumerateFiles(_rootDirectory, _hashedDirectory, hashedFileAction, unhashedFileAction);
        }

        private void EnumerateFiles(DirectoryInfo directory, HashedDirectory hashedDirectory, Action<FileInfo, HashedFile> hashedFileAction, Action<FileInfo> unhashedFileAction)
        {
            foreach (var file in directory.GetFiles())
            {
                if (!file.IsHiddenAndSystem() && file.FullName != _hashesXmlFileName)
                {
                    HashedFile hashedFile;

                    if (hashedDirectory != null && hashedDirectory.Files.TryGetValue(file.Name, out hashedFile))
                    {
                        // Is the hash actually valid?
                        if (file.IsModifiedAfter(UpdateTime))
                        {
                            unhashedFileAction(file);
                        }
                        else
                        {
                            hashedFileAction(file, hashedFile);
                        }
                    }
                    else
                    {
                        // The file hasn't been hashed at all
                        unhashedFileAction(file);
                    }
                }
            }

            foreach (var childDirectory in directory.GetDirectories())
            {
                if (!childDirectory.IsHiddenAndSystem())
                {
                    HashedDirectory hashedChildDirectory = null;

                    // If we don't have hashes at all for the child directory we'll just pass null through
                    if (hashedDirectory != null)
                    {
                        hashedDirectory.Directories.TryGetValue(childDirectory.Name, out hashedChildDirectory);
                    }

                    EnumerateFiles(childDirectory, hashedChildDirectory, hashedFileAction, unhashedFileAction);
                }
            }
        }
    }
}
