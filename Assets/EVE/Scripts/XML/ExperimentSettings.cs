using System;
using System.Xml.Serialization;

namespace EVE.Scripts.XML
{
    /// <summary>
    /// Stores experiment baseline settings
    /// </summary>
    [Serializable] 
    public class ExperimentSettings
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string EVEVersion;
    
        public DatabaseSettings DatabaseSettings;

        public MenuSettings MenuSettings;

        public SensorSettings SensorSettings;

        public ParameterSettings ParameterSettings;

        public QuestionnaireSettings QuestionnaireSettings;

        public SceneSettings SceneSettings;

        public LabchartSettings LabchartSettings;

        public UISettings UISettings;

    }
}