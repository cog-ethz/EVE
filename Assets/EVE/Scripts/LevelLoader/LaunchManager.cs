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
using JetBrains.Annotations;
using UnityEngine.UI;

public class LaunchManager : MonoBehaviour
{
    public ExperimentSettings ExperimentSettings;
    public GameObject FPC;
    public GameObject MenuCanvas;
    private int _currentScene;
    private bool _loadScene, _initialized, _configureLabchart, _inQuestionnaire;
    private string _nextScene, _activeSceneName, _participantId, _filePathParticipants;

    public Dictionary<string, string> SessionParameters { get; private set; }

    /// <summary>
    /// Make the LaunchManager a singleton.
    /// </summary>
    /// 
    private static LaunchManager _instance;
    
    public int SessionId { get; set; }
    
    public string QuestionnaireName { get; private set; }
    
    public string ExperimentName {get {return ExperimentSettings.Name;} set { ExperimentSettings.Name = value; }}

    public LoggingManager LoggingManager { get; private set; }

    public MenuManager MenuManager { get; private set; }

    public QuestionnaireManager QuestionnaireManager { get; private set; }

    public int ReplaySessionId { get; set; }

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
        MenuManager = MenuCanvas.GetComponent<MenuManager>();
        MenuCanvas.GetComponent<CanvasScaler>().referenceResolution = ExperimentSettings.UISettings.ReferenceResolution;
        QuestionnaireManager = gameObject.GetComponent<QuestionnaireManager>();

        SessionParameters = new Dictionary<string, string>();


        LoggingManager = new LoggingManager();
        var connected = LoggingManager.ConnectToServer(ExperimentSettings.DatabaseSettings);
        _initialized = false;
        if (!connected)
        {
            var databaseSetupMenu = GameObject.Find("Database Configuration Menu").GetComponent<BaseMenu>();
            MenuManager.DisplayErrorMessage("Unable to connect to the database! Press ok to check the database status", databaseSetupMenu);
        }
        else
        {
            LoadSettingsIntoDB();
            SessionId = LoggingManager.CurrentSessionID;
        }

        
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
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_initialized)
        {
            _initialized = true;
            return;
        }

        var isReplay = FPC.GetComponentInChildren<ReplayRoute>().isActivated();
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        _activeSceneName = SceneManager.GetActiveScene().name;
        var subSceneName = sceneList[_currentScene];
        Debug.Log("Scene " + _currentScene  + ":" + subSceneName + " in " + _activeSceneName);
        
        if (_activeSceneName == "Launcher" && !_inQuestionnaire && !_configureLabchart)
        { //coming back from a scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            LoggingManager.LogSceneEnd(subSceneName);

            if (isReplay)
            {
                MenuManager.ShowMenu(GameObject.Find("Participant Menu").GetComponent<BaseMenu>());
            }
            else
            {
                if (_currentScene < sceneList.Count - 1)
                {
                    _currentScene++;
                    LoadCurrentScene();
                }
                else
                {
                    MenuManager.InstantiateAndShowMenu("Finish Menu","Launcher");
                }
            }
        }

        //Reset state machine flags for a clean load 
        _inQuestionnaire = false;
        _configureLabchart = false;
        
        //A new scene started that is not the 
        if (!isReplay)
            LoggingManager.LogSceneStart(subSceneName);
    }

    /// <summary>
    /// This method manually starts a new scene in EVE to
    /// account for scenes that have no 3D scenes.
    /// </summary>
    /// <remarks>
    /// Do not call this method unless you know what you are doing.</remarks>
    public void ManualContinueToNextScene()
    {
        OnSceneLoaded(new Scene(), LoadSceneMode.Additive);
    }

    // -----------------------------------------
    //			 Initialization
    //------------------------------------------	
    void Start()
    {         
        
    }

    /// <summary>
    /// The function resets the state of the participant for the next round
    /// </summary>
    public void SetCompletedAndReset() {
        _currentScene = 0;
        LoggingManager.updateParameters();
        SessionId = LoggingManager.CurrentSessionID;
    }
    
    public void LoadCurrentScene()
    {
        SynchroniseSceneListWithDB();
        var scene = ExperimentSettings.SceneSettings.Scenes[_currentScene];
        
        if (scene.Contains(".xml"))
        {
            QuestionnaireName = scene.Split('.')[0];

            LoggingManager.CreateUserAnswer(LoggingManager.CurrentSessionID, QuestionnaireName);
            LoggingManager.setQuestionnaireName(QuestionnaireName);

            scene = "Questionnaire";
        }
        switch (scene)
        {
            case "LabchartStartScene":
                _configureLabchart = true;
                MenuManager.ShowMenu(GameObject.Find("Configure Labchart Menu").GetComponent<BaseMenu>());
                ManualContinueToNextScene();
                break;
            case "Questionnaire":
                _inQuestionnaire = true;
                QuestionnaireManager.enabled = true;
                QuestionnaireManager.DisplayQuestionnaire();
                ManualContinueToNextScene();
                break;
            default:
                SceneManager.LoadScene(_nextScene);
                break;
        }
    }
  
    /// <summary>
    /// Start experiment and checks errors.
    /// </summary>
    /// <remarks>
    /// The part with the logsessionparameters is from the old implementation and is not suited for everchanging sensors
    /// </remarks>
    public void StartExperiment() {
        
        _participantId = MenuManager.ParticipantId;

        if (string.IsNullOrEmpty(_participantId))
        {
            var originBaseMenu = GameObject.Find("Experiment Menu").GetComponent<BaseMenu>();
            MenuManager.DisplayErrorMessage("The Subject ID is invalid!", originBaseMenu);
        }
        else
        {
            SynchroniseSceneListWithDB();

            var sceneList = ExperimentSettings.SceneSettings.Scenes;
            Console.Write(sceneList.Count);
            var nParameters = MenuManager.GetExperimentParameterList().Count;
            if (sceneList.Count <= 0)
            {
                var originBaseMenu = GameObject.Find("Scene Configuration").GetComponent<BaseMenu>();
                MenuManager.DisplayErrorMessage("No scenes selected!", originBaseMenu);
            }
            else
            {
                if (SessionParameters.Count != nParameters)
                {

                    var originBaseMenu = GameObject.Find("Session Parameters Menu").GetComponent<BaseMenu>();
                    MenuManager.DisplayErrorMessage("Session parameters are not set.",originBaseMenu);
                }
                else
                {
                    LoggingManager.LogSession(ExperimentSettings.Name, _participantId);
                    if (SessionParameters.ContainsKey("Labchart File Name"))
                        LoggingManager.SetLabChartFileName(SessionParameters["Labchart File Name"]);

                    if (LoggingManager.getSensors().Contains("HL7Server"))
                        this.gameObject.GetComponent<HL7ServerStarter>().enabled = true;
                    StoreSessionParameters();
                    MenuManager.InstantiateAndShowMenu("Start Menu","Launcher");
                    //var startBaseMenu = GameObject.Find("Start Menu").GetComponent<BaseMenu>();
                    //MenuManager.ShowMenu(startBaseMenu);
                }
            }
        }

    }

    private void StoreSessionParameters()
    {
        foreach (var entry in SessionParameters)
        {
            LoggingManager.LogSessionParameter(entry.Key, entry.Value);
        }
    }

    public void ChangeSessionsParameter(string name, string value)
    {
        if (SessionParameters.ContainsKey(name))
            SessionParameters.Remove(name);
        SessionParameters.Add(name, value);
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
        LoggingManager.LogExperiment(name);
        LoggingManager.RemoveExperimentSceneOrder(name);
        foreach (var scene in ExperimentSettings.SceneSettings.Scenes)
        {
            LoggingManager.AddScene(scene);
        }
        LoggingManager.SetExperimentSceneOrder(name, ExperimentSettings.SceneSettings.Scenes.ToArray());
        UpdateParameters();
        foreach (var sensor in ExperimentSettings.SensorSettings.Sensors)
        {
            LoggingManager.AddSensor(sensor);
        }
    }

    public void SynchroniseSceneListWithDB()
    {
        if (LoggingManager.CurrentSessionID> -1)
            ExperimentSettings.SceneSettings.Scenes = new List<string>(LoggingManager.getSceneNamesInOrder(ExperimentSettings.Name));
    }

    public void SynchroniseSensorListWithDB()
    {
        if (LoggingManager.CurrentSessionID> -1)
        {
            var sensors = new List<string>(LoggingManager.getSensors());
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
        var existingParams = LoggingManager.GetExperimentParameters(ExperimentSettings.Name);
        var requiredParams = ExperimentSettings.ParameterSettings.Parameters;
        foreach (var param in requiredParams)
        {
            if (!existingParams.Contains(param))
            {
                LoggingManager.CreateExperimentParameter(ExperimentSettings.Name, param);
            }
        }
        foreach (var param in existingParams)
        {
            if (!requiredParams.Contains(param))
            {
                requiredParams.Add(param);
            }
        }
        MenuManager.SetActiveParameters(requiredParams);
    }
}