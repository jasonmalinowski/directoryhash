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

                default:

                    PrintUsage();
                    return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: directoryhash recompute");
        }

        private static void Recompute()
        {
            var directory = new HashedDirectory();
            var updatedTime = DateTime.UtcNow;

            var directoryToHash = new DirectoryInfo(Environment.CurrentDirectory);
            var outputFile = new FileInfo(Path.Combine(directoryToHash.FullName, "Hashes.xml"));
            
            directory.RefreshFrom(
                directoryToHash, 
                shouldInclude: info => !info.IsHiddenAndSystem() && info.FullName != outputFile.FullName,
                shouldReprocessFile: file => true,
                reportDirectory: d => Console.WriteLine("Recomputing hashes of " + d.FullName + "..."));

            var writerSettings = new XmlWriterSettings { Indent = true };

            using (var xmlWriter = XmlWriter.Create(outputFile.FullName, writerSettings))
            {
                xmlWriter.WriteStartElement("hashes");
                xmlWriter.WriteAttributeString("updateTime", updatedTime.ToString("O"));
                directory.WriteTo(xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }
    }
}
