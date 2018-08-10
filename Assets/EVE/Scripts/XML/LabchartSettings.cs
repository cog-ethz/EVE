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
        public string StarterPath, CommentWriterPath, ParticipantsPath;

        [XmlArray]
        [XmlArrayItem("Sensor")]
        public List<string> Commenters;
    }
}