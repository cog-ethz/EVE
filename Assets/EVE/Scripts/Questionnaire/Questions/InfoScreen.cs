

using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    /// <summary>
    /// This is only here to subtype the abstract Question 2 class. All functionality is there.s
    /// </summary>
    public class InfoScreen : Question
    {
        public InfoScreen() { }

        public InfoScreen(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public InfoScreen(string name, string text)
        {
            Name = name;
            Text = text;
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Info,
                null,
                null,
                null);
        }

        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            if (q.QuestionType == (int) Enums.Question.Info)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
            }
            else
            {
                Debug.LogError("The question type is wrong: " + q.QuestionType);
            }
        }

        public override KeyValuePair<int, string>[] GetAnswer()
        {
            return null;
        }

        public override bool IsAnswered()
        {
            return true;
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
