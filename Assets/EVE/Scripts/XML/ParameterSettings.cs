using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EVE.Scripts.XML
{
    /// <summary>
    /// Stores experiment parameters for EVE.
    /// </summary>
    [Serializable]
    public class ParameterSettings
    {
        [XmlArray]
        [XmlArrayItem("Parameter")]
        public List<string> Parameters;
    }
}