using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.Questionnaire.XMLHelper
{
    [Serializable]
    public class Jump
    {
        [XmlAttribute]
        public string Destination, Activator;

        public Jump () { }

        public Jump(string destination, string activator)
        {
            Destination = destination;
            Activator = activator;
        }

    }
}
