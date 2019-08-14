using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    public class TextQuestion : Question
    {
        [XmlArray]
        [XmlArrayItem("Label")]
        public List<Label> RowLabels;

        [XmlIgnore]
        private Dictionary<int,string> _temporaryStringAnswers;

        public TextQuestion()
        {
            _temporaryStringAnswers = new Dictionary<int, string>();
        }

        public TextQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
            _temporaryStringAnswers = new Dictionary<int, string>();
        }

        public TextQuestion(string name, string text)
        {
            Name = name;
            Text = text;
            RowLabels = null;
            NRows =  1;
            _temporaryStringAnswers = new Dictionary<int, string>();
        }

        public TextQuestion(string name, string text, List<Label> rowLabels)
        {
            Name = name;
            Text = text;
            RowLabels = rowLabels;
            NRows = RowLabels?.Count ?? 1;
            _temporaryStringAnswers = new Dictionary<int, string>();
        }


        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            var labels = new string[RowLabels.Count];
            for (var i = 0; i < RowLabels.Count; i++)
            {
                labels[i] = RowLabels[i].Text;
            }
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Text,
                new int[2] { NRows, NColumns },
                labels,
                null);
        }

        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            _temporaryStringAnswers = new Dictionary<int, string>();
            if (q.QuestionType == (int)Enums.Question.Text)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
                RowLabels = new List<Label>();
                if (q.Labels == null || q.Labels.Length == 0)
                {
                    RowLabels = null;
                    return;
                }
                NRows = q.Labels.Length>0? q.Labels.Length:1;
                foreach (var l in q.Labels)
                {
                    RowLabels.Add(new Label(l));
                }
            }
            else
            {
                Debug.LogError("The question type is wrong: " + q.QuestionType);
            }
            NRows = RowLabels?.Count ?? 1;
        }

        public override Dictionary<int, string> GetAnswer() => _temporaryStringAnswers;

        public override bool IsAnswered()
        {
            return _temporaryStringAnswers.Count == NRows && _temporaryStringAnswers.All(answer => !string.IsNullOrEmpty(answer.Value));
        }
        
        public override void RetainAnswer(int offsetPosition, string answer)
        {
            _temporaryStringAnswers[offsetPosition] = answer;
        }

        public override string GetJumpDestination()
        {
            if (Jumps != null)
            {
                return
                (from jump in Jumps
                 where jump.Activator.Equals("*")
                 select jump.Destination).FirstOrDefault();
            }
            return null;
        }
        public override void Accept(IQuestionVisitor qv)
        {
            qv.Visit(this);
        }
    }
}
