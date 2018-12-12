using System;
using System.Collections;
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
        private Text _instructionMessage;
        private UnityAction _nextButtonAction;

        private const string ZerothMessage = "Please read this carefully before you continue";
        private const string FirstMessage = "Press \"next\" when you are ready to continue";
        private const string SecondMessage = "Press \"next\" again to confirm";

        void Awake()
        {
            _displayableTextFields = new Dictionary<int, bool>();
            _currentlyDisplayed = new Dictionary<int, bool>();
            _backButton = transform.Find("Panel").Find("ControlArea").Find("FlowControlButtons").Find("BackButton").GetComponent<Button>();
            _nextButton = transform.Find("Panel").Find("ControlArea").Find("FlowControlButtons").Find("NextButton").GetComponent<Button>();
            _instructionMessage = transform.Find("Panel").Find("ControlArea").Find("Instructions").GetComponent<Text>();
            _nameHolder = transform.Find("Panel").Find("questionName");


            SetInstruction("");
        }
        
        /// <summary>
        /// Connects the next and back button of an question instance to the questionnaire manager.
        /// </summary>
        /// <remarks>
        /// Stores the next button action in case other actions need to be added to the listener.
        /// </remarks>
        /// <param name="backButtonAction"></param>
        /// <param name="nextButtonAction"></param>
        public void AddBackAndNextButtons(UnityAction backButtonAction, UnityAction nextButtonAction)
        {
            _nextButtonAction = nextButtonAction;
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

        /// <summary>
        /// Set whether next can be pressed.
        ///
        /// Displays a message that the task needs to be read.
        /// </summary>
        public void DisableInteractableNextButton()
        {
            _nextButton.interactable = false;
            SetInstruction(ZerothMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public IEnumerator EnableInteractableNextButton(int delay)
        {
            yield return new WaitForSeconds(delay);
            _nextButton.interactable = true;
            SetInstruction(FirstMessage);
        }

        /// <summary>
        /// Requires a confirmation before the next button can be pressed.
        /// </summary>
        public void RequireConfirmationToContinue()
        {
            _nextButton.onClick.RemoveListener(_nextButtonAction);
            _nextButton.onClick.AddListener(()=>
            {
                SetInstruction(SecondMessage);
                _nextButton.onClick.AddListener(_nextButtonAction);
            });
        }

        public void SetInstruction(string message)
        {
            _instructionMessage.text = message;
        }
    }
}

