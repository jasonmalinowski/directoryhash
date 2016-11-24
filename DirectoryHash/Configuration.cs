using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DirectoryHash
{
    internal sealed class Configuration
    {
        public ImmutableArray<Pattern> IgnoredDirectories { get; }
        public ImmutableArray<Pattern> IgnoredFiles { get; }

        private Configuration(ImmutableArray<Pattern> ignoredDirectories, ImmutableArray<Pattern> ignoredFiles)
        {
            IgnoredDirectories = ignoredDirectories;
            IgnoredFiles = ignoredFiles;
        }

        public static Configuration ReadFrom(DirectoryInfo directory)
        {
            string configurationFileName = Path.Combine(directory.FullName, "Hashes.config");

            if (!File.Exists(configurationFileName))
            {
                return new Configuration(
                    ignoredDirectories: ImmutableArray<Pattern>.Empty,
                    ignoredFiles: ImmutableArray<Pattern>.Empty);
            }

            var ignoredDirectories = ImmutableArray<Pattern>.Empty.ToBuilder();
            var ignoredFiles = ImmutableArray<Pattern>.Empty.ToBuilder();

            using (var xmlReader = XmlReader.Create(configurationFileName))
            {
                xmlReader.ReadToDescendant("exclude");

                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement("directories"))
                    {
                        xmlReader.MoveToAttribute("matching");
                        ignoredDirectories.Add(new Pattern(xmlReader.Value));
                    }
                    else if (xmlReader.IsStartElement("files"))
                    {
                        xmlReader.MoveToAttribute("matching");
                        ignoredFiles.Add(new Pattern(xmlReader.Value));
                    }
                }
            }

            return new Configuration(ignoredDirectories.ToImmutable(), ignoredFiles.ToImmutable());
        }

        /// <summary>
        /// Returns if the directory should be included per the exclusion rules.
        /// </summary>
        public bool ShouldInclude(FileSystemInfo info)
        {
            if (info is DirectoryInfo)
            {
                return !IgnoredDirectories.Any(p => p.NameMatchesPattern(info.Name));
            }
            else if (info is FileInfo)
            {
                return !IgnoredFiles.Any(p => p.NameMatchesPattern(info.Name));
            }

            return true;
        }
    }
}
