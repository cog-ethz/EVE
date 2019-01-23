using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.Questionnaire.XMLHelper
{
    [Serializable]
    public class Label
    {
        [XmlIgnore]
        public string Text { get; set; }
        [XmlElement("Text")]
        public System.Xml.XmlCDataSection TextToXml
        {
            get { return new System.Xml.XmlDocument().CreateCDataSection(Text); }
            set { Text = value.Value; }
        }

        [XmlIgnore]
        public int? Output { get; set; }

        [XmlAttribute("Output")]
        public string OutputToXml
        {
            get { return (Output.HasValue) ? Output.ToString() : null; }
            set { Output = !string.IsNullOrEmpty(value) ? int.Parse(value) : (int?)null; }
        }

        [XmlIgnore]
        public bool? Answerable { get; set; }

        
        [XmlAttribute("Answerable")]
        public string AnswerableToXml
        {
            get { return (Answerable.HasValue) ? Answerable.ToString() : null; }
            set { Answerable = !string.IsNullOrEmpty(value) && bool.Parse(value); }
        }

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
