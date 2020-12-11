using System;
using System.Xml.Serialization;

namespace Query.Models
{
    public class Xml
    {
        [Serializable]
        [XmlRoot("ids")]
        public class Ids
        {
            [XmlElement("id")]
            public int[] Id { get; set; }
        }
    }
}
