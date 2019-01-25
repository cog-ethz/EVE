using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML
{
    [Serializable]
    public class ParameterSettings
    {
        [XmlArray]
        [XmlArrayItem("Parameter")]
        public List<string> Parameters;
    }
}