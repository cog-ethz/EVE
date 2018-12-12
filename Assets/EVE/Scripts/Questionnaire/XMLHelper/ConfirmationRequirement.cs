using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.Questionnaire.XMLHelper
{
    [Serializable]
    public class ConfirmationRequirement {

        [XmlAttribute]
        public bool Required;

        [XmlAttribute]
        public int ConfirmationDelay;

        public ConfirmationRequirement() { }

        public ConfirmationRequirement(bool required, int confirmationDelay)
        {
            Required = required;
            ConfirmationDelay = confirmationDelay;
        }
    }
}
