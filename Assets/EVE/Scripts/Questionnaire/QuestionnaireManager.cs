using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.EVE.Scripts.Menu.Buttons;
using Assets.EVE.Scripts.Questionnaire.Enums;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.Questionnaire.Visitor;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private Questions.Question _question;
    
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

        private GameObject _oldQuestionPlaceholder,
            _questionPlaceholder,
            _canvas;

        private List<string> _questionNames;
        private LaunchManager _launchManager;
        private QuestionnaireFactory _qf;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _log = _launchManager.LoggingManager;
            _qf = new QuestionnaireFactory(_log,_launchManager.ExperimentSettings);
            
            this.enabled = false;
        }

        public void DisplayQuestionnaire()
        {

            var questionnaireName = _launchManager.QuestionnaireName;

            var questionnaires = _qf.ReadQuestionnairesFromDb(new List<string>() { questionnaireName });
            _questionSets = _qf.ReadQuestionSetsFromDb(questionnaires);
            _questionnaire = questionnaires.First();

            _log.InsertLiveSystemEvent("QuestionnaireSystem", "Start QuestionnaireSystem", null, _questionnaire.Name);


            _previousQuestion = new Stack<int>();
            _previousNofAnswers = new Stack<int>();
            _displayedQuestion = -1;
            _showedQuestions = 1;
            _lastValidQuestion = 1;

            _questionSetIndex = 0;
            LoadQuestionSetAt(_questionSetIndex);


            _canvas = GameObject.Find("Canvas");

            _canvas.GetComponent<CanvasScaler>().referenceResolution = _launchManager.ExperimentSettings.UISettings.ReferenceResolution;
            _menuManager = _launchManager.MenuManager;
            _menuManager.CloseCurrentMenu();

            _oldQuestionPlaceholder = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/QuestionPlaceholder");
            MenuUtils.PlaceElement(_oldQuestionPlaceholder, _canvas.transform);
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
            if (_currentQuestion >= 0 && _currentQuestion < _totalQuestions && _displayedQuestion != _currentQuestion)
            {            
                _displayedQuestion = _currentQuestion;
                _question = _currentQuestionSet.Questions[_currentQuestion];
                while (_question == null && _currentQuestion < _totalQuestions)
                {
                    _currentQuestion++;
                    _question = _currentQuestionSet.Questions[_currentQuestion];
                }
            
                _menuManager.CloseMenu(_oldQuestionPlaceholder.GetComponent<Menu.BaseMenu>());
                _questionPlaceholder = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/QuestionPlaceholder");
                _questionPlaceholder.name = "QuestionPlaceholder";
                _oldQuestionPlaceholder.name = "Old QuestionPlaceholder";
                MenuUtils.PlaceElement(_questionPlaceholder, _canvas.transform);

                var questionMenuButtons = _questionPlaceholder.GetComponent<QuestionMenuButtons>();
                questionMenuButtons.AddBackAndNextButtons(GoBackOneQuestion, GoToNextQuestion);
                
                _questionContent = _questionPlaceholder.transform
                    .Find("Panel")
                    .Find("QuestionContent").transform;

                _dynamicFieldsWithScrollbar = _questionContent
                    .Find("DynFieldsWithScrollbar").transform;

                _dynamicField = _dynamicFieldsWithScrollbar
                    .Find("DynFields").transform;

                SetupQuestionDisplay();
                ClearQuestionContent();
            
                _question.Accept(this);
            
                _oldAnswers = new Dictionary<int, string>();
                UpdateQuestionDisplay();

                if (_showedQuestions <= _lastValidQuestion)
                    _menuManager.CurrentMenu
                        .getDynamicFields("BackButton")
                        .gameObject
                        .GetComponent<Button>()
                        .interactable = false;

                //StartCoroutine(GameObjectUtils.GameObjectUtils(_oldQuestionPlaceholder,5));
                Destroy(_oldQuestionPlaceholder);
				_oldQuestionPlaceholder = _questionPlaceholder;
            }
            else if (_currentQuestion == _totalQuestions)
            {
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
        /// Logs the end of a questionnaire, puts the questionnaire manager to
        /// sleep and manually moves to the next scene.
        /// </summary>
        private void CloseQuestionnaire()
        {
            _log.InsertLiveSystemEvent("Questionnaire", "Complete Questionnaire", null, _questionnaire.Name);
            _launchManager.MenuManager.CloseCurrentMenu();
            this.enabled = false;
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
            _log.insertLiveMeasurement("QuestionnaireSystem", "Start Set", null, name);

            _currentQuestion = 0;
            _totalQuestions = _currentQuestionSet.Questions.Count;
            _questionNames = _currentQuestionSet.Questions.Select(question => question.Name).ToList();
        }
        
        /// <summary>
        /// Delete all entries in the dynamic field.
        /// </summary>
        /// <remarks>
        /// Note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
        /// </remarks>
        /// <param name="dynFieldType">Dynamic Field to be cleared</param>
        private void ClearDynamicFields()
        {
            var entriesObjects = (from Transform entry in _dynamicField select entry.gameObject).ToList();
            foreach (var entryObject in entriesObjects) Destroy(entryObject);
        }

        private void ClearQuestionContent()
        {
            ClearDynamicFields();
            var topRowTransform = _questionContent.Find("TopRow(Clone)");
            if (topRowTransform != null)
            {
                Destroy(topRowTransform.gameObject);
            }
            if (_customContent != null)
            {
                Destroy(_customContent.gameObject);
            }
            _dynamicFieldsWithScrollbar.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets the title of a question and links the question to the UI.
        /// </summary>
        private void SetupQuestionDisplay()
        {
            _questionPlaceholder.GetComponent<QuestionMenuButtons>().AssociatedQuestion = _question;
            var title = "";//"Question: " + (showedQuestions);
            _questionPlaceholder
                .transform.Find("Panel")
                .Find("questionName")
                .GetComponent<Text>()
                .text = title;
            _questionContent
                .Find("questionText")
                .GetComponent<Text>()
                .text = _question.Text;

        }

        /// <summary>
        /// Updates the question display to show the new content.
        /// </summary>
        private void UpdateQuestionDisplay()
        {
            var menu = _questionPlaceholder.GetComponent<Menu.BaseMenu>();
            _menuManager.ShowMenu(menu);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_questionContent.GetComponent<RectTransform>());
        }
        
        /// <summary>
        /// Adds a label element to a question.
        /// </summary>
        /// <param name="labelType">Name of Prefab to be instantiated</param>
        /// <param name="labelText">Text on Label</param>
        /// <param name="parent">Container where to place label</param>
        /// <returns>Returns the new label</returns>
        private GameObject AddLabelText(string labelType, string labelText, Transform parent)
        {
            var label = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + labelType);
            MenuUtils.PlaceElement(label, parent);
            label.GetComponent<Text>().text = labelText;
            return label;
        }

        /// <summary>
        /// Adds a sub label element to a question.
        /// </summary>
        /// <param name="labelType">Name of Prefab to be instantiated</param>
        /// <param name="labelText">Text on sublabel</param>
        /// <param name="parent">Container where to place label</param>
        /// <param name="subLabelType">Container within the Prefab where the sublabel is found</param>
        /// <param name="subLabelLength">Length of sublabel</param>
        /// <returns></returns>
        private GameObject AddLabelText(string labelType, string labelText, Transform parent, string subLabelType, float subLabelLength)
        {
            var label = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + labelType);
            MenuUtils.PlaceElement(label, parent);
            var sublabel = label.transform.Find(subLabelType);

            sublabel.GetComponent<RectTransform>().sizeDelta = new Vector2(subLabelLength, 50);
            sublabel.GetComponent<Text>().text = labelText;
            return label;
        }
    

        /// <summary>
        /// Creates a toggle with a width and height.
        /// </summary>
        /// <param name="toggleType">Toggle type to be created.</param>
        /// <param name="parent">Container where to place toggle</param>
        /// <param name="width">Width of toggle area</param>
        /// <param name="height">Height of toggle area</param>
        /// <returns>Transform of created toggle</returns>
        private Transform CreateToggleElement(string toggleType, Transform parent, float width, float height)
        {
            var toggleObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + toggleType);
            toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            MenuUtils.PlaceElement(toggleObject, parent);
            var button = toggleObject.transform.Find("ToggleButtons");
            return button;
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
            _oldAnswers = _log.readAnswer(qOld.Name);
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
                _oldAnswers = _log.readAnswer(_question.Name);
                
                _previousNofAnswers.Push(_numberOfAnswers);
                _numberOfAnswers++;
            }
            else
            {
                _menuManager.DisplayErrorMessage("Please answer the question!");
            }

        }

        private static int CalculateLengthOfMessage(string message, Text tex)
        {
            var totalLength = 0;
            tex.font.RequestCharactersInTexture(message, tex.fontSize, tex.fontStyle);
            var myFont = tex.font; 

            var arr = message.ToCharArray();

            foreach (var c in arr)
            {
                CharacterInfo characterInfo;
                myFont.GetCharacterInfo(c, out characterInfo, tex.fontSize);
                totalLength += characterInfo.advance;
            }

            return totalLength;
        }


        private static int GetMaxTextLength(IEnumerable<string> labels)
        {
            var labelTextTmp = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/ToggleTopLabel");
            var textTmp = labelTextTmp.GetComponent<Text>();

            var maxLength = labels.Select(t => CalculateLengthOfMessage(t, textTmp)).Concat(new[] {0}).Max();

            Destroy(textTmp);
            Destroy(labelTextTmp);
            return maxLength;
        }

        private static Vector2 ComputeTopLabelSize(int nColumns, IList<string> labels)
        {
            // find longest substring to set top label width
            var labelTextTmp = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/ToggleTopLabel");
            var textTmp = labelTextTmp.GetComponent<Text>(); ;

            float minWidth = 150;
            float height = 0;
            for (var i = 0; i < nColumns; i++)
            {
                textTmp.text = labels[i];
                var fullLength = textTmp.preferredWidth;
                var substrings = labels[i].Split(' ');
                foreach (var t in substrings)
                {
                    textTmp.text = t;
                    if (textTmp.preferredWidth > minWidth)
                        minWidth = textTmp.preferredWidth;
                }
                var nlines = (int)(Math.Ceiling(fullLength / minWidth));
                if (nlines * 32 > height)
                    height = nlines * 32;

            }

            Destroy(textTmp);
            Destroy(labelTextTmp);
            return new Vector2(minWidth, height);
        }

        public void Visit(Question q)
        {
            throw new NotImplementedException();
        }

        public void Visit(InfoScreen q) { 
			var t = _questionContent.GetComponentInChildren<Text> ();
			t.alignment = TextAnchor.MiddleLeft;

			_dynamicFieldsWithScrollbar.gameObject.SetActive (false);

		}

        public void Visit(ChoiceQuestion q)
        {
            var dynamicFieldSize = _dynamicField.GetComponent<RectTransform>().sizeDelta;
            var nRows = q.NRows;
            var nColumns = q.NColumns;
            var answernumber = 0;

            var qmb = _questionPlaceholder.GetComponent<QuestionMenuButtons>();

            var multiColObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/ToggleMulticolLayoutEmpty");
            MenuUtils.PlaceElement(multiColObject, _dynamicField);
            var isMultiple = q.Choice == Choice.Multiple;
        
            if (nColumns > 1)
            {
                var cLabels = q.ColumnLabels.Select(l => l.Text).ToList();

                var topRow = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/TopRow");

                MenuUtils.PlaceElement(topRow, _questionContent);
                topRow.transform.SetSiblingIndex(1);

                var topLabelSize = ComputeTopLabelSize(nColumns, cLabels);
                
                for (var i = 0; i < nColumns; i++)
                {
                    //Set side labels
                    var filenameObj = AddLabelText("ToggleTopLabel", cLabels[i], topRow.transform);
                    filenameObj.GetComponent<RectTransform>().sizeDelta = topLabelSize;
                }

                var widthAllTopLabels = nColumns*topLabelSize.x;
                var sideLabelWidth = dynamicFieldSize.x - widthAllTopLabels;
                
                var leftPlaceholder = topRow.transform.Find("Placeholder");
                leftPlaceholder.GetComponent<RectTransform>().sizeDelta = new Vector2(sideLabelWidth, 0);

                for (var i = 0; i < nRows; i++)
                {
                    //Set side labels
                    var oneRow = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/ToggleOneRow");
                    var newParent = multiColObject.transform.Find("ResponseRows");
                    MenuUtils.PlaceElement(oneRow, newParent);

                    var text = q.RowLabels!=null?q.RowLabels[i].Text:"";
                    var filenameObj = AddLabelText("ToggleSidelabel", text, oneRow.transform);
                    var tex = filenameObj.GetComponent<Text>();

                    var count = Regex.Split(tex.text, "\n").Length - 1;
                    var labelLength = CalculateLengthOfMessage(tex.text, tex);
                    var rowHeight = (int) (Math.Ceiling(labelLength/sideLabelWidth) + count)*32;
                    filenameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sideLabelWidth, rowHeight);

                    for (var j = 0; j < nColumns; j++)
                    {
                        var button = isMultiple ? CreateToggleElement("ToggleLayoutObject", oneRow.transform, topLabelSize.x,
                            100) : CreateToggleElement("ToggleLayoutObjectMultipleChoice", oneRow.transform,
                            topLabelSize.x,
                            100);
                        var btn = button.GetComponent<ToggleExtended>();
                        if (!isMultiple) btn.group = oneRow.GetComponent<ToggleGroup>();


                        var iLocal = answernumber;
                        btn.onValueChanged.AddListener(isOn =>
                        {
                            qmb.SetAnswerInt(iLocal, isOn ? 1 : 0);
                        });
                        btn.isOn = _oldAnswers != null && _oldAnswers.ContainsKey(i);
                        answernumber++;
                    }
                }
            }
            else
            {
                var rLabels = q.RowLabels.Select(l => l.Text).ToList();
                float length = GetMaxTextLength(rLabels);
                var newParent = multiColObject.transform.Find("ResponseRows");
                for (var i = 0; i < nRows; i++)
                {
                    var hasText = q.RowLabels[i].Answerable.HasValue && q.RowLabels[i].Answerable.Value;
                
                    //Set side labels
                    GameObject oneRow;
                    if (q.RowLabels[i].Image != null)
                    {
                        oneRow = AddLabelText("ToggleOneRowImage", rLabels[i], newParent, "ToggleSidelabel", length);
                        oneRow.transform.Find("RawImage").GetComponent<RawImage>().texture = Resources.Load<Sprite>(q.RowLabels[i].Image).texture;
                    }
                    else
                    {
                        var labelType = hasText ? "ToggleOneRowText" : "ToggleOneRowNoText";
                        oneRow = AddLabelText(labelType, rLabels[i], newParent, "ToggleSidelabel", length);
                    }
                    MenuUtils.PlaceElement(oneRow, newParent);

                    var rightPlaceholderWidth = dynamicFieldSize.x - length;
                    var rightPlaceholder = oneRow.transform.Find("NoTextPlaceholder");
                    rightPlaceholder.GetComponent<RectTransform>().sizeDelta = new Vector2(rightPlaceholderWidth, 0);
                
                    var toggleObject = oneRow.transform.Find("ToggleLayoutObject").gameObject;
                    toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 50);

                    var btn = toggleObject.transform.Find("ToggleButtons").GetComponent<ToggleExtended>();
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
            var answernumber = 0;
            if (q.NRows == 1)
            {
                var textInputField = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/QuestionOnlyField");
                MenuUtils.PlaceElement(textInputField, _dynamicField);

                var inpt = textInputField.transform.Find("InputField").GetComponent<InputField>();
                inpt.onEndEdit.AddListener(answer =>
                {
                    qmb.SetAnswerString(0, answer);
                });
                if (_oldAnswers == null || !_oldAnswers.ContainsKey(0)) return;
                inpt.text = _oldAnswers[0];
            }
            else
            {
                var rLabels = q.RowLabels.Select(l => l.Text).ToList();
                float length = GetMaxTextLength(rLabels);
                for (var index = 0; index < q.RowLabels.Count; index++)
                {

                    var iLocal = index;

                    var labelAndField = AddLabelText("QuestionChoiceLabelAndField", q.RowLabels[index].Text, _dynamicField, "Label", length);

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
        }

        public void Visit(LadderQuestion q)
        {
            var ladder = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/LadderContent");
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

            var manikin = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/ManikinContent");
            _customContent = manikin.transform;
            MenuUtils.PlaceElement(manikin, _questionContent);

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
            _questionPlaceholder.transform
                    .Find("Panel")
                    .Find("QuestionControlButtons")
                    .Find("BackButton")
                        .gameObject
                        .SetActive(false);
            _questionPlaceholder.transform
                    .Find("Panel")
                    .Find("QuestionControlButtons")
                    .Find("NextButton")
                        .gameObject
                        .SetActive(false);
            _questionPlaceholder.transform
                    .Find("Panel")
                    .Find("questionName").gameObject
                    .SetActive(false);
            var test = _questionContent.GetComponentsInChildren<Transform>();
            foreach (var t in test)
            {
                t.gameObject.SetActive(false);
            }

            var rep =_questionContent.
                gameObject.
                AddComponent<Representations.VisualStimuli>();
            rep.Question = q;

            _questionContent.GetComponent<RectTransform>().sizeDelta = new Vector2(5760, 1080);
            _questionContent.GetComponent<RectTransform>().position = new Vector3(0, 0, 0);
            
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

            expositionScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(5760,1080);

            _questionContent.gameObject.SetActive(true);

            _questionContent.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            rep.InitialiseRepresentation(this);
        }
    }
}

