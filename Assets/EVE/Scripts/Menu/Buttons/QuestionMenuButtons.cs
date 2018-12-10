using System;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.Utils;
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
        private Transform _nameHolder;


        void Awake()
        {
            _displayableTextFields = new Dictionary<int, bool>();
            _currentlyDisplayed = new Dictionary<int, bool>();
            _backButton = transform.Find("Panel").Find("QuestionControlButtons").Find("BackButton").GetComponent<Button>();
            _nextButton = transform.Find("Panel").Find("QuestionControlButtons").Find("NextButton").GetComponent<Button>();
            _nameHolder = transform.Find("Panel").Find("questionName");
        }
        
        public void AddBackAndNextButtons(UnityAction backButtonAction, UnityAction nextButtonAction)
        {
            _backButton.onClick.AddListener(backButtonAction);
            _nextButton.onClick.AddListener(nextButtonAction);
        }

        public void SetActiveDisableTextField(int positionOffset, bool enabled)
        {
            if (!_displayableTextFields.ContainsKey(positionOffset))
            {
                _displayableTextFields.Add(positionOffset, enabled);
                _currentlyDisplayed.Add(positionOffset, enabled);
            }
            else
            {
                _displayableTextFields[positionOffset] = enabled;
                _currentlyDisplayed[positionOffset] = enabled;
            }
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
            if (answer > 0)
            {
                _currentlyDisplayed[positionOffset] = true;
            }
            else
            {
                _currentlyDisplayed[positionOffset] = false;
            }

            var child = GameObjectUtils.FindGameObjectInChildren(transform,"ResponseRows").GetChild(positionOffset);
            child.Find("InputField").GetComponent<InputField>().interactable = _currentlyDisplayed[positionOffset];
        }

        /// <summary>
        /// Makes the back button non-interactable.
        /// </summary>
        public void DisableBackButton()
        {
            _backButton.interactable = false;
        }

        /// <summary>
        /// Deactivates (removes) the next and back button.
        /// </summary>
        /// <remarks>
        /// If you use this, make sure that your participant has a way to continue.
        /// </remarks>
        public void DeactivateQuestionControlButtons()
        {
            _backButton.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
        }

        public void DeactivateQuestionName()
        {
            _nameHolder.gameObject.SetActive(false);
        }
    }
}

