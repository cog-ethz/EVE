

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using UnityEngine;

namespace Assets.EVE.Scripts.Questionnaire.Questions
{
    /// <summary>
    /// This is only here to subtype the abstract Question 2 class. All functionality is there.s
    /// </summary>
    public class InfoScreen : Question
    {
        [XmlElement] public ConfirmationRequirement ConfirmationRequirement;
        
        public InfoScreen() { }

        public InfoScreen(QuestionData questionData)
        {
            FromDatabaseQuestion(questionData);
        }

        public InfoScreen(string name, string text)
        {
            Name = name;
            Text = text;
            ConfirmationRequirement = new ConfirmationRequirement(false, 0);
        }

        public InfoScreen(string name, string text, ConfirmationRequirement confirmationRequirement)
        {
            Name = name;
            Text = text;
            ConfirmationRequirement = confirmationRequirement;
        }

        internal override QuestionData AsDatabaseQuestion(string questionSet)
        {
            var vals = ConfirmationRequirement.Required
                ? new[] {ConfirmationRequirement.Required ? 1 : 0, ConfirmationRequirement.ConfirmationDelay}
                : null;
            return new QuestionData(Name,
                Text,
                questionSet,
                (int)Enums.Question.Info,
                vals,
                null,
                null);
        }

        internal sealed override void FromDatabaseQuestion(QuestionData q)
        {
            if (q.QuestionType == (int) Enums.Question.Info)
            {
                Name = q.QuestionName;
                Text = q.QuestionText;
                ConfirmationRequirement = new ConfirmationRequirement
                {
                    Required = q.Vals != null && q.Vals[0] == 1, 
                    ConfirmationDelay = q.Vals != null && q.Vals.Length > 1 ? q.Vals[1] : 0
                };
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
