﻿using System;
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
            return MainCore(new DirectoryInfo(Environment.CurrentDirectory), args);
        }

        internal static int MainCore(DirectoryInfo currentDirectory, string[] args)
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

                    Recompute(currentDirectory);
                    return 0;

                case "update":

                    Update(currentDirectory);
                    return 0;

                case "purge":

                    var dryRun = remainingArgs.Contains("--dry-run");
                    var directories = remainingArgs.Where(arg => arg != "--dry-run");

                    Purge(currentDirectory, directories, dryRun);
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

        private static void Recompute(DirectoryInfo directoryToHash)
        {
            var hashesFile = HashesXmlFile.CreateNew(directoryToHash);
            var configuration = Configuration.ReadFrom(directoryToHash);

            hashesFile.HashedDirectory.RefreshFrom(
                directoryToHash,
                shouldInclude: info => ShouldInclude(info, hashesFile, configuration),
                shouldReprocessFile: file => true,
                reportDirectory: d => Console.WriteLine("Recomputing hashes of " + d.FullName + "..."));

            hashesFile.WriteToHashesXml();
        }

        private static void Update(DirectoryInfo directoryToHash)
        {
            var hashesFile = HashesXmlFile.ReadFrom(directoryToHash);
            var configuration = Configuration.ReadFrom(directoryToHash);
            var originalUpdateTime = hashesFile.UpdateTime;

            hashesFile.TouchUpdateTime();

            hashesFile.HashedDirectory.RefreshFrom(
                directoryToHash,
                shouldInclude: info => ShouldInclude(info, hashesFile, configuration),
                shouldReprocessFile: file => file.IsModifiedAfter(originalUpdateTime),
                reportDirectory: d => Console.WriteLine("Updating hashes of " + d.FullName + "..."));

            hashesFile.WriteToHashesXml();
        }

        private static void Purge(DirectoryInfo directoryToPurge, IEnumerable<string> directories, bool dryRun)
        {
            var knownFiles = new Dictionary<HashedFile, string>();

            foreach (var directory in directories)
            {
                var hashesFile = HashesXmlFile.ReadFrom(new DirectoryInfo(directory));
                var configuration = Configuration.ReadFrom(new DirectoryInfo(directory));

                hashesFile.EnumerateFiles(
                    shouldInclude: info => ShouldInclude(info, hashesFile, configuration),
                    hashedFileAction: (fileInfo, hashedFile) => knownFiles[hashedFile] = fileInfo.FullName,
                    unhashedFileAction: fileInfo => Utilities.WriteColoredConsoleLine(ConsoleColor.Yellow, "File {0} doesn't have an updated hash and will be ignored", fileInfo.FullName));
            }

            var directoryToPurgeHashes = HashesXmlFile.ReadFrom(directoryToPurge);
            var directoryToPurgeConfiguration = Configuration.ReadFrom(directoryToPurge);

            var directoriesWithDeletedFiles = new HashSet<string>();

            directoryToPurgeHashes.EnumerateFiles(
                shouldInclude: info => ShouldInclude(info, directoryToPurgeHashes, directoryToPurgeConfiguration),
                hashedFileAction: (fileInfo, hashedFile) =>
                {
                    string matchingFile;

                    if (knownFiles.TryGetValue(hashedFile, out matchingFile))
                    {
                        Console.WriteLine("Deleting file {0} that matches {1}", fileInfo.FullName, matchingFile);

                        if (!dryRun)
                        {
                            // Files that are marked read-only can't be deleted by FileInfo.Delete()
                            fileInfo.IsReadOnly = false;
                            fileInfo.Delete();

                            directoriesWithDeletedFiles.Add(fileInfo.Directory.FullName);
                        }
                    }
                },
                unhashedFileAction: fileInfo => Utilities.WriteColoredConsoleLine(ConsoleColor.Yellow, "Skipping {0} since it doesn't have an updated hash", fileInfo.FullName));

            if (!dryRun)
            {
                // Try cleaning up child directories. We don't call this on the root since we never want to try deleting that
                foreach (var childDirectory in directoryToPurge.GetDirectories())
                {
                    TryCleanupEmptyDirectory(childDirectory, directoryToPurgeConfiguration, directoriesWithDeletedFiles);
                }
            }
        }

        private static bool TryCleanupEmptyDirectory(DirectoryInfo directory, Configuration configuration, HashSet<string> potentiallyEmptyDirectories)
        {
            // If we're ignoring it entirely, don't even traverse
            if (!ShouldInclude(directory, hashesFile: null, configuration: configuration))
            {
                return false;
            }

            bool cleanedUpAtLeastOneChildDirectory = false;

            // First, cleanup any children
            foreach (var childDirectory in directory.GetDirectories())
            {
                if (TryCleanupEmptyDirectory(childDirectory, configuration, potentiallyEmptyDirectories))
                {
                    // Child couldn't be cleaned up, so nor can we
                    return false;
                }
                else
                {
                    cleanedUpAtLeastOneChildDirectory = true;
                }
            }

            // If it's not empty, definitely we can't do anything
            if (directory.EnumerateFileSystemInfos().Any())
            {
                return false;
            }

            // We know we have an empty directory. We should delete if either it's because we
            // deleted a file earlier, or deleted a directory now
            if (cleanedUpAtLeastOneChildDirectory || potentiallyEmptyDirectories.Contains(directory.FullName))
            {
                directory.Delete(recursive: false);
            }

            return false;
        }

        private static bool ShouldInclude(FileSystemInfo info, HashesXmlFile hashesFile, Configuration configuration)
        {
            return !info.IsHiddenAndSystem() && info.FullName != hashesFile?.FullName && configuration.ShouldInclude(info);
        }
    }
}
