using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Demonstration.Serialization
{
    class XMLSerializer : ISerializer
    {
        public string Serizlize(object o)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(o.GetType());
            string resultString;
            using (var stringWriter = new StringWriter())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                {
                    SetSerializationOptions(xmlTextWriter);
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    xmlSerializer.Serialize(xmlTextWriter, o, ns);
                    resultString = stringWriter.ToString();
                }
            }
            return resultString;
        }

        private void SetSerializationOptions(XmlTextWriter xmlTextWriter)
        {
            xmlTextWriter.Formatting = Formatting.Indented;
        }
    }
}
