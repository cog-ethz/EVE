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
    public class ScaleQuestion : Question {

        [XmlAttribute]
        public Scale Scale;

        [XmlAttribute] public bool LabelledToggles;

        public string LeftLabel, RightLabel, Image;
        
        [XmlIgnore]
        private int _temporaryIntAnswer = -1;

        public ScaleQuestion() { }

        public ScaleQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }
        
        public ScaleQuestion(string name, string text, string image, Scale scale, string leftLabel, string rightLabel, bool labelledToggles = false, int nColumns = 9, List<Jump> jumps = null)
        {
            Name = name;
            Text = text;
            Image = image;
            Scale = scale;
            LeftLabel = leftLabel;
            RightLabel = rightLabel;
            LabelledToggles = labelledToggles;
            NRows = 1;
            NColumns = nColumns;
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Scale,
                new int[4] { NRows, NColumns, (int)Scale , LabelledToggles?1:0},
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
                LabelledToggles = q.Vals[3] == 1;
                LeftLabel = q.Labels[0];
                RightLabel = q.Labels[1];
                Image = q.Labels.Length<=2?null:q.Labels[2];
            }
            else
            {
                Debug.LogError("The question type is wrong: " + q.QuestionType);
            }
        }

        public override Dictionary<int, string> GetAnswer() => new Dictionary<int, string> {{_temporaryIntAnswer, "1"}};

        public override bool IsAnswered() => _temporaryIntAnswer > -1;
        
        public override void RetainAnswer(int offsetPosition, int answer)
        {
            _temporaryIntAnswer = answer == 1 ? offsetPosition : -1;
        }

        public override string GetJumpDestination()
        {
            var answer = new StringBuilder(new string('F', NRows * NColumns)) { [_temporaryIntAnswer] = 'T' }.ToString();
            if (Jumps != null)
            {
                return
                (from jump in Jumps
                 where jump.Activator.Equals("*") || jump.Activator.Equals(answer)
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
