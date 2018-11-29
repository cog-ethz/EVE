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
        private string[] _temporaryStringAnswers;

        public TextQuestion() { }

        public TextQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public TextQuestion(string name, string text)
        {
            Name = name;
            Text = text;
            RowLabels = null;
            NRows =  1;
        }

        public TextQuestion(string name, string text, List<Label> rowLabels)
        {
            Name = name;
            Text = text;
            RowLabels = rowLabels;
            NRows = RowLabels != null ? RowLabels.Count : 1;
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

            if (q.QuestionType == (int)Enums.Question.Text)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
                RowLabels = new List<Label>();
                if (q.Labels == null)
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
            NRows = RowLabels != null ? RowLabels.Count : 1;
        }

        public override KeyValuePair<int, string>[] GetAnswer()
        {
            if (_temporaryStringAnswers == null) return null;
            var entryArray = new KeyValuePair<int, string>[_temporaryStringAnswers.Length];
            var i = 0;
            foreach (var answer in _temporaryStringAnswers)
            {
                entryArray[i] = new KeyValuePair<int, string>(i, answer);
                i++;
            }
            return entryArray;
        }

        public override bool IsAnswered()
        {
            return _temporaryStringAnswers.All(answer => !string.IsNullOrEmpty(answer));
        }

        public override void RetainAnswer(string internalAnswer)
        {
            if (_temporaryStringAnswers==null)
                _temporaryStringAnswers = new string[NRows];
            _temporaryStringAnswers[0] = internalAnswer;
        }

        public override void RetainAnswer(int number, string internalAnswer)
        {
            if (_temporaryStringAnswers == null)
                _temporaryStringAnswers = new string[NRows];
            _temporaryStringAnswers[number] = internalAnswer;
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
