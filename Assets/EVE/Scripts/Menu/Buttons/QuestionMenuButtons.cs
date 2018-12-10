using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire.Questions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class QuestionMenuButtons : MonoBehaviour
    {
        /// <summary>
        /// Adds a question to the menu to manage
        /// </summary>
        public Question AssociatedQuestion { get; set; }
        private Button _backButton;
        private Button _nextButton;
        private Dictionary<int, bool> _displayableTextFields;
        private Dictionary<int, bool> _currentlyDisplayed;


        void Awake()
        {
            _displayableTextFields = new Dictionary<int, bool>();
            _currentlyDisplayed = new Dictionary<int, bool>();
            _backButton = transform.Find("Panel").Find("QuestionControlButtons").Find("BackButton").GetComponent<Button>();
            _nextButton = transform.Find("Panel").Find("QuestionControlButtons").Find("NextButton").GetComponent<Button>();
        }
        
        public void AddBackAndNextButtons(UnityAction backButtonAction, UnityAction nextButtonAction)
        {
            _backButton.onClick.AddListener(backButtonAction);
            _nextButton.onClick.AddListener(nextButtonAction);
        }

        public void SetActiveDisableTextField(int positionOffset, bool enabled)
        {
            _displayableTextFields.Add(positionOffset, enabled);
            _currentlyDisplayed.Add(positionOffset,false);
        }

        public void SetAnswerString(int positionOffset, string answer)
        {
            AssociatedQuestion.RetainAnswer(positionOffset, answer);
            Debug.Log("Passed answer " + answer + " at " + positionOffset);
        }

        public void SetAnswerInt(int positionOffset, int answer)
        {
            AssociatedQuestion.RetainAnswer(positionOffset, answer);
            Debug.Log("Passed answer " + answer + " at " + positionOffset);

            if (!_displayableTextFields.ContainsKey(positionOffset)) return;
            //switch whether the text can be edited based on whether button is selected
            _currentlyDisplayed[positionOffset] = !_currentlyDisplayed[positionOffset];
            gameObject.GetComponentsInChildren<InputField>()[positionOffset].interactable = _currentlyDisplayed[positionOffset];
        }

        public void DisableBackButton()
        {
            _backButton.interactable = false;
        }
    }
}

