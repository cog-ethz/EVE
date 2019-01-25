using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.Questionnaire.XMLHelper.VisualStimuli
{
    [Serializable]
    public class Times
    {

        [XmlAttribute] public float Fixation, Exposition, Decision;
    }
}
