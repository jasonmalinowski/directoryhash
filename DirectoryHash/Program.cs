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

            switch (args[0])
            {
                case "recompute":

                    Recompute();
                    return 0;

                case "update":

                    Update();
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
                shouldReprocessFile: file => file.CreationTimeUtc > hashesFile.UpdateTime || file.LastWriteTimeUtc > hashesFile.UpdateTime,
                reportDirectory: d => Console.WriteLine("Updating hashes of " + d.FullName + "..."));

            hashesFile.WriteToHashesXml();
        }
    }
}
