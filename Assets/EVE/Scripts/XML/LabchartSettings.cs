using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML
{
    /// <summary>
    /// Stores important locations for labchart scripts
    /// </summary>
    [Serializable]
    public class LabchartSettings
    {
        [XmlIgnore]
        public string Path { get; set; }
        [XmlElement("Path")]
        public string PathToXml
        {
            get
            {
                if (!Path.EndsWith("\\"))
                {
                    Path += "\\";
                }
                return Path;
            }
            set
            {
                Path = value;
                if (!Path.EndsWith("\\"))
                {
                    Path += "\\";
                }
            }
        }

        [XmlArray]
        [XmlArrayItem("Sensor")]
        public List<string> Commenters;
    }
}