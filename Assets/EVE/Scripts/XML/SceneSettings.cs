using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Assets.EVE.Scripts.XML.XMLHelper;

namespace Assets.EVE.Scripts.XML
{
    [Serializable]
    public class SceneSettings
    {
        [XmlArray]
        [XmlArrayItem("Scene")]
        public List<SceneEntry> Scenes;
    }
}