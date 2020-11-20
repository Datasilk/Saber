using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Query.Models
{
    public class Xml
    {
        [Serializable]
        [XmlRoot("keys")]
        public class Keys
        {
            [XmlElement("key")]
            public List<Key> Key { get; set; }
        }

        public class Key
        {
            [XmlAttribute("name")]
            public Key Name { get; set; }
            [XmlAttribute("value")]
            public Key Value { get; set; }
            [XmlAttribute("isplatform")]
            public Key IsPlatform { get; set; }
        }
    }
}
