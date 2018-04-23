using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML
{
    /// <summary>
    /// Stores questionnaires names for synchronisation with DB.
    /// </summary>
    /// <remarks>
    /// Questionnaires need to be either within EVE, i.e. `EVE/Resources/Questionnaire/`,
    /// or within the Experiment, i.e. `Experiment/Resources/Questionnaire/`.
    /// </remarks>
    [Serializable]
    public class QuestionnaireSettings
    {
        [XmlArray]
        [XmlArrayItem("Questionnaire")]
        public List<string> Questionnaires;
    }
}