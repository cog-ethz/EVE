using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML.XMLHelper
{
    [Serializable]
    public class ReferenceResolution
    {
        [XmlAttribute]
        public float X, Y;
    }
}
