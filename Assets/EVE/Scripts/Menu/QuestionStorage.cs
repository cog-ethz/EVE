using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.EVE.Scripts.Questionnaire.Questions;


namespace Assets.EVE.Scripts.Questionnaire
{
    public class QuestionStorage : MonoBehaviour
    {

        private Question _associatedQuestion;

        public void AssociateQuestion(Question associatedQuestion)
        {
            _associatedQuestion = associatedQuestion;
        }

        public Question GetQuestion()
        {
            return _associatedQuestion;
        }
    }
}

