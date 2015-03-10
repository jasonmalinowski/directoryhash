using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DirectoryHash
{
    internal sealed class HashedDirectory
    {
        private readonly SortedDictionary<string, HashedDirectory> _directories = new SortedDictionary<string, HashedDirectory>(StringComparer.Ordinal);
        private readonly SortedDictionary<string, HashedFile> _files = new SortedDictionary<string, HashedFile>(StringComparer.Ordinal);

        private const string DirectoryElementLocalName = "directory";

        public SortedDictionary<string, HashedDirectory> Directories { get { return _directories; } }
        public SortedDictionary<string, HashedFile> Files { get { return _files; } }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement(DirectoryElementLocalName);
            WriteChildren(writer);
            writer.WriteEndElement();
        }

        private void WriteTo(XmlWriter writer, string directoryName)
        {
            writer.WriteStartElement(DirectoryElementLocalName);
            writer.WriteAttributeString("name", directoryName);
            WriteChildren(writer);
            writer.WriteEndElement();
        }

        private void WriteChildren(XmlWriter writer)
        {
            foreach (var directory in _directories)
            {
                directory.Value.WriteTo(writer, directoryName: directory.Key);
            }

            foreach (var file in _files)
            {
                file.Value.WriteTo(writer, file.Key);
            }
        }

        public void RefreshFrom(DirectoryInfo directory, Predicate<FileSystemInfo> shouldInclude, Predicate<FileInfo> shouldReprocessFile)
        {
            RefreshChildDirectoriesFrom(directory, shouldInclude, shouldReprocessFile);
            RefreshChildFilesFrom(directory, shouldInclude, shouldReprocessFile);
        }

        private void RefreshChildDirectoriesFrom(DirectoryInfo directory, Predicate<FileSystemInfo> shouldInclude, Predicate<FileInfo> shouldReprocessFile)
        {
            // We'll remove directories we traverse from this HashSet, and whatever is left over
            // we can prune. It's intentionally case-sensitive as to reprocess any directories
            // whose name changes case.
            var unvisitedDirectories = new HashSet<string>(_directories.Keys, StringComparer.Ordinal);

            foreach (var childDirectory in directory.GetDirectories())
            {
                if (shouldInclude(childDirectory))
                {
                    HashedDirectory hashedChildDirectory;

                    if (!_directories.TryGetValue(childDirectory.Name, out hashedChildDirectory))
                    {
                        hashedChildDirectory = new HashedDirectory();
                        _directories.Add(childDirectory.Name, hashedChildDirectory);
                    }

                    unvisitedDirectories.Remove(childDirectory.Name);
                    hashedChildDirectory.RefreshFrom(childDirectory, shouldInclude, shouldReprocessFile);
                }
            }

            foreach (var unvisitedDirectory in unvisitedDirectories)
            {
                _directories.Remove(unvisitedDirectory);
            }
        }

        private void RefreshChildFilesFrom(DirectoryInfo directory, Predicate<FileSystemInfo> shouldInclude, Predicate<FileInfo> shouldReprocessFile)
        {
            // We'll remove files we process from this HashSet, and whatever is left over
            // we can prune. It's intentionally case-sensitive as to reprocess any directories
            // whose name changes case.
            var unvisitedFiles = new HashSet<string>(_files.Keys, StringComparer.Ordinal);

            foreach (var file in directory.GetFiles())
            {
                if (shouldInclude(file))
                {
                    HashedFile hashedFile;

                    if (!_files.TryGetValue(file.Name, out hashedFile))
                    {
                        _files.Add(file.Name, HashedFile.FromFile(file));
                    }
                    else
                    {
                        // We already have data for it. But we'll only recompute it if needed.
                        if (shouldReprocessFile(file))
                        {
                            _files[file.Name] = HashedFile.FromFile(file);
                        }
                    }

                    unvisitedFiles.Remove(file.Name);
                }
            }

            foreach (var unvisitedFile in unvisitedFiles)
            {
                _files.Remove(unvisitedFile);
            }
        }
    }
}
