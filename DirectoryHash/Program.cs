using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DirectoryHash
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            var remainingArgs = args.Skip(1);

            switch (args[0])
            {
                case "recompute":

                    Recompute();
                    return 0;

                case "update":

                    Update();
                    return 0;

                case "purge":

                    var dryRun = remainingArgs.Contains("--dry-run");
                    var directories = remainingArgs.Where(arg => arg != "--dry-run");

                    Purge(directories, dryRun);
                    return 0;

                default:

                    PrintUsage();
                    return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: directoryhash recompute");
            Console.WriteLine("       directoryhash update");
            Console.WriteLine("       directoryhash purge [--dry-run] directory [directory...]");
        }

        private static void Recompute()
        {
            var directoryToHash = new DirectoryInfo(Environment.CurrentDirectory);
            var hashesFile = HashesXmlFile.CreateNew(directoryToHash);

            hashesFile.HashedDirectory.RefreshFrom(
                directoryToHash, 
                shouldInclude: info => !info.IsHiddenAndSystem() && info.FullName != hashesFile.FullName,
                shouldReprocessFile: file => true,
                reportDirectory: d => Console.WriteLine("Recomputing hashes of " + d.FullName + "..."));

            hashesFile.WriteToHashesXml();
        }
        
        private static void Update()
        {
            var directoryToHash = new DirectoryInfo(Environment.CurrentDirectory);
            var hashesFile = HashesXmlFile.ReadFrom(directoryToHash);

            hashesFile.TouchUpdateTime();

            hashesFile.HashedDirectory.RefreshFrom(
                directoryToHash,
                shouldInclude: info => !info.IsHiddenAndSystem() && info.FullName != hashesFile.FullName,
                shouldReprocessFile: file => file.IsModifiedAfter(hashesFile.UpdateTime),
                reportDirectory: d => Console.WriteLine("Updating hashes of " + d.FullName + "..."));

            hashesFile.WriteToHashesXml();
        }

        private static void Purge(IEnumerable<string> directories, bool dryRun)
        {
            var knownFiles = new Dictionary<HashedFile, string>();

            foreach (var directory in directories)
            {
                var hashesFile = HashesXmlFile.ReadFrom(new DirectoryInfo(directory));

                hashesFile.EnumerateFiles(
                    hashedFileAction: (fileInfo, hashedFile) => knownFiles[hashedFile] = fileInfo.FullName,
                    unhashedFileAction: fileInfo => Utilities.WriteColoredConsoleLine(ConsoleColor.Yellow, "File {0} doesn't have an updated hash and will be ignored", fileInfo.FullName));
            }

            var directoryToPurge = HashesXmlFile.ReadFrom(new DirectoryInfo(Environment.CurrentDirectory));

            directoryToPurge.EnumerateFiles(
                hashedFileAction: (fileInfo, hashedFile) =>
                {
                    string matchingFile;

                    if (knownFiles.TryGetValue(hashedFile, out matchingFile))
                    {
                        Console.WriteLine("Deleting file {0} that matches {1}", fileInfo.FullName, matchingFile);

                        if (!dryRun)
                        {
                            fileInfo.Delete();
                        }
                    }
                },
                unhashedFileAction: fileInfo => Utilities.WriteColoredConsoleLine(ConsoleColor.Yellow, "Skipping {0} since it doesn't have an updated hash", fileInfo.FullName));
        }
    }
}
