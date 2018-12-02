using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    public class LadderQuestion : Questions.Question
    {
        [XmlIgnore]
        public string LadderText { get; set; }
        [XmlElement("LadderText")]
        public System.Xml.XmlCDataSection LadderTextToXml
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(LadderText);
            }
            set
            {
                LadderText = value.Value;
            }
        }

        [XmlIgnore]
        private int _temporaryIntAnswer = -1;

        public LadderQuestion() { }

        public LadderQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public LadderQuestion(string name, string text, string ladderText)
        {
            Name = name;
            Text = text;
            LadderText = ladderText;
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Ladder,
                new int[2] { NRows, NColumns },
                new string[1] { LadderText },
                null);
        }
        
        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            if (q.QuestionType == (int)Enums.Question.Ladder)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
                LadderText = q.Labels[0];
            }
            else
            {
                Debug.LogError("The question type is wrong: " + q.QuestionType);
            }
        }

        public override KeyValuePair<int, string>[] GetAnswer()
        {
            return new[] { new KeyValuePair<int, string>(_temporaryIntAnswer, "") };
        }

        public override bool IsAnswered()
        {
            return _temporaryIntAnswer > -1;
        }
        public override void RetainAnswer(int answer)
        {
            _temporaryIntAnswer = answer;
        }

        public override void RetainAnswer(int positionOffset, int answer)
        {
            _temporaryIntAnswer = positionOffset;
        }

        public override string GetJumpDestination()
        {
            if (Jumps != null)
            {
                var answerB = new StringBuilder(new string('F', NRows * NColumns));
                answerB[_temporaryIntAnswer] = 'T';
                var answer = answerB.ToString();
                return
                (from jump in Jumps
                 where jump.Activator.Equals("*") || jump.Activator.Equals(answer)
                 select jump.Destination).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public override void Accept(IQuestionVisitor qv)
        {
            qv.Visit(this);
        }
    }
}
