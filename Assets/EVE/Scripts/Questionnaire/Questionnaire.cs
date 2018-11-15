using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.Questionnaire
{
    [Serializable]
    public class Questionnaire
    {
        [XmlAttribute]
        public string Name;

        [XmlArray]
        [XmlArrayItem("QuestionSet")]
        public List<string> QuestionSets;

        public Questionnaire() { }

        public Questionnaire(string name)
        {
            Name = name;
            QuestionSets = new List<string>();
        }

    }
}