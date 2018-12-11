using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML.XMLHelper
{
    [Serializable]
    public class SceneEntry
    {
        [XmlAttribute] public bool Curtain;

        [XmlAttribute] public string Name;


        public SceneEntry()
        {
        }

        public SceneEntry(string name, bool curtain)
        {
            Name = name;
            Curtain = curtain;
        }
    }
}