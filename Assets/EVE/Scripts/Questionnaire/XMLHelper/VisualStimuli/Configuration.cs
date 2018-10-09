using Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;
using System;
using System.Xml.Serialization;
using Type = Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli.Type;

namespace Assets.EVE.Scripts.Questionnaire.XMLHelper.VisualStimuli
{
    [Serializable]
    public class Configuration {
        [XmlAttribute]
        public Separator Separator;

        [XmlAttribute]
        public Choice Choice;

        [XmlAttribute]
        public Randomisation Randomisation;

        [XmlAttribute]
        public Type Type;

        [XmlAttribute]
        public bool SeparatorFirst;
    }
}
