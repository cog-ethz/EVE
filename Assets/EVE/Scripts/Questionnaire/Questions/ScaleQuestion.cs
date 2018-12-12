using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Enums;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    public class ScaleQuestion : Questions.Question {

        [XmlAttribute]
        public Scale Scale;

        public string LeftLabel, RightLabel, Image;

        [XmlIgnore]
        private int _temporaryIntAnswer = -1;

        public ScaleQuestion() { }

        public ScaleQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public ScaleQuestion(string name, string text, string image, Scale scale, string leftLabel, string rightLabel)
        {
            Name = name;
            Text = text;
            Image = image;
            Scale = scale;
            LeftLabel = leftLabel;
            RightLabel = rightLabel;
            NRows = 1;
            NColumns = 9;
        }

        public ScaleQuestion(string name, string text, string image, Scale scale, string leftLabel, string rightLabel,int nColumns)
        {
            Name = name;
            Text = text;
            Image = image;
            Scale = scale;
            LeftLabel = leftLabel;
            RightLabel = rightLabel;
            NRows = 1;
            NColumns = nColumns;
        }

        public ScaleQuestion(string name, string text, string image, Scale scale, string leftLabel, string rightLabel, List<Jump> jumps )
        {
            Name = name;
            Text = text;
            Image = image;
            Scale = scale;
            LeftLabel = leftLabel;
            RightLabel = rightLabel;
            Jumps = jumps;
            NRows = 1;
            NColumns = 9;
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Scale,
                new int[3] { NRows, NColumns, (int)Scale },
                string.IsNullOrEmpty(Image) ? new string[2] { LeftLabel, RightLabel } : new string[3] { LeftLabel, RightLabel, Image },
                null);
        }

        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            if (q.QuestionType == (int)Enums.Question.Scale)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
                NRows = q.Vals[0];
                NColumns = q.Vals[1];
                Scale = (Scale)q.Vals[2];
                LeftLabel = q.Labels[0];
                RightLabel = q.Labels[1];
                Image = q.Labels.Length<=2?null:q.Labels[2];
            }
            else
            {
                Debug.LogError("The question type is wrong: " + q.QuestionType);
            }
        }

        public override KeyValuePair<int, string>[] GetAnswer()
        {
            return new [] { new KeyValuePair<int, string>(_temporaryIntAnswer, "")} ;
        }

        public override bool IsAnswered()
        {
            return _temporaryIntAnswer > -1;
        }

        public override void RetainAnswer(int internalnumber)
        {
            _temporaryIntAnswer = internalnumber;
        }

        public override void RetainAnswer(int positionOffset, int answer)
        {
            _temporaryIntAnswer = answer == 1 ? positionOffset : -1;
        }

        public override string GetJumpDestination()
        {
            var answerB = new StringBuilder(new string('F', NRows * NColumns));
            answerB[_temporaryIntAnswer] = 'T';
            var answer = answerB.ToString();
            if (Jumps != null)
            {
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
