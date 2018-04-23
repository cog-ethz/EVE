using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML
{
    [Serializable]
    public class SceneSettings
    {
        [XmlArray]
        [XmlArrayItem("Scene")]
        public List<string> Scenes;
    }
}