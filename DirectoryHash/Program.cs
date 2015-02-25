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

            directory.RefreshFrom(new DirectoryInfo(Environment.CurrentDirectory), shouldReprocessFile: file => true);

            var writerSettings = new XmlWriterSettings { Indent = true };

            using (var xmlWriter = XmlWriter.Create(Path.Combine(Environment.CurrentDirectory, "Hashes.xml"), writerSettings))
            {
                xmlWriter.WriteStartElement("hashes");
                xmlWriter.WriteAttributeString("updateTime", updatedTime.ToString("O"));
                directory.WriteTo(xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }
    }
}
