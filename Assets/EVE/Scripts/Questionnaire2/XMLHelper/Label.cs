using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.Questionnaire2.XMLHelper
{
    [Serializable]
    public class Label
    {
        [XmlIgnore]
        public string Text { get; set; }
        [XmlElement("Text")]
        public System.Xml.XmlCDataSection TextToXml
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(Text);
            }
            set
            {
                Text = value.Value;
            }
        }

        [XmlAttribute]
        public int? Output;

        [XmlAttribute]
        public bool? Answerable;

        [XmlAttribute]
        public string Image;

        public Label() { }

        public Label(string text)
        {
            Text = text;
        }
        public Label(string text, int output)
        {
            Text = text;
            Output = output;
        }

        public Label(string text, int output, string image)
        {
            Text = text;
            Output = output;
            Image = image;
        }

        public Label(string text, int output, bool answerable)
        {
            Text = text;
            Output = output;
            Answerable = answerable;
        }

        public Label(string text, int output, string image, bool answerable)
        {
            Text = text;
            Output = output;
            Image = image;
            Answerable = answerable;
        }
        
    }
}
