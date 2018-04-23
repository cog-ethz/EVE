using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ShowPopUpQuestionnaire : MonoBehaviour
{
    public string questionnaireName;
    public GameObject questionnaireCanvas;

    private QuestionnaireBuilder builder;
    private QuestionSet currentQuestionSet;

    private int totalQuestions;
    private int currentQuestion, showedQuestions;
    private Stack<int> previousQuestion;
    private int totalInfoScreens;
    private int numberOfAnswers;
    private Stack<int> previousNofAnswers;
    private Dictionary<int, string> oldAnswers;
    private int displayedquestion;
    private int questionSetNumber;
    private bool lastQuestionSet, once, active;
    private Dictionary<string, QuestionSet> questionnaire;
    private GameObject player;

    private Question q;
    private LoggingManager _log;
    private LaunchManager _launchManager;
    private MenuManager _menuManager;

    // Use this for initialization
    void Awake()
    {
        //creating for debugging reasons
        _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = _launchManager.GetLoggingManager();
        _menuManager = _launchManager.GetMenuManager();

        builder = new QuestionnaireBuilder(_log);       

        questionnaire = builder.LoadQuestionnaire(questionnaireName);

        var first = questionnaire.First();
        string key = first.Key;
        currentQuestionSet = first.Value;
        questionSetNumber = 0;

        //Load other
        totalQuestions = currentQuestionSet.getPageAmount();
        currentQuestion = 0;
        previousQuestion = new Stack<int>();
        previousNofAnswers = new Stack<int>();
        displayedquestion = -1;
        showedQuestions = 1;
    }

    void OnTriggerEnter(Collider other)
    {   
        if (other.tag == "Player")
        {
            player = other.gameObject;
            questionnaireCanvas.SetActive(true);
            player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
            displayedquestion = -1;
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            active = true;
        }
    }


    void OnGUI()
    {
       /* Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        throw new NotImplementedException();
        /*if (active)
        {
            if (currentQuestion >= 0 && currentQuestion < totalQuestions && displayedquestion != currentQuestion)
            {
                displayedquestion = currentQuestion;

                q = currentQuestionSet.getQuestion(currentQuestion);
                while (q == null && currentQuestion < totalQuestions)
                {
                    currentQuestion++;
                    q = currentQuestionSet.getQuestion(currentQuestion);
                }

                int questionTypeFiltered = q.GetQuestionType();

                if (q.GetQuestionType() == 9)
                {
                    questionTypeFiltered = 7;
                }

                switch (questionTypeFiltered)
                {
                    case 1: //TEXTANSWER:
                        string[] labelsT = q.GetLabels();
                        if (labelsT == null)
                        {
                            GameObject.Find("TextQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                            GameObject.Find("TextQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                            GameObject.Find("TextQuestion").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();

                            //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
                            Transform dynamicFieldTQ = GameObject.Find("TextQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsTQ");
                            List<GameObject> entriesObjectsT = new List<GameObject>();
                            foreach (Transform entry in dynamicFieldTQ) entriesObjectsT.Add(entry.gameObject);
                            foreach (GameObject entryObject in entriesObjectsT) Destroy(entryObject);


                            GameObject filenameObjT = Instantiate(Resources.Load("Prefabs/Menus/QuestionOnlyField")) as GameObject;
                            Transform dynamicFieldTQQ = GameObject.Find("TextQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsTQ");
                            filenameObjT.transform.SetParent(dynamicFieldTQQ);
                            filenameObjT.transform.localPosition = new Vector3(filenameObjT.transform.localPosition.x, filenameObjT.transform.localPosition.y, dynamicFieldTQQ.localPosition.z);
                            filenameObjT.transform.localScale = new Vector3(1, 1, 1);
                            //set answer number
                            //filenameObjT.transform.Find("ChoiceButton").GetComponent<ChoiceButtonScript>().setAnswerNumber(answernumber);
                            //answernumber++;
                            if (oldAnswers != null)
                                if (oldAnswers.ContainsKey(0))
                                {
                                    UnityEngine.UI.InputField inpt = filenameObjT.transform.Find("InputField").GetComponent<UnityEngine.UI.InputField>();
                                    inpt.text = oldAnswers[0];
                                }
                            oldAnswers = new Dictionary<int, string>();
                            GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("TextQuestion").GetComponent<Menu>());
                        }
                        else
                        {
                            GameObject.Find("TextQuestion (1)").GetComponent<QuestionStorage>().associateQuestionObject(q);
                            GameObject.Find("TextQuestion (1)").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                            GameObject.Find("TextQuestion (1)").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();

                            //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
                            Transform dynamicFieldTQ = GameObject.Find("TextQuestion (1)").GetComponent<Menu>().getDynamicFields("DynFieldsTQ");
                            List<GameObject> entriesObjectsT = new List<GameObject>();
                            foreach (Transform entry in dynamicFieldTQ) entriesObjectsT.Add(entry.gameObject);
                            foreach (GameObject entryObject in entriesObjectsT) Destroy(entryObject);

                            int answernumberT = 0;
                            foreach (string label in labelsT)
                            {
                                GameObject filenameObjT = Instantiate(Resources.Load("Prefabs/Menus/QuestionChoiceLabelAndField")) as GameObject;
                                Transform dynamicFieldTQQ = GameObject.Find("TextQuestion (1)").GetComponent<Menu>().getDynamicFields("DynFieldsTQ");
                                filenameObjT.transform.SetParent(dynamicFieldTQQ);
                                filenameObjT.transform.localPosition = new Vector3(filenameObjT.transform.localPosition.x, filenameObjT.transform.localPosition.y, dynamicFieldTQQ.localPosition.z);
                                filenameObjT.transform.localScale = new Vector3(1, 1, 1);

                                Transform te = filenameObjT.transform.Find("Label");
                                UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
                                tex.text = label;
                                //set answer number                           
                                if (oldAnswers != null)
                                    if (oldAnswers.ContainsKey(answernumberT))
                                    {
                                        UnityEngine.UI.InputField inpt = filenameObjT.transform.Find("InputField").GetComponent<UnityEngine.UI.InputField>();
                                        inpt.text = oldAnswers[answernumberT];
                                    }
                                answernumberT++;

                            }
                            oldAnswers = new Dictionary<int, string>();
                            GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("TextQuestion (1)").GetComponent<Menu>());
                        }
                        break;
                    case 2: //MULTIPLECHOICEANSWER:                        
                        GameObject.Find("MultipleChoiceQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                        GameObject.Find("MultipleChoiceQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                        GameObject.Find("MultipleChoiceQuestion").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();
                        //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
                        Transform dynamicFieldTMQ = GameObject.Find("MultipleChoiceQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsMCQ");
                        List<GameObject> entriesObjects = new List<GameObject>();
                        foreach (Transform entry in dynamicFieldTMQ) entriesObjects.Add(entry.gameObject);
                        foreach (GameObject entryObject in entriesObjects) Destroy(entryObject);

                        string[] labels = q.GetLabels();
                        int rows = q.GetRows();
                        int columns = q.GetColumns();

                        if (rows > 1)
                        {
                            GameObject multiColObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleMulticolLayoutEmpty")) as GameObject;
                            Transform dynamicFieldObj = GameObject.Find("MultipleChoiceQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsMCQ");
                            multiColObject.transform.SetParent(dynamicFieldObj);
                            multiColObject.transform.localPosition = new Vector3(multiColObject.transform.localPosition.x, multiColObject.transform.localPosition.y, dynamicFieldObj.localPosition.z);
                            multiColObject.transform.localScale = new Vector3(1, 1, 1);

                            int answernumber = 0;

                            for (int i = 0; i < columns; i++)
                            {
                                //Set side labels
                                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/ToggleTopLabel")) as GameObject;
                                Transform newParent = multiColObject.transform.Find("TopRow");
                                filenameObj.transform.SetParent(newParent);
                                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, newParent.localPosition.z);
                                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                                UnityEngine.UI.Text tex = filenameObj.GetComponent<UnityEngine.UI.Text>();
                                tex.text = labels[i];
                            }

                            for (int i = 0; i < rows; i++)
                            {
                                //Set side labels
                                GameObject oneRow = Instantiate(Resources.Load("Prefabs/Menus/ToggleOneRow")) as GameObject;
                                Transform newParent = multiColObject.transform.Find("ResponseRows");
                                oneRow.transform.SetParent(newParent);
                                oneRow.transform.localPosition = new Vector3(oneRow.transform.localPosition.x, oneRow.transform.localPosition.y, newParent.localPosition.z);
                                oneRow.transform.localScale = new Vector3(1, 1, 1);

                                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/ToggleSidelabel")) as GameObject;
                                filenameObj.transform.SetParent(oneRow.transform);
                                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, oneRow.transform.localPosition.z);
                                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                                UnityEngine.UI.Text tex = filenameObj.GetComponent<UnityEngine.UI.Text>();
                                tex.text = labels[columns + i];
                                for (int j = 0; j < columns; j++)
                                {
                                    GameObject toggleObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleLayoutObjectMultipleChoice")) as GameObject;
                                    toggleObject.transform.SetParent(oneRow.transform);
                                    toggleObject.transform.localPosition = new Vector3(toggleObject.transform.localPosition.x, toggleObject.transform.localPosition.y, oneRow.transform.localPosition.z);
                                    toggleObject.transform.localScale = new Vector3(1, 1, 1);
                                    Transform button = toggleObject.transform.Find("ToggleButtons");
                                    button.GetComponent<MultiChoiceButton>().setAnswerNumber(answernumber);
                                    if (oldAnswers != null)
                                        if (oldAnswers.ContainsKey(answernumber))
                                        {
                                            button.GetComponent<ToggleExtended>().isOn = true;
                                        }
                                    answernumber++;
                                }
                            }
                        }
                        else
                        {
                            GameObject multiColObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleMulticolLayoutEmpty")) as GameObject;
                            Transform dynamicFieldObj = GameObject.Find("MultipleChoiceQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsMCQ");
                            multiColObject.transform.SetParent(dynamicFieldObj);
                            multiColObject.transform.localPosition = new Vector3(multiColObject.transform.localPosition.x, multiColObject.transform.localPosition.y, dynamicFieldObj.localPosition.z);
                            multiColObject.transform.localScale = new Vector3(1, 1, 1);
                            multiColObject.transform.Find("TopRow").gameObject.SetActive(false);

                            int answernumber = 0;
                            for (int i = 0; i < columns; i++)
                            {
                                //Set side labels
                                GameObject oneRow = Instantiate(Resources.Load("Prefabs/Menus/ToggleOneRow")) as GameObject;
                                Transform newParent = multiColObject.transform.Find("ResponseRows");
                                oneRow.transform.SetParent(newParent);
                                oneRow.transform.localPosition = new Vector3(oneRow.transform.localPosition.x, oneRow.transform.localPosition.y, newParent.localPosition.z);
                                oneRow.transform.localScale = new Vector3(1, 1, 1);

                                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/ToggleSidelabel")) as GameObject;
                                filenameObj.transform.SetParent(oneRow.transform);
                                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, oneRow.transform.localPosition.z);
                                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                                filenameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);
                                UnityEngine.UI.Text tex = filenameObj.GetComponent<UnityEngine.UI.Text>();
                                tex.text = labels[i];
                                for (int j = 0; j < rows; j++)
                                {
                                    GameObject toggleObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleLayoutObjectMultipleChoice")) as GameObject;
                                    toggleObject.transform.SetParent(oneRow.transform);
                                    toggleObject.transform.localPosition = new Vector3(toggleObject.transform.localPosition.x, toggleObject.transform.localPosition.y, oneRow.transform.localPosition.z);
                                    toggleObject.transform.localScale = new Vector3(1, 1, 1);
                                    toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 50);
                                    Transform button = toggleObject.transform.Find("ToggleButtons");
                                    button.GetComponent<MultiChoiceButton>().setAnswerNumber(answernumber);
                                    if (oldAnswers != null)
                                        if (oldAnswers.ContainsKey(answernumber))
                                        {
                                            button.GetComponent<ToggleExtended>().isOn = true;
                                        }
                                    answernumber++;
                                }
                            }
                        }

                        oldAnswers = new Dictionary<int, string>();
                        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("MultipleChoiceQuestion").GetComponent<Menu>());
                        break;
                    case 3: //SINGLECHOICEANSWER:                   
                        GameObject.Find("SingleChoiceQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                        GameObject.Find("SingleChoiceQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                        GameObject.Find("SingleChoiceQuestion").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();
                        //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
                        Transform dynamicFieldT = GameObject.Find("SingleChoiceQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsSCQ");
                        List<GameObject> entriesObjectsSQ = new List<GameObject>();
                        foreach (Transform entry in dynamicFieldT) entriesObjectsSQ.Add(entry.gameObject);
                        foreach (GameObject entryObject in entriesObjectsSQ) Destroy(entryObject);

                        string[] labelsSQ = q.GetLabels();
                        int rowsSQ = q.GetRows();
                        int columnsSQ = q.GetColumns();

                        if (rowsSQ > 1)
                        {
                            GameObject multiColObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleMulticolLayoutEmpty")) as GameObject;
                            Transform dynamicFieldObj = GameObject.Find("SingleChoiceQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsSCQ");
                            multiColObject.transform.SetParent(dynamicFieldObj);
                            multiColObject.transform.localPosition = new Vector3(multiColObject.transform.localPosition.x, multiColObject.transform.localPosition.y, dynamicFieldObj.localPosition.z);
                            multiColObject.transform.localScale = new Vector3(1, 1, 1);

                            int answernumber = 0;

                            for (int i = 0; i < columnsSQ; i++)
                            {
                                //Set side labels
                                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/ToggleTopLabel")) as GameObject;
                                Transform newParent = multiColObject.transform.Find("TopRow");
                                filenameObj.transform.SetParent(newParent);
                                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, newParent.localPosition.z);
                                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                                UnityEngine.UI.Text tex = filenameObj.GetComponent<UnityEngine.UI.Text>();
                                tex.text = labelsSQ[i];
                            }

                            for (int i = 0; i < rowsSQ; i++)
                            {
                                //Set side labels
                                GameObject oneRow = Instantiate(Resources.Load("Prefabs/Menus/ToggleOneRow")) as GameObject;
                                UnityEngine.UI.ToggleGroup rowGroup = oneRow.GetComponent<UnityEngine.UI.ToggleGroup>();
                                Transform newParent = multiColObject.transform.Find("ResponseRows");
                                oneRow.transform.SetParent(newParent);
                                oneRow.transform.localPosition = new Vector3(oneRow.transform.localPosition.x, oneRow.transform.localPosition.y, newParent.localPosition.z);
                                oneRow.transform.localScale = new Vector3(1, 1, 1);

                                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/ToggleSidelabel")) as GameObject;
                                filenameObj.transform.SetParent(oneRow.transform);
                                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, oneRow.transform.localPosition.z);
                                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                                UnityEngine.UI.Text tex = filenameObj.GetComponent<UnityEngine.UI.Text>();
                                tex.text = labelsSQ[columnsSQ + i];
                                for (int j = 0; j < columnsSQ; j++)
                                {
                                    GameObject toggleObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleLayoutObject")) as GameObject;
                                    toggleObject.transform.SetParent(oneRow.transform);
                                    toggleObject.transform.localPosition = new Vector3(toggleObject.transform.localPosition.x, toggleObject.transform.localPosition.y, oneRow.transform.localPosition.z);
                                    toggleObject.transform.localScale = new Vector3(1, 1, 1);
                                    Transform button = toggleObject.transform.Find("ToggleButtons");
                                    button.GetComponent<ToggleExtended>().group = rowGroup;
                                    button.GetComponent<ChoiceButtonScript>().setAnswerNumber(answernumber);
                                    if (oldAnswers != null)
                                        if (oldAnswers.ContainsKey(answernumber))
                                        {
                                            button.GetComponent<ToggleExtended>().isOn = true;
                                        }
                                    answernumber++;
                                }
                            }
                        }
                        else
                        {
                            GameObject multiColObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleMulticolLayoutEmpty")) as GameObject;
                            Transform dynamicFieldObj = GameObject.Find("SingleChoiceQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsSCQ");
                            multiColObject.transform.SetParent(dynamicFieldObj);
                            multiColObject.transform.localPosition = new Vector3(multiColObject.transform.localPosition.x, multiColObject.transform.localPosition.y, dynamicFieldObj.localPosition.z);
                            multiColObject.transform.localScale = new Vector3(1, 1, 1);
                            multiColObject.transform.Find("TopRow").gameObject.SetActive(false);

                            int answernumber = 0;
                            for (int i = 0; i < columnsSQ; i++)
                            {
                                //Set side labels
                                GameObject oneRow = Instantiate(Resources.Load("Prefabs/Menus/ToggleOneRow")) as GameObject;
                                Transform newParent = multiColObject.transform.Find("ResponseRows");
                                UnityEngine.UI.ToggleGroup rowGroup = newParent.GetComponent<UnityEngine.UI.ToggleGroup>();
                                oneRow.transform.SetParent(newParent);
                                oneRow.transform.localPosition = new Vector3(oneRow.transform.localPosition.x, oneRow.transform.localPosition.y, newParent.localPosition.z);
                                oneRow.transform.localScale = new Vector3(1, 1, 1);

                                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/ToggleSidelabel")) as GameObject;
                                filenameObj.transform.SetParent(oneRow.transform);
                                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, oneRow.transform.localPosition.z);
                                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                                filenameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);
                                UnityEngine.UI.Text tex = filenameObj.GetComponent<UnityEngine.UI.Text>();
                                tex.text = labelsSQ[i];
                                for (int j = 0; j < rowsSQ; j++)
                                {
                                    GameObject toggleObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleLayoutObject")) as GameObject;
                                    toggleObject.transform.SetParent(oneRow.transform);
                                    toggleObject.transform.localPosition = new Vector3(toggleObject.transform.localPosition.x, toggleObject.transform.localPosition.y, oneRow.transform.localPosition.z);
                                    toggleObject.transform.localScale = new Vector3(1, 1, 1);
                                    toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 50);
                                    Transform button = toggleObject.transform.Find("ToggleButtons");
                                    button.GetComponent<ToggleExtended>().group = rowGroup;
                                    button.GetComponent<ChoiceButtonScript>().setAnswerNumber(answernumber);
                                    if (oldAnswers != null)
                                        if (oldAnswers.ContainsKey(answernumber))
                                        {
                                            button.GetComponent<ToggleExtended>().isOn = true;
                                        }
                                    answernumber++;
                                }
                            }
                        }

                        oldAnswers = new Dictionary<int, string>();
                        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("SingleChoiceQuestion").GetComponent<Menu>());
                        break;
                    case 4: //SLIDERANSWER:
                        Debug.Log("The case of question type SLIDERANSWER is not implemented on purpose. Please contact the developers in case you need it.");
                        break;
                    case 5: //MANIKINANSWER:
                        GameObject.Find("ManikinQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                        GameObject.Find("ManikinQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                        GameObject.Find("ManikinQuestion").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();
                        Transform image = GameObject.Find("ManikinQuestion").transform.Find("Panel").Find("QuestionContent").Find("Image");
                        string[] labelsMAQ = q.GetLabels();
                        if (labelsMAQ[0] == "SAD")
                        {
                            image.GetComponent<UnityEngine.UI.Image>().overrideSprite = Resources.Load<Sprite>("Textures/SAM-V-5");
                        }
                        else if (labelsMAQ[0] == "QUIET")
                        {
                            image.GetComponent<UnityEngine.UI.Image>().overrideSprite = Resources.Load<Sprite>("Textures/SAM-A-5");
                        }
                        else if (labelsMAQ[0] == "DEPENDENT")
                        {
                            image.GetComponent<UnityEngine.UI.Image>().overrideSprite = Resources.Load<Sprite>("Textures/SAM-D-5");
                        }
                        else
                        {
                            image.GetComponent<UnityEngine.UI.Image>().overrideSprite = Resources.Load<Sprite>("Textures/questionline");
                        }

                        //set label
                        GameObject.Find("ManikinQuestion").transform.Find("Panel").Find("QuestionContent").Find("extremesText").Find("extreme1").gameObject.GetComponent<UnityEngine.UI.Text>().text = labelsMAQ[0];
                        GameObject.Find("ManikinQuestion").transform.Find("Panel").Find("QuestionContent").Find("extremesText").Find("extreme2").gameObject.GetComponent<UnityEngine.UI.Text>().text = labelsMAQ[labelsMAQ.Length - 1];

                        Transform buttons = GameObject.Find("ManikinQuestion").transform.Find("Panel").Find("QuestionContent").Find("Buttons");

                        for (int i = 0; i < 9; i++)
                        {
                            Transform mainikinToggle = buttons.Find("ToggleButtons" + i);
                            UnityEngine.UI.Toggle btn = mainikinToggle.GetComponent<UnityEngine.UI.Toggle>();
                            mainikinToggle.GetComponent<ChoiceButtonManikinScript>().setAnswerNumber(-1);
                            btn.isOn = false;
                            mainikinToggle.GetComponent<ChoiceButtonManikinScript>().setAnswerNumber(i);

                            if (oldAnswers != null)
                                if (oldAnswers.ContainsKey(i))
                                {
                                    btn.isOn = true;
                                }
                        }
                        oldAnswers = new Dictionary<int, string>();
                        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("ManikinQuestion").GetComponent<Menu>());
                        break;
                    case 6: //LADDERANSWER:
                        GameObject.Find("LadderQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                        GameObject.Find("LadderQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                        GameObject.Find("LadderQuestion").transform.Find("Panel").Find("QuestionContent").Find("DynFieldsWithScrollbar").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();
                        string[] labelsLQ = q.GetLabels();
                        GameObject.Find("LadderQuestion").transform.Find("Panel").Find("LadderLabel").gameObject.GetComponent<UnityEngine.UI.Text>().text = labelsLQ[0];
                        Transform ladderButtons = GameObject.Find("LadderQuestion").transform.Find("Panel").Find("QuestionContent").Find("Buttons");

                        for (int i = 9; i >= 0; i--)
                        {
                            Transform ladderToggle = ladderButtons.Find("ToggleButtons" + i);
                            UnityEngine.UI.Toggle btn = ladderToggle.GetComponent<UnityEngine.UI.Toggle>();

                            ladderToggle.GetComponent<ChoiceLadderButtonScript>().setAnswerNumber(-1);
                            btn.isOn = false;
                            ladderToggle.GetComponent<ChoiceLadderButtonScript>().setAnswerNumber(i);

                            if (oldAnswers != null)
                                if (oldAnswers.ContainsKey(i))
                                {
                                    btn.isOn = true;
                                }
                        }
                        oldAnswers = new Dictionary<int, string>();
                        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("LadderQuestion").GetComponent<Menu>());
                        break;
                    case 7: //SINGLECHOICETEXTANSWER:                        
                        GameObject.Find("SingleChoiceTextQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                        GameObject.Find("SingleChoiceTextQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "Question: " + (showedQuestions);
                        GameObject.Find("SingleChoiceTextQuestion").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();
                        //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
                        Transform dynamicFieldTSCTQ = GameObject.Find("SingleChoiceTextQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsSCTQ");
                        List<GameObject> entriesObjectsSCTQ = new List<GameObject>();
                        foreach (Transform entry in dynamicFieldTSCTQ) entriesObjectsSCTQ.Add(entry.gameObject);
                        foreach (GameObject entryObject in entriesObjectsSCTQ) Destroy(entryObject);
                        string[] labelsSCTQ = q.GetLabels();

                        int rowsSCTQ = q.GetRows();
                        int columnsSCTQ = q.GetColumns();

                        int answernumberSCTQ = 0;

                        if (columnsSCTQ == 1)
                        {

                            GameObject multiColObject = Instantiate(Resources.Load("Prefabs/Menus/ToggleMulticolLayoutEmpty")) as GameObject;
                            Transform dynamicFieldObj = GameObject.Find("SingleChoiceTextQuestion").GetComponent<Menu>().getDynamicFields("DynFieldsSCTQ");
                            multiColObject.transform.SetParent(dynamicFieldObj);
                            multiColObject.transform.localPosition = new Vector3(multiColObject.transform.localPosition.x, multiColObject.transform.localPosition.y, dynamicFieldObj.localPosition.z);
                            multiColObject.transform.localScale = new Vector3(1, 1, 1);
                            multiColObject.transform.Find("TopRow").gameObject.SetActive(false);

                            int[] vals = q.GetVals();

                            for (int i = 0; i < rowsSCTQ; i++)
                            {
                                //Set side labels
                                if (vals[2 + i] != 0)
                                {
                                    GameObject oneRow = Instantiate(Resources.Load("Prefabs/Menus/ToggleOneRowText")) as GameObject;
                                    Transform newParent = multiColObject.transform.Find("ResponseRows");
                                    UnityEngine.UI.ToggleGroup rowGroup = newParent.GetComponent<UnityEngine.UI.ToggleGroup>();
                                    oneRow.transform.SetParent(newParent);
                                    oneRow.transform.localPosition = new Vector3(oneRow.transform.localPosition.x, oneRow.transform.localPosition.y, newParent.localPosition.z);
                                    oneRow.transform.localScale = new Vector3(1, 1, 1);

                                    GameObject filenameObjSCTQ = oneRow.transform.Find("ToggleSidelabel").gameObject;
                                    filenameObjSCTQ.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);
                                    UnityEngine.UI.Text texSCTQ = filenameObjSCTQ.GetComponent<UnityEngine.UI.Text>();
                                    texSCTQ.text = labelsSCTQ[i];

                                    GameObject toggleObjectSCTQ = oneRow.transform.Find("ToggleLayoutObject").gameObject;
                                    toggleObjectSCTQ.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 50);

                                    Transform button = toggleObjectSCTQ.transform.Find("ToggleButtons");
                                    button.GetComponent<ToggleExtended>().group = rowGroup;

                                    button.GetComponent<ChoiceButtonText>().setAnswerNumber(answernumberSCTQ);
                                    if (oldAnswers != null)
                                        if (oldAnswers.ContainsKey(answernumberSCTQ))
                                        {
                                            button.GetComponent<ToggleExtended>().isOn = true;
                                            UnityEngine.UI.InputField inpt = oneRow.transform.Find("InputFieldSCTQ").GetComponent<UnityEngine.UI.InputField>();
                                            inpt.text = oldAnswers[answernumberSCTQ];
                                        }
                                    answernumberSCTQ++;
                                }
                                else
                                {
                                    GameObject oneRow = Instantiate(Resources.Load("Prefabs/Menus/ToggleOneRowNoText")) as GameObject;
                                    Transform newParent = multiColObject.transform.Find("ResponseRows");
                                    UnityEngine.UI.ToggleGroup rowGroup = newParent.GetComponent<UnityEngine.UI.ToggleGroup>();
                                    oneRow.transform.SetParent(newParent);
                                    oneRow.transform.localPosition = new Vector3(oneRow.transform.localPosition.x, oneRow.transform.localPosition.y, newParent.localPosition.z);
                                    oneRow.transform.localScale = new Vector3(1, 1, 1);

                                    GameObject filenameObjSCTQ = oneRow.transform.Find("ToggleSidelabel").gameObject;
                                    filenameObjSCTQ.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);
                                    UnityEngine.UI.Text texSCTQ = filenameObjSCTQ.GetComponent<UnityEngine.UI.Text>();
                                    texSCTQ.text = labelsSCTQ[i];

                                    GameObject toggleObjectSCTQ = oneRow.transform.Find("ToggleLayoutObject").gameObject;
                                    toggleObjectSCTQ.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 50);

                                    Transform button = toggleObjectSCTQ.transform.Find("ToggleButtons");
                                    button.GetComponent<ToggleExtended>().group = rowGroup;

                                    button.GetComponent<ChoiceButtonTextScript>().setAnswerNumber(answernumberSCTQ);
                                    if (oldAnswers != null)
                                        if (oldAnswers.ContainsKey(answernumberSCTQ))
                                        {
                                            button.GetComponent<ToggleExtended>().isOn = true;
                                        }
                                    answernumberSCTQ++;
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Multi column text questions are not implemented");
                            throw new NotImplementedException();
                        }
                        oldAnswers = new Dictionary<int, string>();
                        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("SingleChoiceTextQuestion").GetComponent<Menu>());
                        break;
                    case 8: //INFOSCREEN:
                        GameObject.Find("InfoScreenQuestion").GetComponent<QuestionStorage>().associateQuestionObject(q);
                        GameObject.Find("InfoScreenQuestion").transform.Find("Panel").Find("questionName").GetComponent<UnityEngine.UI.Text>().text = "";
                        GameObject.Find("InfoScreenQuestion").transform.Find("Panel").Find("QuestionContent").Find("questionText").GetComponent<UnityEngine.UI.Text>().text = q.GetQuestionText();
                        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("InfoScreenQuestion").GetComponent<Menu>());
                        break;
                }
                if (showedQuestions == 1)
                {
                    GameObject.Find("Canvas").GetComponent<MenuManager>().CurrentMenu.getDynamicFields("BackButton").gameObject.GetComponent<UnityEngine.UI.Button>().interactable = false;
                }
                else
                {
                    GameObject.Find("Canvas").GetComponent<MenuManager>().CurrentMenu.getDynamicFields("BackButton").gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
                }
            }
            else if (currentQuestion == totalQuestions)
            {
                if (currentQuestionSet == questionnaire.Values.Last())
                {
                    Debug.Log("Questionnaire Done, go to next scene");
                    if (!once)
                    {
                        once = true;
                        questionnaireCanvas.SetActive(false);
                        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
                        Cursor.lockState = UnityEngine.CursorLockMode.Locked;
                        Cursor.visible = false;
                        active = false;
                    }

                }
                else
                {
                    questionSetNumber++;
                    var next = questionnaire.ElementAt(questionSetNumber);

                    string key = next.Key;
                    currentQuestionSet = next.Value;
                    currentQuestion = 0;
                    totalQuestions = currentQuestionSet.getPageAmount();
                }
            }
        }*/
    }

    public void goBackOneQuestion()
    {
        if (currentQuestion > 0)
        {
            currentQuestion = previousQuestion.Pop();
            Question q_old = currentQuestionSet.getQuestion(currentQuestion);
            oldAnswers = _log.readAnswer(q_old.GetQuestionName());
            showedQuestions--;
            if (!q_old.GetType().Equals(typeof(InfoScreenQuestion)))
            {
                numberOfAnswers = previousNofAnswers.Pop();
            }
        }
        else if (questionSetNumber > 0)
        {
            questionSetNumber--;
            var next = questionnaire.ElementAt(questionSetNumber);

            string key = next.Key;
            currentQuestionSet = next.Value;

            totalQuestions = currentQuestionSet.getPageAmount();

            currentQuestion = previousQuestion.Pop();
            Question q_old = currentQuestionSet.getQuestion(currentQuestion);
            oldAnswers = _log.readAnswer(q_old.GetQuestionName());
            showedQuestions--;
            if (!q_old.GetType().Equals(typeof(InfoScreenQuestion)))
            {
                numberOfAnswers = previousNofAnswers.Pop();
            }
        }
    }

    public void nextQuestion()
    {
        //make sure that the question-window gave the information about the answers to the questionObject before saving answers!!!
        if (q.AnswerGiven())
        {
            q.SaveAnswer();
            previousQuestion.Push(currentQuestion);
            showedQuestions++;

            int jumpSize = currentQuestionSet.getJumpSize(currentQuestion);
            if (jumpSize == 0)
            {
                currentQuestion = totalQuestions;
            }
            else
            {
                currentQuestion += currentQuestionSet.getJumpSize(currentQuestion);
            }

            if (currentQuestion < totalQuestions)
            {
                q = currentQuestionSet.getQuestion(currentQuestion);
                oldAnswers = _log.readAnswer(q.GetQuestionName());

                if (!q.GetType().Equals(typeof(InfoScreenQuestion)))
                {
                    previousNofAnswers.Push(numberOfAnswers);
                    numberOfAnswers++;
                }
            }
        }
        else
        {
            _menuManager.DisplayErrorMessage("Please answer the question!");
        }

    }



}
