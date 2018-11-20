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
    
    private LoggingManager _log;
    private MenuManager _menuManager;

    private int _replaySessionId, _currentScene;
    private bool _loadScene, _initialized;
    private string _nextScene, _activeSceneName, _questionnaireName, _participantId, _filePathParticipants;

    public Dictionary<string, string> SessionParameters { get; private set; }

    /// <summary>
    /// Make the LaunchManager a singleton.
    /// </summary>
    /// 
    private static LaunchManager _instance;

    public int SessionId { get; set; }

    public int GetCurrentSessionId()
    {
        return SessionId;
    }
    
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


        _filePathParticipants = Application.persistentDataPath + "/participant_files/"; ;
        var dirInf = new DirectoryInfo(_filePathParticipants);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory for participant data"); dirInf.Create();
            Debug.Log("Data stored at " + _filePathParticipants);
            dirInf.Create();
        }
        _log = new LoggingManager();
        _log.ConnectToServer(ExperimentSettings.DatabaseSettings);
        _initialized = false;
        _menuManager = MenuCanvas.GetComponent<MenuManager>();
        SessionParameters = new Dictionary<string, string>();
        LoadSettingsIntoDB();

        var _canvas = GameObject.Find("Canvas");

        _canvas.GetComponent<CanvasScaler>().referenceResolution = ExperimentSettings.UISettings.ReferenceResolution;


        //THIS SECTION CAN TEST WRITING QUESTION SETS AND QUESTIONNAIRES
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
        var previousSceneName = _activeSceneName;
        _activeSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Scene N " + _currentScene);
        if (!_initialized)
        {
            _initialized = true;
            return;
        }
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        if (_activeSceneName == "Loader" && previousSceneName != "Evaluation" && sceneList != null)
        { //coming back from a scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            _log.LogSceneEnd(sceneList[_currentScene]);
            this.enabled = true;

            if (_currentScene < sceneList.Count - 1)
            {
                _currentScene++;
                LoadCurrentScene();
            }
            else
            {
                _menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
                _menuManager.ShowMenu(GameObject.Find("Finished").GetComponent<BaseMenu>());
            }
        }
        else if (_activeSceneName == "Loader" && previousSceneName == "Evaluation")
        {
            _menuManager.ShowMenu(GameObject.Find("Evaluation Menu").GetComponent<BaseMenu>());
        }
        else if (_activeSceneName == "Evaluation" && previousSceneName != "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            this.enabled = true;
            _menuManager.ActiveSessionId = _replaySessionId;
            _menuManager.ShowMenu(GameObject.Find("Evaluation Details").GetComponent<BaseMenu>());
        }
        else if (_activeSceneName == "Evaluation")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
        }
        else if (_activeSceneName == "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
        }
        else
        {          
            if (!FPC.GetComponentInChildren<ReplayRoute>().isActivated())
                _log.LogSceneStart(sceneList[_currentScene]);
            this.enabled = false;   
        }
    }


    // -----------------------------------------
    //			 Initialization
    //------------------------------------------	
    void Start()
    {         
        SessionId = _log.GetCurrentSessionID();
        if (SessionId < 0)
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
    public void SetCompletedAndReset() {
        _currentScene = 0;
        _log.updateParameters();
        SessionId = _log.GetCurrentSessionID();
    }

    // -----------------------------------------
    //			 Scene Loading
    //------------------------------------------	
    void Update()
    {
        //this is done in Update s.t. the loading screen has time to display
        if (_loadScene)
        {
            _loadScene = false;
            
            SceneManager.LoadScene(_nextScene);
        }
    }

    public void LoadCurrentScene()
    {
        SynchroniseSceneListWithDB();
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        LoadScene(sceneList[_currentScene].Contains(".xml") ? "Questionnaire" : sceneList[_currentScene]);
    }
    private void LoadScene(string scene)
    {
        var scenes = ExperimentSettings.SceneSettings.Scenes;
        if (scenes[_currentScene].Contains(".xml"))
        {
            var splittedFileName = scenes[_currentScene].Split('.');
            _questionnaireName = splittedFileName[0];

            _log.CreateUserAnswer(_log.GetCurrentSessionID(), splittedFileName[0]);
            _log.setQuestionnaireName(splittedFileName[0]);
        }        
        _nextScene = scene;
        _loadScene = true;
    }
  
    /// <summary>
    /// Start experiment and checks errors.
    /// </summary>
    /// <remarks>
    /// The part with the logsessionparameters is from the old implementation and is not suited for everchanging sensors
    /// </remarks>
    public void StartExperiment() {
        
        _participantId = _menuManager.ParticipantId;

        if (_participantId.Length < 1)
        {
            var originBaseMenu = GameObject.Find("Experiment Menu").GetComponent<BaseMenu>();
            _menuManager.DisplayErrorMessage("The Subject ID is invalid!", originBaseMenu);
        }
        else
        {
            SynchroniseSceneListWithDB();

            var sceneList = ExperimentSettings.SceneSettings.Scenes;
            Console.Write(sceneList.Count);
            var nParameters = _menuManager.GetExperimentParameterList().Count;
            if (sceneList.Count <= 0)
            {
                var originBaseMenu = GameObject.Find("Scene Configuration").GetComponent<BaseMenu>();
                _menuManager.DisplayErrorMessage("No scenes selected!", originBaseMenu);
            }
            else
            {
                if (SessionParameters.Count != nParameters)
                {

                    var originBaseMenu = GameObject.Find("Session Parameters Menu").GetComponent<BaseMenu>();
                    _menuManager.DisplayErrorMessage("Session parameters are not set.",originBaseMenu);
                }
                else
                {
                    _log.LogSession(ExperimentSettings.Name, _participantId);
                    if (SessionParameters.ContainsKey("Labchart File Name"))
                        _log.SetLabChartFileName(SessionParameters["Labchart File Name"]);

                    if (_log.getSensors().Contains("HL7Server"))
                        this.gameObject.GetComponent<HL7ServerStarter>().enabled = true;
                    StoreSessionParameters();
                    var startBaseMenu = GameObject.Find("Start Menu").GetComponent<BaseMenu>();
                    _menuManager.ShowMenu(startBaseMenu);
                }
            }
        }

    }

    private void StoreSessionParameters()
    {
        foreach (var entry in SessionParameters)
        {
            _log.LogSessionParameter(entry.Key, entry.Value);
        }
    }

    
    


    public string GetQuestionnaireName() {
        return _questionnaireName;
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
        return SessionParameters;
    }

    public void changeSessionsParameter(string name, string value)
    {
        if (SessionParameters.ContainsKey(name))
            SessionParameters.Remove(name);
        SessionParameters.Add(name, value);
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
        var ta = Resources.Load<TextAsset>("experiment_settings");
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

