using System.Xml;
using System.Xml.Serialization;
using System.IO;


namespace Query.Common
{
    public static class Serializer
    {
        public static XmlDocument ToXmlDocument(object input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType());

            XmlDocument xd = null;

            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, input);

                memStm.Position = 0;

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, settings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }

            return xd;
        }
    }
}
