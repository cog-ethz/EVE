using System;
using System.Xml.Serialization;

namespace EVE.Scripts.XML
{
    /// <summary>
    /// Stores menu information for EVE.
    /// </summary>
    [Serializable]
    public class MenuSettings
    {
        [XmlAttribute]
        public string FirstMenu = "Startup Menu";
    }
}
