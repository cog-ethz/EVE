using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EVE.Scripts.XML
{
    /// <summary>
    /// Stores sensor settings.
    /// </summary>
    [Serializable]
    public class SensorSettings
    {
        [XmlAttribute]
        // ReSharper disable once InconsistentNaming
        public bool Labchart, H7Server, MiddleVR;

        [XmlArray]
        [XmlArrayItem("Sensor")]
        public List<string> Sensors;
    }
}