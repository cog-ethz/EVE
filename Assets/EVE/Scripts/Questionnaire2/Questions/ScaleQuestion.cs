﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire2.Enums;
using Assets.EVE.Scripts.Questionnaire2.Visitor;
using Assets.EVE.Scripts.Questionnaire2.XMLHelper;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire2.Questions
{
    public class ScaleQuestion : Questions.Question {

        [XmlAttribute]
        public Scale Scale;

        public string LeftLabel, RightLabel;

        [XmlIgnore]
        private int _temporaryIntAnswer = -1;

        public ScaleQuestion() { }

        public ScaleQuestion(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public ScaleQuestion(string name, string text, Scale scale, string leftLabel, string rightLabel)
        {
            Name = name;
            Text = text;
            Scale = scale;
            LeftLabel = leftLabel;
            RightLabel = rightLabel;
            NRows = 1;
            NColumns = 9;
        }

        public ScaleQuestion(string name, string text, Scale scale, string leftLabel, string rightLabel, List<Jump> jumps )
        {
            Name = name;
            Text = text;
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
                new string[2] {LeftLabel,RightLabel}, 
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
