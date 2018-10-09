using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML
{
    /// <summary>
    /// Stores experiment baseline settings
    /// </summary>
    [Serializable] 
    public class ExperimentSettings
    {
        [XmlAttribute]
        public string Name;
    
        public DatabaseSettings DatabaseSettings;

        public SensorSettings SensorSettings;

        public ParameterSettings ParameterSettings;

        public QuestionnaireSettings QuestionnaireSettings;

        public SceneSettings SceneSettings;

        public LabchartSettings LabchartSettings;

        public UISettings UISettings;

    }
}