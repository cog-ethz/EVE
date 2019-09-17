using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.EVE.Scripts.Menu.Buttons;
using Assets.EVE.Scripts.Questionnaire.Enums;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Utils;
using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;
using Question = Assets.EVE.Scripts.Questionnaire.Questions.Question;

namespace Assets.EVE.Scripts.Questionnaire
{
    public class QuestionnaireManager : MonoBehaviour, IQuestionVisitor
    {
        private LoggingManager _log;
        private MenuManager _menuManager;

        private Questionnaire _questionnaire;
        private List<QuestionSet> _questionSets;
        private QuestionSet _currentQuestionSet;
        private Question _question;
    
        private int _currentQuestion, 
            _showedQuestions, 
            _totalQuestions, 
            _numberOfAnswers, 
            _displayedQuestion, 
            _questionSetIndex,
            _lastValidQuestion;
        private Stack<int> _previousQuestion, 
            _previousNofAnswers;
        private Dictionary<int, string> _oldAnswers;
        private bool _once;

        private Transform _customContent,
            _questionContent,
            _dynamicFieldsWithScrollbar,
            _dynamicField;

        private GameObject _questionPlaceholder;

        private List<string> _questionNames;
        private LaunchManager _launchManager;
        private QuestionnaireFactory _qf;
        private bool _resumeFromError;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _log = _launchManager.LoggingManager;
            _menuManager = _launchManager.MenuManager;
            _qf = new QuestionnaireFactory(_log,_launchManager.ExperimentSettings);
            
            enabled = false;
        }

        /// <summary>
        /// Initialises variables for questionnaire.
        /// </summary>
        public void DisplayQuestionnaire()
        {
            var questionnaireName = _launchManager.QuestionnaireName;
            _log.InsertLiveSystemEvent("QuestionnaireSystem", "Start Questionnaire", null, questionnaireName);

            var questionnaires = _qf.ReadQuestionnairesFromDb(new List<string>() { questionnaireName });
            _questionSets = _qf.ReadQuestionSetsFromDb(questionnaires);
            _questionnaire = questionnaires.First();

            _previousQuestion = new Stack<int>();
            _previousNofAnswers = new Stack<int>();
            _displayedQuestion = -1;
            _showedQuestions = 1;
            _lastValidQuestion = 1;

            _questionSetIndex = 0;
            LoadQuestionSetAt(_questionSetIndex);
        }

        void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Delete)
            {
                _currentQuestion = _totalQuestions;
            }
            if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
            {
                CloseQuestionnaire();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_resumeFromError || (_currentQuestion >= 0 && _currentQuestion < _totalQuestions && _displayedQuestion != _currentQuestion))
            {
                _resumeFromError = false;
               _displayedQuestion = _currentQuestion;
                _question = _currentQuestionSet.Questions[_currentQuestion];
                while (_question == null && _currentQuestion < _totalQuestions)
                {
                    _currentQuestion++;
                    _question = _currentQuestionSet.Questions[_currentQuestion];
                }

                _menuManager.InstantiateAndShowMenu("QuestionPlaceholder", "Questionnaire");
                _questionPlaceholder = _menuManager.CurrentMenu.gameObject;
                _questionPlaceholder.name = "QuestionPlaceholder";
                _questionContent = _questionPlaceholder.transform
                    .Find("Panel")
                    .Find("QuestionContent").transform;
                _dynamicFieldsWithScrollbar = _questionContent
                    .Find("DynFieldsWithScrollbar").transform;
                _dynamicField = _dynamicFieldsWithScrollbar
                    .Find("DynFields").transform;

                var questionMenuButtons = _questionPlaceholder.GetComponent<QuestionMenuButtons>();
                questionMenuButtons.AddBackAndNextButtons(GoBackOneQuestion, GoToNextQuestion);
                if (_showedQuestions <= _lastValidQuestion)
                    questionMenuButtons.DisableBackButton();
                
                DisplayQuestion();

                _oldAnswers = new Dictionary<int, string>();

            }
            else if (_currentQuestion >= _totalQuestions)
            {
                _log.InsertLiveSystemEvent("QuestionnaireSystem", "End Set", null, _questionSets[_questionSetIndex].Name);
                if (_currentQuestionSet == _questionSets.Last())
                {
                    if (_once) return;
                    _once = true;
                    CloseQuestionnaire();
                }
                else
                {
                    LoadQuestionSetAt(++_questionSetIndex);
                }
            }
        }

        /// <summary>
        /// Arranges the question into the placeholder.
        /// </summary>
        private void DisplayQuestion()
        {
            _questionPlaceholder.GetComponent<QuestionMenuButtons>().AssociatedQuestion = _question;
            _questionPlaceholder
                .transform.Find("Panel")
                .Find("questionName")
                .GetComponent<Text>()
                .text = "";
            _questionContent
                .Find("questionText")
                .GetComponent<Text>()
                .text = _question.Text;

            _question.Accept(this);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_questionContent.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Logs the end of a questionnaire, puts the questionnaire manager to
        /// sleep and manually moves to the next scene.
        /// </summary>
        private void CloseQuestionnaire()
        {
            _log.InsertLiveSystemEvent("QuestionnaireSystem", "Complete Questionnaire", null, _questionnaire.Name);
            _launchManager.MenuManager.CloseCurrentMenu();
            enabled = false;
            _once = false;
            _launchManager.ManualContinueToNextScene();
        }

        /// <summary>
        /// Loads a new question set and logs the start of the set.
        /// </summary>
        /// <param name="index">Index of Set</param>
        private void LoadQuestionSetAt(int index)
        {
            _currentQuestionSet = _questionSets[index];

            var name = _currentQuestionSet.Name;
            name = name.Length > 15 ? name.Substring(0, 15) : name;
            _log.InsertLiveSystemEvent("QuestionnaireSystem", "Start Set", null, name);

            _currentQuestion = 0;
            _totalQuestions = _currentQuestionSet.Questions.Count;
            _questionNames = _currentQuestionSet.Questions.Select(question => question.Name).ToList();
        }
        
        /// <summary>
        /// Resume questionnaire from error message.
        /// </summary>
        public void ContinueFromError()
        {
            enabled = true;
            _resumeFromError = true;
        }

        public void GoBackOneQuestion()
        {
            if (_questionSetIndex == 0 && _currentQuestion == 0) return;
            if (_currentQuestion == 0)
            { 
                LoadQuestionSetAt(--_questionSetIndex);
            }
            _currentQuestion = _previousQuestion.Pop();
            var qOld = _currentQuestionSet.Questions[_currentQuestion];
            _oldAnswers = _log.ReadAnswer(qOld.Name);
            _showedQuestions--;
            _numberOfAnswers = _previousNofAnswers.Pop();
        }

        public void GoToNextQuestion()
        {        
            //make sure that the question-window gave the information about the answers to the questionObject before saving answers!!!
            if (_question.IsAnswered())
            {

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
                var answer= _question.GetAnswer();
                if (answer != null) _log.InsertAnswer(_question.Name, _currentQuestionSet.Name, answer);

                _previousQuestion.Push(_currentQuestion);
                _showedQuestions++;

                var jumpDest = _question.GetJumpDestination();
                if (jumpDest == null)
                    _currentQuestion++;
                else
                    _currentQuestion = jumpDest.Equals("*") ? _totalQuestions : _questionNames.IndexOf(jumpDest);

                if (_currentQuestion >= _totalQuestions) return;
                _question = _currentQuestionSet.Questions[_currentQuestion];
                _oldAnswers = _log.ReadAnswer(_question.Name);
                
                _previousNofAnswers.Push(_numberOfAnswers);
                _numberOfAnswers++;
            }
            else
            {
                var tempAnswer = _question.GetAnswer();
                _oldAnswers =new Dictionary<int, string>();
                foreach (var keyValuePair in tempAnswer)
                {
                    _oldAnswers.Add(keyValuePair.Key,keyValuePair.Value);
                }
                enabled = false;
                _menuManager.DisplayErrorMessage("Please answer the question!","Questionnaire","QuestionnaireSystem");
            }
        }

        public void Visit(Question q)
        {
            throw new NotImplementedException();
        }

        public void Visit(InfoScreen q) { 
			var t = _questionContent.GetComponentInChildren<Text> ();
			t.alignment = TextAnchor.MiddleLeft;

			_dynamicFieldsWithScrollbar.gameObject.SetActive (false);


            if (q.ConfirmationRequirement.Required)
            {
                var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();

                qmb.RequireConfirmationToContinue();
                qmb.DisableInteractableNextButton();
                StartCoroutine(qmb.EnableInteractableNextButton(q.ConfirmationRequirement.ConfirmationDelay));
            }
		}

        public void Visit(ChoiceQuestion q)
        {
            var dynamicFieldSize = _dynamicField.GetComponent<RectTransform>().sizeDelta;
            var nRows = q.NRows;
            var nColumns = q.NColumns;
            var answernumber = 0;

            var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();

            var multiColObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Questionnaire/Content/ChoiceQuestionLayout");
            MenuUtils.PlaceElement(multiColObject, _dynamicField);
            var newParent = multiColObject.transform.Find("ResponseRows");

            var isMultiple = q.Choice == Choice.Multiple;

            if (nColumns > 1)
            {
                var cLabels = q.ColumnLabels.Select(l => l.Text).ToList();

                var topRow = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Questionnaire/Rows/TopRow");

                MenuUtils.PlaceElement(topRow, _questionContent);
                topRow.transform.SetSiblingIndex(1);

                var topLabelSize = MenuUtils.ComputeTopLabelSize(cLabels);
                var oldDelta = topRow.GetComponent<RectTransform>().sizeDelta;
                topRow.GetComponent<RectTransform>().sizeDelta = new Vector2(oldDelta.x, topLabelSize.y);


                for (var i = 0; i < nColumns; i++)
                {
                    //Set top labels
                    var filenameObj = MenuUtils.AddLabelText("Questionnaire/Rows/Elements/OneLabel", cLabels[i], topRow.transform);
                    filenameObj.GetComponent<RectTransform>().sizeDelta = topLabelSize;
                }

                var widthAllTopLabels = nColumns*topLabelSize.x;
                var sideLabelWidth = dynamicFieldSize.x - widthAllTopLabels;
                
                var leftPlaceholder = topRow.transform.Find("Placeholder");
                leftPlaceholder.GetComponent<RectTransform>().sizeDelta = new Vector2(sideLabelWidth, 0);

                for (var i = 0; i < nRows; i++)
                {
                    //Set side labels
                    var oneRow = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Questionnaire/Rows/OneRow");
                    MenuUtils.PlaceElement(oneRow, newParent);

                    var text = q.RowLabels!=null?q.RowLabels[i].Text:"";
                    var filenameObj = MenuUtils.AddLabelText("Questionnaire/Rows/Elements/OneLabel", text, oneRow.transform);
                    var tex = filenameObj.GetComponent<Text>();

                    var count = Regex.Split(tex.text, "\n").Length - 1;
                    var labelLength = MenuUtils.MessagePixelLength(tex.text, tex);
                    var rowHeight = (int) (Math.Ceiling(labelLength/sideLabelWidth) + count)*32;
                    filenameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sideLabelWidth, rowHeight);

                    for (var j = 0; j < nColumns; j++)
                    {
                        var button = MenuUtils.CreateToggleElement("Questionnaire/Rows/Elements/OneToggleButton", oneRow.transform, topLabelSize.x,100);
                        var btn = button.GetComponent<Toggle>();
                        if (!isMultiple) btn.group = oneRow.GetComponent<ToggleGroup>();


                        var iLocal = answernumber;
                        btn.isOn = _oldAnswers != null && _oldAnswers.ContainsKey(iLocal) ;
                        btn.onValueChanged.AddListener(isOn =>
                        {
                            qmb.SetAnswerInt(iLocal, isOn ? 1 : 0);
                        });
                        answernumber++;
                    }
                }
            }
            else
            {
                var rLabels = q.RowLabels.Select(l => l.Text).ToList();
                float length = MenuUtils.GetMaxTextLength(rLabels);
                for (var i = 0; i < nRows; i++)
                {
                    var hasText = q.RowLabels[i].Answerable.HasValue && q.RowLabels[i].Answerable.Value;
                
                    //Set side labels
                    GameObject oneRow;
                    if (q.RowLabels[i].Image != null)
                    {
                        oneRow = MenuUtils.AddLabelText("Questionnaire/Rows/ImageChoiceRow", rLabels[i], newParent, "OneLabel", length);
                        oneRow.transform.Find("RawImage").GetComponent<RawImage>().texture = Resources.Load<Sprite>(q.RowLabels[i].Image).texture;
                    }
                    else
                    {
                        var labelType = "Questionnaire/Rows/" + (hasText ? "TextChoiceRow" : "SingleChoiceRow");
                        oneRow = MenuUtils.AddLabelText(labelType, rLabels[i], newParent, "OneLabel", length);
                    }
                    MenuUtils.PlaceElement(oneRow, newParent);

                    var rightPlaceholderWidth = dynamicFieldSize.x - length;
                    var rightPlaceholder = oneRow.transform.Find("NoTextPlaceholder");
                    rightPlaceholder.GetComponent<RectTransform>().sizeDelta = new Vector2(rightPlaceholderWidth, 0);
                
                    var toggleObject = oneRow.transform.Find("OneToggleButton").gameObject;
                    toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 50);

                    var btn = toggleObject.transform.Find("ToggleButtons").GetComponent<Toggle>();
                    if (!isMultiple) btn.group = newParent.GetComponent<ToggleGroup>();



                    var iLocal = answernumber;
                    btn.onValueChanged.AddListener(isOn =>
                    {
                        qmb.SetAnswerInt(iLocal, isOn ? 1 : 0);
                    });
                    btn.isOn = false;

                    if (hasText)
                    {
                        var inpt = oneRow.transform.Find("InputField").GetComponent<InputField>();

                        inpt.onEndEdit.AddListener(answer =>
                        {
                            qmb.SetAnswerString(iLocal, answer);
                        });
                        if (_oldAnswers != null && _oldAnswers.ContainsKey(answernumber))
                        {
                            inpt.text = _oldAnswers[answernumber];
                        }
                        qmb.SetActiveDisableTextField(answernumber,false);
                    }

                    if (_oldAnswers != null && _oldAnswers.ContainsKey(answernumber)) { 
                        btn.isOn = true;
                        if (hasText)
                        {
                            qmb.SetActiveDisableTextField(answernumber, true);
                            oneRow.transform.Find("InputField").GetComponent<InputField>().text = _oldAnswers[answernumber];
                        }
                    
                    }
                    answernumber++;
                }
            }
        
        }

        public void Visit(TextQuestion q)
        {
            var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();
            var length = 0;
            if (q.RowLabels != null)
            {
                length = MenuUtils.GetMaxTextLength(q.RowLabels.Select(l => l.Text).ToList());
            }

            var answernumber = 0;
            for (var index = 0; index < q.NRows; index++)
            {
                var iLocal = index;

                var text = q.RowLabels != null ? q.RowLabels[index].Text : "";

                var labelAndField = MenuUtils.AddLabelText("Questionnaire/Rows/LabelledTextField", text, _dynamicField, "Label", length);

                var inpt = labelAndField.transform.Find("InputField").GetComponent<InputField>();

                inpt.onEndEdit.AddListener(answer =>
                {
                    qmb.SetAnswerString(iLocal, answer);
                });                        
                if (_oldAnswers != null && _oldAnswers.ContainsKey(answernumber))
                {
                    inpt.text = _oldAnswers[answernumber];
                }
                answernumber++;
            }
        }

        public void Visit(LadderQuestion q)
        {
            var ladder = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Questionnaire/Content/LadderContent");
            _customContent = ladder.transform;
            MenuUtils.PlaceElement(ladder, _questionContent);

            _dynamicFieldsWithScrollbar.gameObject.SetActive(false);

            _customContent.Find("LadderLabel").gameObject.GetComponent<Text>().text = q.LadderText;
            var ladderButtons = _customContent.Find("LadderGroup").Find("Buttons");

            var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();
            for (var i = 9; i >= 0; i--)
            {
                var btn = ladderButtons.Find("ToggleButtons" + i).GetComponent<Toggle>();

                var iLocal = i;
                btn.onValueChanged.AddListener(isOn =>
                {
                    qmb.SetAnswerInt(iLocal, isOn ? 1 : 0);
                });
                btn.isOn = _oldAnswers != null && _oldAnswers.ContainsKey(i);
            }
        }

        public void Visit(ScaleQuestion q)
        {
            _dynamicFieldsWithScrollbar.gameObject.SetActive(false);

            var scaleContent = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Questionnaire/Content/ScaleContent");
            _customContent = scaleContent.transform;
            MenuUtils.PlaceElement(scaleContent, _questionContent);

            //Load image
            var image = _customContent.Find("Image");
            var imageSource = "Textures/questionline";
            switch (q.Scale)
            {
                case Scale.Pleasure:
                    imageSource = "Textures/SAM-V-5";
                    break;
                case Scale.Arousal:
                    imageSource = "Textures/SAM-A-5";
                    break;
                case Scale.Dominance:
                    imageSource = "Textures/SAM-D-5";
                    break;
                case Scale.Custom:
                    imageSource = q.Image;
                    break;
            }
            image.GetComponent<Image>().overrideSprite = Resources.Load<Sprite>(imageSource);

            if (!q.LabelledToggles)
            {
                var labels = scaleContent.transform.Find("Buttons").gameObject.GetComponentsInChildren<Text>();
                foreach (var label in labels)
                {
                    label.gameObject.SetActive(false);
                }
            }

            var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();

            //Set label
            _customContent.Find("extremesText").Find("extreme1").gameObject.GetComponent<Text>().text = q.LeftLabel;
            _customContent.Find("extremesText").Find("extreme2").gameObject.GetComponent<Text>().text = q.RightLabel;

            var buttons = _customContent.Find("Buttons");
            for (var i = 0; i < q.NColumns; i++)
            {
                var btn = buttons.Find("ToggleButtons" + i).GetComponent<Toggle>();

                var iLocal = i;
                btn.onValueChanged.AddListener(isOn =>
                {
                    qmb.SetAnswerInt(iLocal, isOn?1:0);
                });
                btn.isOn = _oldAnswers != null && _oldAnswers.ContainsKey(i);
            }
        }

        public void Visit(VisualStimuli q)
        {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
            _lastValidQuestion = _currentQuestion + 1;
            var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();
            qmb.DeactivateQuestionControlButtons();
            qmb.DeactivateQuestionName();
            MenuUtils.ClearList(_questionContent);
            
            var rep =_questionContent.gameObject.AddComponent<Representations.VisualStimuli>();
            rep.Question = q;

            _questionContent.GetComponent<RectTransform>().sizeDelta = _launchManager.ExperimentSettings.UISettings.ReferenceResolution;
            
            var fixationScreen = GameObjectUtils.InstatiatePrefab("Prefabs/Questionnaire/VisualStimuli/Fixation");
            MenuUtils.PlaceElement(fixationScreen, _questionContent);
            fixationScreen.SetActive(false);
            rep.FixationSceen = fixationScreen;
            var decisionScreen = GameObjectUtils.InstatiatePrefab("Prefabs/Questionnaire/VisualStimuli/Decision");
            MenuUtils.PlaceElement(decisionScreen, _questionContent);
            decisionScreen.SetActive(false);
            rep.DecisionScreen = decisionScreen;
            decisionScreen.GetComponentInChildren<Text>().text = q.Text;
            var expositionScreen = GameObjectUtils.InstatiatePrefab("Prefabs/Questionnaire/VisualStimuli/Exposition");
            MenuUtils.PlaceElement(expositionScreen, _questionContent);
            expositionScreen.SetActive(false);
            rep.ExpositionScreen = expositionScreen;

            expositionScreen.transform.Find("Screen").GetComponent<RectTransform>().sizeDelta = _launchManager.ExperimentSettings.UISettings.ReferenceResolution;

            _questionContent.gameObject.SetActive(true);

            _questionContent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            rep.InitialiseRepresentation(this);
        }

    }
}
