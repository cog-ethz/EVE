using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.EVE.Scripts.Questionnaire.Questions;


namespace Assets.EVE.Scripts.Questionnaire
{
    public class QuestionStorage : MonoBehaviour
    {

        private Questions.Question _associatedQuestion;

        public void AssociateQuestion(Questions.Question associatedQuestion)
        {
            this._associatedQuestion = associatedQuestion;
        }

        public Questions.Question GetQuestion()
        {
            return _associatedQuestion;
        }
    }
}

