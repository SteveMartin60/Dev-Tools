using IconsRestorer.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace IconsRestorer.Code
{
    internal class storage
    {
        private static readonly string FilePath = @"D:\Dev-Ops\Deploy Workstation\icon-positions.xml";

        public void SaveIconPositions(IEnumerable<NamedDesktopPoint> iconPositions, IDictionary<string, string> registryValues)
        {
            var xDoc = new XDocument
            (
                new XElement
                (
                    "Desktop",
                    new XElement
                    (
                        "Icons",
                        iconPositions.Select
                        (
                            p => new XElement
                            (
                                "Icon",
                                new XAttribute("x", p.X),
                                new XAttribute("y", p.Y),
                                new XText     (p.Name  )
                            )
                        )
                    ),
                    new XElement
                    (
                        "Registry", registryValues.Select
                        (
                            p => new XElement
                            (
                                "Value", 
                                new XElement ("Name", new XCData(p.Key  )), 
                                new XElement ("Data", new XCData(p.Value))
                            )
                        )
                    )
                )
            );

            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(FilePath, settings))
            {
                xDoc.WriteTo(writer);
            }
        }

        public (List<NamedDesktopPoint> IconPositions, Dictionary<string, string> RegistryValues) LoadIconPositions()
        {
            if (!File.Exists(FilePath))
            {
                return (new List<NamedDesktopPoint>(), new Dictionary<string, string>());
            }

            try
            {
                XDocument doc = XDocument.Load(FilePath);

                var iconPositions = doc.Descendants("Icon")
                    .Select(i => new NamedDesktopPoint(
                        i.Value,
                        int.Parse(i.Attribute("x")?.Value ?? "0"),
                        int.Parse(i.Attribute("y")?.Value ?? "0")))
                    .ToList();

                var registryValues = doc.Descendants("Value")
                    .ToDictionary(
                        v => v.Element("Name")?.Value ?? string.Empty,
                        v => v.Element("Data")?.Value ?? string.Empty);

                return (iconPositions, registryValues);
            }
            catch (Exception ex)
            {
                // Consider logging the error
                return (new List<NamedDesktopPoint>(), new Dictionary<string, string>());
            }
        }
    }
}
