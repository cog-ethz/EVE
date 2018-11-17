using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Assets.EVE.Scripts.Menu;
using Assets.EVE.Scripts.XML;
using UnityEngine.SceneManagement;
using Assets.EVE.Scripts.Questionnaire.Questions;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Questionnaire.Enums;
using VisualStimuliEnums = Assets.EVE.Scripts.Questionnaire.Enums.VisualStimuli;
using Assets.EVE.Scripts.Questionnaire.XMLHelper;
using UnityEngine.UI;

public class LaunchManager : MonoBehaviour
{
    public ExperimentSettings ExperimentSettings;
    public GameObject FPC;
    public GameObject MenuCanvas;

    //public Boolean debug = false;

    private LoggingManager _log;
    private MenuManager _menuManager;
    private string _subjectId = "";
    private int _sessionId, _replaySessionId;
   
    //private List<String> sceneList;
    private System.Timers.Timer _delayTimer;
    private int currentScene = 0;
    private string nextScene, activeSceneName;
    private bool loadScene = false, labchartReady, initialized;

    private string filePathRoot = "";// participant files

    private Dictionary<string,string> _sessionParameters;


    public Dictionary<string, string> SessionParameters
    {
        get
        {
            return _sessionParameters;
        }
    }

    private string[] SCENESEXTENSIONS;
    private string[] SCENESQUESTIONNAIREREALNAMES;
    private string QuestionnaireName;

    // Make objects of this class a singleton
    private static LaunchManager _instance;

    void Awake()
    {
        if (_instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(FPC);
            DontDestroyOnLoad(MenuCanvas); 

            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        ReadExperimentSettings();

        //THIS SECTION CAN TEST WRITING EXPERIMENT SETTINGS
        //ExperimentSettings.UISettings.ReferenceResolution = new Vector2(1920,1080);
        //var path = UnityEditor.EditorUtility.SaveFilePanel("Save Experiment Settings", "", "experiment_settings", "xml");
        //WriteExperimentSettings(path);


        filePathRoot = Application.persistentDataPath + "/participant_files/"; ;
        DirectoryInfo dirInf = new DirectoryInfo(filePathRoot);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory for participant data"); dirInf.Create();
            Debug.Log("Data stored at " + filePathRoot);
            dirInf.Create();
        }
        _log = new LoggingManager();
        _log.ConnectToServer(ExperimentSettings.DatabaseSettings);
        initialized = false;
        _menuManager = MenuCanvas.GetComponent<MenuManager>();
        _sessionParameters = new Dictionary<string, string>();
        LoadSettingsIntoDB();

        var _canvas = GameObject.Find("Canvas");

        _canvas.GetComponent<CanvasScaler>().referenceResolution = ExperimentSettings.UISettings.ReferenceResolution;

        /*var qs = new QuestionSet("TestSet");
        qs.Questions.Add(new InfoScreen("example_info", "People questionnaire:Perception of people in the neighborhood"));
        qs.Questions.Add(new TextQuestion("exercise", "What type of exercise have you done recently?"));
        qs.Questions.Add(new TextQuestion("born_in", "Where were you born?", new List<Label>() { new Label("Country:"), new Label("City:") }));
        qs.Questions.Add(new ChoiceQuestion("yes_no", "Is this a Yes/No Question?", Choice.Single,new List<Label>() { new Label("Yes",1), new Label("No",0) },null));
        qs.Questions.Add(new ChoiceQuestion("multiple_yes_no", "Answer quickly without thinking:", Choice.Single, new List<Label>() { new Label("Do you like questionnaires?"), new Label("Have you ever been to New York?"), new Label("Coffee with Milk?"), new Label("Coffee with Sugar?"), new Label("Black Coffee?") }, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }));
        qs.Questions.Add(new ChoiceQuestion("coffe_consumption", "Did you have coffee, espresso, or another beverage containing caffeine in the past 24 hours?", Choice.Single, new List<Label>() { new Label("If yes, how many hours ago", 1,true), new Label("No, I did not have any caffeine", 0) },null));
        qs.Questions.Add(new ChoiceQuestion("band_judgement", "Please check with which assessment you agree most about the bands:", Choice.Multiple, new List<Label>() { new Label("Coldplay"), new Label("Maroon5"), new Label("Nsync"), new Label("Red Hot Chili Peppers")}, new List<Label>() { new Label("Mainstream", 0), new Label("Best of their Genre", 1), new Label("Original",2)}));
        qs.Questions.Add(new ChoiceQuestion("likert_scale", "Is this a good question?", Choice.Single, null,new List<Label>() { new Label("Not at all", 0), new Label("", 1), new Label("", 2), new Label("It is okay", 3), new Label("", 4), new Label("", 5), new Label("Very Good", 6) }));
        qs.Questions.Add(new ChoiceQuestion("multi_c_text", "Which animals do you like:", Choice.Multiple, new List<Label>() { new Label("Bears", 0), new Label("Lions", 1), new Label("Frogs", 2), new Label("Axolotls", 3), new Label("Humans", 4), new Label("Bats", 5), new Label("None", 6), new Label("Other:", 7,true) }, null));
        qs.Questions.Add(new ChoiceQuestion("image_example", "Which image looks the best?", Choice.Single, new List<Label>() { new Label("This", 0, "Images/test_image"), new Label("This", 1, "Images/test_image"), new Label("This", 2, "Images/test_image") }, null));
        qs.Questions.Add(new ScaleQuestion("amuesment_scale", "How much did you feel AMUSEMENT?",null, Scale.Line, "Did not experience at all", "Strongest experience ever felt"));
        qs.Questions.Add(new ScaleQuestion("SAM_pleasure", "Please rate how happy-unhappy you actually felt", null, Scale.Pleasure, "SAD", "CHEERFUL"));
        qs.Questions.Add(new ScaleQuestion("SAM_arousal", "Please rate how excited - calm you actually felt", null, Scale.Arousal, "QUIET", "ACTIVE"));
        qs.Questions.Add(new ScaleQuestion("SAM_dominance", "Please rate how controlled vs. in-control you actually felt", null, Scale.Dominance, "DEPENDENT", "INDEPENDENT"));
        qs.Questions.Add(new ScaleQuestion("custom_amuesment_scale", "How much did you feel AMUSEMENT?", "Textures/questionline", Scale.Custom, "Did not experience at all", "Strongest experience ever felt"));
        qs.Questions.Add(new LadderQuestion("swiss_status", "Swiss comparison of social status", "At the TOP of the ladder are the people who are the best off. The lower you are, the closer you are to the people at the very bottom. Where would you place yourself on this ladder, compared to all the other people in Switzerland?"));
        qs.Questions.Add(new ChoiceQuestion("flu_med", "Have you taken any medication for cold or flu symptoms today?", Choice.Single, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }, null, new List<Jump>() { new Jump("inflam_med", "FT") }));
        qs.Questions.Add(new TextQuestion("flu_med_name", "Which medication(s)?"));
        qs.Questions.Add(new ChoiceQuestion("inflam_med", "Have you taken any anti-inflammatory medication today?", Choice.Single, new List<Label>() { new Label("Yes", 1), new Label("No", 0) }, null, new List<Jump>() { new Jump("*", "FT") }));
        qs.Questions.Add(new TextQuestion("inflam_med_name", "Which medication(s)?"));
        qs.Questions.Add(new VisualStimuli("test_stimuli", 
            "Which stimulus is more pleasing?\n\nPress \"1\" for the first or \"2\" for the second.", 
            VisualStimuliEnums.Separator.FixationCross, 
            VisualStimuliEnums.Choice.ArrowKeys, 
            VisualStimuliEnums.Randomisation.ExperimentParameter,
            VisualStimuliEnums.Type.Image, "test_stimuli_order", false, 20, 10, 30,
            new List<string>
            {
                "Images/test_image", "Images/test_image"
            }));

        

        
        var qn = new Questionnaire("ExampleQuestionnaire");


        qn.QuestionSets.Add(qs.Name);

        var qf = new QuestionnaireFactory(_log,ExperimentSettings);
        qf.WriteQuestionSetToXml(qs, "TestSet.xml");
        qf.WriteQuestionnaireToXml(qn, "ExampleQuestionnaire");*/
    }


    // -----------------------------------------
    //			 During one Playthrough
    //------------------------------------------
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string previousSceneName = activeSceneName;
        activeSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Scene N " + currentScene);
        if (!initialized)
        {
            initialized = true;
            return;
        }
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        if (activeSceneName == "Loader" && previousSceneName != "Evaluation" && sceneList != null)
        { //coming back from a scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            _log.LogSceneEnd(sceneList[currentScene]);
            this.enabled = true;

            if (currentScene < sceneList.Count - 1)
            {
                currentScene++;
                loadCurrentScene();
            }
            else
            {
                _menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
                _menuManager.ShowMenu(GameObject.Find("Finished").GetComponent<BaseMenu>());
            }
        }
        else if (activeSceneName == "Loader" && previousSceneName == "Evaluation")
        {
            _menuManager.ShowMenu(GameObject.Find("Evaluation Menu").GetComponent<BaseMenu>());
        }
        else if (activeSceneName == "Evaluation" && previousSceneName != "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            this.enabled = true;
            _menuManager.setDetailsInt(_replaySessionId);
            _menuManager.ShowMenu(GameObject.Find("Evaluation Details").GetComponent<BaseMenu>());
        }
        else if (activeSceneName == "Evaluation")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
        }
        else if (activeSceneName == "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
        }
        else
        {          
            if (!FPC.GetComponentInChildren<ReplayRoute>().isActivated())
                _log.LogSceneStart(sceneList[currentScene]);
            this.enabled = false;   
        }
    }


    // -----------------------------------------
    //			 Initialization
    //------------------------------------------	
    void Start()
    {         
        _sessionId = _log.GetCurrentSessionID();
        if (_sessionId < 0)
        {
            var databaseSetupMenu = GameObject.Find("SetupDatabaseMenu").GetComponent<BaseMenu>();
            _menuManager.DisplayErrorMessage("Unable to connect to the database! Press ok to check the database status", databaseSetupMenu);
        }
        else
        {
            LoadSettingsIntoDB();
        }
    }


    /// <summary>
    /// The function resets the state of the participant for the next round
    /// </summary>
    public void setCompletedAndReset() {
        currentScene = 0;
        _log.updateParameters();
        _sessionId = _log.GetCurrentSessionID();
    }

    public void SetMenuManager(MenuManager menuManager)
    {
        _menuManager = menuManager;
    }
    
    // -----------------------------------------
    //			 Scene Loading
    //------------------------------------------	
    void Update()
    {
        //this is done in Update s.t. the loading screen has time to display
        if (loadScene)
        {
            loadScene = false;
            
            SceneManager.LoadScene(nextScene);
        }
        else if (_menuManager != null)
        {
            if (_menuManager.CheckBackEvalScene())
            {
                SceneManager.LoadScene("Evaluation");
            }

            if (_menuManager.CheckBackLoaderScene())
            {
                SceneManager.LoadScene("Loader");
            }
        }


    }

    public void loadCurrentScene()
    {
        SynchroniseSceneListWithDB();
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        LoadScene(sceneList[currentScene].Contains(".xml") ? "Questionnaire" : sceneList[currentScene]);
    }
    private void LoadScene(string scene)
    {
        var scenes = ExperimentSettings.SceneSettings.Scenes;
        if (scenes[currentScene].Contains(".xml"))
        {
            var splittedFileName = scenes[currentScene].Split('.');
            QuestionnaireName = splittedFileName[0];

            _log.CreateUserAnswer(_log.GetCurrentSessionID(), splittedFileName[0]);
            _log.setQuestionnaireName(splittedFileName[0]);
        }        
        nextScene = scene;
        loadScene = true;
    }
  
   public string getSubjectID()
    {
        return _subjectId;
    }
    
    /// <summary>
    /// Start experiment and checks errors.
    /// </summary>
    /// <remarks>
    /// The part with the logsessionparameters is from the old implementation and is not suited for everchanging sensors
    /// </remarks>
    public void StartExperiment() {
        
        _subjectId = _menuManager.subjectId;

        if (_subjectId.Length < 1)
        {
            BaseMenu originBaseMenu = GameObject.Find("Experiment Menu").GetComponent<BaseMenu>();
            _menuManager.DisplayErrorMessage("The Subject ID is invalid!", originBaseMenu);
        }
        else
        {
            SynchroniseSceneListWithDB();

            var sceneList = ExperimentSettings.SceneSettings.Scenes;
            Console.Write(sceneList.Count);
            int nParameters = _menuManager.GetAttributesList().Count;
            if (sceneList.Count <= 0)
            {
                BaseMenu originBaseMenu = GameObject.Find("Scene Config").GetComponent<BaseMenu>();
                _menuManager.DisplayErrorMessage("No scenes selected!", originBaseMenu);
            }
            else
            {
                if (_sessionParameters.Count != nParameters)
                {

                    BaseMenu originBaseMenu = GameObject.Find("Attribute Form").GetComponent<BaseMenu>();
                    _menuManager.DisplayErrorMessage("Session parameters are not set.",originBaseMenu);
                }
                else
                {
                    _log.LogSession(ExperimentSettings.Name, _subjectId);
                    if (_sessionParameters.ContainsKey("Labchart File Name"))
                        _log.SetLabChartFileName(_sessionParameters["Labchart File Name"]);

                    if (_log.getSensors().Contains("HL7Server"))
                        this.gameObject.GetComponent<HL7ServerStarter>().enabled = true;
                    storeSessionParameters();
                    BaseMenu startBaseMenu = GameObject.Find("Start Menu").GetComponent<BaseMenu>();
                    _menuManager.ShowMenu(startBaseMenu);
                }
            }
        }

    }

    private void storeSessionParameters()
    {
        foreach (KeyValuePair<string, string> entry in _sessionParameters)
        {
            _log.LogSessionParameter(entry.Key, entry.Value);
        }
    }

    public int getCurrentSessionID() {
        return _sessionId;
    }

    public void setSessionId(int id)
    {
        _sessionId = id;
    }
    


    public string GetQuestionnaireName() {
        return QuestionnaireName;
    }


    public void SetExperimentName(string name)
    {
        ExperimentSettings.Name = name;
    }

    public string GetExperimentName()
    {
        return ExperimentSettings.Name;
    }

    public LoggingManager GetLoggingManager()
    {
        return _log;
    }

    public MenuManager GetMenuManager()
    {
        return _menuManager;
    }

    public Dictionary<string,string> getSessionParameters()
    {
        return _sessionParameters;
    }

    public void changeSessionsParameter(string name, string value)
    {
        if (_sessionParameters.ContainsKey(name))
            _sessionParameters.Remove(name);
        _sessionParameters.Add(name, value);
    }

    public int getReplaySessionId()
    {
        return _replaySessionId;
    }

    public void setReplaySessionId(int id)
    {
        _replaySessionId = id;
    }

    /// <summary>
    /// Writes the Experiment Settings into an XML at the
    /// specified location.
    /// </summary>
    /// <remarks>
    /// This will overwrite previous settings.
    /// </remarks>
    /// <param name="folderPath"></param>
    public void WriteExperimentSettings(string folderPath)
    {
        if (!folderPath.EndsWith(".xml"))
            if (!folderPath.EndsWith("/")) folderPath = folderPath+ "/experiment_settings.xml";
        var xmlSerializer = new XmlSerializer(typeof(ExperimentSettings));
        using (var stream = new FileStream(folderPath, FileMode.Create))
        {
            xmlSerializer.Serialize(stream, ExperimentSettings);
        }
        Debug.Log("Wrote settings to " + folderPath);
    }

    /// <summary>
    /// Loads the experiment settings from the Resources folder.
    /// </summary>
    public void ReadExperimentSettings()
    {
        TextAsset ta = Resources.Load<TextAsset>("experiment_settings");
        var xmlSerializer = new XmlSerializer(typeof(ExperimentSettings));
        using (var stream = new StringReader(ta.text))
        {
            ExperimentSettings = (ExperimentSettings)xmlSerializer.Deserialize(stream);
        }
    }

    

    public void LoadSettingsIntoDB()
    {
        var name = ExperimentSettings.Name;
        _log.LogExperiment(name);
        _log.RemoveExperimentSceneOrder(name);
        foreach (var scene in ExperimentSettings.SceneSettings.Scenes)
        {
            _log.AddScene(scene);
        }
        _log.SetExperimentSceneOrder(name, ExperimentSettings.SceneSettings.Scenes.ToArray());
        UpdateParameters();
        foreach (var sensor in ExperimentSettings.SensorSettings.Sensors)
        {
            _log.AddSensor(sensor);
        }
    }

    public void SynchroniseSceneListWithDB()
    {
        if (_log.GetCurrentSessionID() > -1)
            ExperimentSettings.SceneSettings.Scenes = new List<string>(_log.getSceneNamesInOrder(ExperimentSettings.Name));
    }

    public void SynchroniseSensorListWithDB()
    {
        if (_log.GetCurrentSessionID() > -1)
        {
            var sensors = new List<string>(_log.getSensors());
            if (sensors.Contains("Labchart"))
            {
                ExperimentSettings.SensorSettings.Labchart = true;
                sensors.Remove("Labchart");
            }
            if (sensors.Contains("HL7Server"))
            {
                ExperimentSettings.SensorSettings.H7Server = true;
                sensors.Remove("HL7Server");
            }
            ExperimentSettings.SensorSettings.Sensors = sensors;
        }
    }


    /// <summary>
    /// This method allows to establish a correspondence between the parameters
    /// described in the database and in the experiment settings. 
    /// 
    /// This should be called whenever manually experiment parameters have been
    /// added within the EVE UI or within the Experiment Settings XML file.
    /// </summary>
    private void UpdateParameters()
    {
        var existingParams = _log.GetExperimentParameters(ExperimentSettings.Name);
        var requiredParams = ExperimentSettings.ParameterSettings.Parameters;
        for (var i = 0; i < requiredParams.Count; i++)
        {
            if (!existingParams.Contains(requiredParams[i]))
            {
                _log.CreateExperimentParameter(ExperimentSettings.Name, requiredParams[i]);
            }
        }
        for (var i = 0; i < existingParams.Count; i++)
        {
            if (!requiredParams.Contains(existingParams[i]))
            {
                requiredParams.Add(existingParams[i]);
            }
        }
        _menuManager.SetActiveParameters(requiredParams);
    }
}

