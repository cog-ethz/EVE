using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;

namespace Assets.EVE.Scripts.Questionnaire.Questions
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

        /// <summary>
        /// Empty constructor for the XML generation process.
        /// </summary>
        public Question()
        {
            NColumns = 1;
            NRows = 1;
        }

        /// <summary>
        /// Simple constructor.
        /// </summary>
        /// <param name="name">Name of the question.</param>
        /// <param name="text">Text to be displayed to users.</param>
        public Question(string name, string text)
        {
            Name = name;
            NColumns = 1;
            NRows = 1;
            Text = text;
        }

        /// <summary>
        /// Part of the visitor pattern for the QuestionnaireManager
        /// to instantiate a question on the screen.
        /// </summary>
        /// <param name="qv">The questionnaireManager interface for the visitor</param>
        public abstract void Accept(IQuestionVisitor qv);

        /// <summary>
        /// Exports all properties of a question into a QuestionData object
        /// to be stored in a database.
        /// </summary>
        /// <param name="questionSet">The question set the question will belong to.</param>
        /// <returns>Database-ready representation of the question.</returns>
        internal virtual QuestionData AsDatabaseQuestion(string questionSet)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used internally to transform QuestionData into a question.
        /// </summary>
        /// <param name="q">QuestionData from a database</param>
        internal virtual void FromDatabaseQuestion(QuestionData q)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all answers to a question in a readable format for the database.
        ///
        /// The keys are the index to a reference element in the question, the value
        /// is the information to be stored.
        /// </summary>
        /// <returns>An array of key value pairs to be inserted into the database</returns>
        public virtual Dictionary<int, string> GetAnswer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Should return true if an answer to a question has been provided
        /// </summary>
        /// <returns>Whether teh answer was given.</returns>
        public virtual bool IsAnswered()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Store an integer at a specific location.
        /// </summary>
        /// <param name="positionOffset">Location of the integer</param>
        /// <param name="answer">The integer to be stored</param>
        public virtual void RetainAnswer(int positionOffset, int answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Store a single string in the answer.
        /// </summary>
        /// <param name="answer">The string to be stored</param>
        public virtual void RetainAnswer(string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Store a string at a specific location
        /// </summary>
        /// <param name="positionOffset">Location of the string</param>
        /// <param name="answer">The string to be stored</param>
        public virtual void RetainAnswer(int positionOffset, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the name of question to which to jump
        /// </summary>
        /// <returns>Question name</returns>
        public virtual string GetJumpDestination()
        {
            throw new NotImplementedException();
        }
    }
}