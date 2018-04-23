using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire2.Visitor;
using Assets.EVE.Scripts.Questionnaire2.XMLHelper;

namespace Assets.EVE.Scripts.Questionnaire2.Questions
{
    [Serializable]
    public abstract class Question
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public int NColumns, NRows;

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

        [XmlArray]
        [XmlArrayItem("Jump")]
        public List<Jump> Jumps;

        public Question()
        {
            NColumns = 1;
            NRows = 1;
        }

        public Question(string name, string text)
        {
            Name = name;
            NColumns = 1;
            NRows = 1;
            Text = text;
        }

        internal virtual QuestionData AsDatabaseQuestion(string questionSet)
        {
            throw new NotImplementedException();
        }

        internal virtual void FromDatabaseQuestion(QuestionData q)
        {
            throw new NotImplementedException();
        }

        public abstract void Accept(IQuestionVisitor qv);

        public virtual KeyValuePair<int, string>[] GetAnswer()
        {
            throw new NotImplementedException();
        }
        public virtual bool IsAnswered()
        {
            throw new NotImplementedException();
        }

        public virtual void RetainAnswer(int internalnumber)
        {
            throw new NotImplementedException();
        }

        public virtual void RetainAnswer(string internalAnswer)
        {
            throw new NotImplementedException();
        }

        public virtual void RetainAnswer(int number, string internalAnswer)
        {
            throw new NotImplementedException();
        }

        public virtual string GetJumpDestination()
        {
            throw new NotImplementedException();
        }
    }
}