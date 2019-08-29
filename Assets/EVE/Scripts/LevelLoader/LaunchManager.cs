using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Assets.EVE.Scripts.XML;
using UnityEngine.SceneManagement;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.Utils;
using Assets.EVE.Scripts.XML.XMLHelper;
using UnityEngine.UI;

/// <summary>
/// This class controls the overall flow of EVE.
///
/// Whenever an EVE action is completed the LaunchManager
/// returns into control and updates the overall state.
/// </summary>
/// <remarks>
/// Using EVE components without a launch manager active
/// may result in non-working behaviours.
/// </remarks>
public class LaunchManager : MonoBehaviour
{
    /// <summary>
    /// Experiment settings loaded from the Experiment/Resources folder.
    /// </summary>
    public ExperimentSettings ExperimentSettings { get; set; }

    /// <summary>
    /// First person controller that is moved through all scenes of an experiment.
    /// </summary>
    public GameObject FirstPersonController { get; private set; }

    /// <summary>
    /// Canvas for menus to be displayed in.
    /// </summary>
    public GameObject MenuCanvas { get; private set; }

    /// <summary>
    /// Helper variable to execute the writing of example questionnaires.
    /// </summary>
    public bool ShouldCreateExampleQuestionnaire;

    /// <summary>
    /// Session parameters for the currently active session id.
    /// </summary>
    /// <remarks>
    /// This is reset when an experiment is completed.
    /// </remarks>
    public Dictionary<string, string> SessionParameters { get; private set; }
    
    /// <summary>
    /// The session Id used for the current experiment.
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// The name of the currently active questionnaire.
    /// </summary>
    public string QuestionnaireName { get; private set; }

    /// <summary>
    /// The name of the currently active experiment.
    /// </summary>
    public string ExperimentName { get { return ExperimentSettings.Name; } set { ExperimentSettings.Name = value; } }

    /// <summary>
    /// Access to the LoggingManager.
    /// </summary>
    public LoggingManager LoggingManager { get; private set; }

    /// <summary>
    /// Access to the MenuManager.
    /// </summary>
    public MenuManager MenuManager { get; private set; }

    /// <summary>
    /// Access to the QuestionnaireManager.
    /// </summary>
    /// <remarks>
    /// Note that the QuestionnaireManager is disabled if no questionnaire is active.
    /// </remarks>
    public QuestionnaireManager QuestionnaireManager { get; private set; }

    /// <summary>
    /// Session Id which to use to start a replay.
    /// </summary>
    public int ReplaySessionId { get; set; }

    /// <summary>
    /// Make the LaunchManager a singleton.
    /// </summary>
    private static LaunchManager _instance;

    private int _currentScene;
    private bool _initialized, _configureLabchart, _inQuestionnaire;
    private string _activeSceneName, _participantId, _filePathParticipants;

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
            FirstPersonController = GameObjectUtils.InstatiatePrefab("Prefabs/Player/FPSController");// GameObject.FindGameObjectWithTag("Player");
            FirstPersonController.SetActive(false);
            DontDestroyOnLoad(FirstPersonController);
            MenuCanvas = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Canvas");GameObject.FindGameObjectWithTag("MenuCanvas");
            MenuCanvas.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            DontDestroyOnLoad(MenuCanvas); 

            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        ReadExperimentSettings();
        
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
            MenuManager.DisplayErrorMessage("Unable to connect to the database! Press ok to check the database status", "Database Configuration Menu","Launcher");
        }
        else
        {
            MenuManager.InstantiateAndShowMenu("Main Menu", "Launcher");
            LoadSettingsIntoDatabase();
            SessionId = LoggingManager.CurrentSessionId;
        }

        if(ShouldCreateExampleQuestionnaire)
        {
            QuestionnaireUtils.CreateExampleQuestionnaire(LoggingManager, ExperimentSettings);
        }
    }


    // -----------------------------------------
    //			 During one Playthrough
    //------------------------------------------
    /// <summary>
    /// This method is called whenever unity finished loading a scene or we manually move onto a new
    /// scene to update the state of the experiment.
    /// </summary>
    /// <param name="scene">Upcoming scene</param>
    /// <param name="mode">Unity parameter needed to mach listener.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_initialized)
        {
            _initialized = true;
            return;
        }

        var isReplay = FirstPersonController.GetComponentInChildren<ReplayRoute>().isActivated();
        var sceneList = ExperimentSettings.SceneSettings.Scenes;
        _activeSceneName = SceneManager.GetActiveScene().name;
        var subSceneName = sceneList[_currentScene].Name;
        Debug.Log("Scene " + _currentScene  + ":" + subSceneName + " in " + _activeSceneName);
        LoggingManager.InsertLiveSystemEvent("SceneFlow","switch",null, "Scene " + _currentScene + ":" + subSceneName.Substring(0, Math.Min(subSceneName.Length, 25)) + " in " + _activeSceneName.Substring(0, Math.Min(_activeSceneName.Length, 25)));
        FirstPersonController.transform.position = Vector3.zero;
        FirstPersonController.transform.rotation = Quaternion.identity;
        FirstPersonController.SetActive(false);
        if (_activeSceneName == "Launcher" && !_inQuestionnaire && !_configureLabchart)
        { //coming back from a scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LoggingManager.LogSceneEnd(subSceneName);

            if (isReplay)
            {
                MenuManager.InstantiateAndShowMenu("Participant Menu","Launcher");
            }
            else
            {
                if (_currentScene < sceneList.Count - 1)
                {
                    _currentScene++;
                    LoadCurrentScene();
                    return;
                }
                else
                {
                    MenuManager.InstantiateAndShowMenu("Finish Menu","Launcher");
                    SessionParameters.Clear();
                    return;
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
    /// Do not call this method unless you know what you are doing.
    /// </remarks>
    public void ManualContinueToNextScene()
    {
        OnSceneLoaded(new Scene(), LoadSceneMode.Additive);
    }

    /// <summary>
    /// The function resets the state of the participant for the next round
    /// </summary>
    public void SetCompletedAndReset() {
        _currentScene = 0;
        LoggingManager.UpdateParameters();
        SessionId = LoggingManager.CurrentSessionId;
    }
    
    /// <summary>
    /// Loads the scene to be loaded next.
    /// </summary>
    /// <remarks>
    /// The function call hides the fact that some scenes are within the
    /// main scene.
    /// </remarks>
    public void LoadCurrentScene()
    {
        SynchroniseScenesWithDatabase();
        var scene = ExperimentSettings.SceneSettings.Scenes[_currentScene].Name;
        
        if (scene.Contains(".xml"))
        {
            QuestionnaireName = scene.Split('.')[0];

            LoggingManager.CreateUserAnswer(LoggingManager.CurrentSessionId, QuestionnaireName);
            LoggingManager.SetQuestionnaireName(QuestionnaireName);

            scene = "Questionnaire";
        }
        switch (scene)
        {
			case "LabchartStartScene":
				_configureLabchart = true;
				MenuManager.InstantiateAndShowMenu ("Configure Labchart Menu","Launcher");
                ManualContinueToNextScene();
                break;
            case "Questionnaire":
                _inQuestionnaire = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                QuestionnaireManager.enabled = true;
                QuestionnaireManager.DisplayQuestionnaire();
                ManualContinueToNextScene();
                break;
            default:
                SceneManager.LoadScene(scene);
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
            MenuManager.DisplayErrorMessage("The Subject ID is invalid!", "Experiment Menu","Launcher");
        }
        else
        {
            SynchroniseScenesWithDatabase();

            var sceneList = ExperimentSettings.SceneSettings.Scenes;
            Console.Write(sceneList.Count);
            var nParameters = MenuManager.ExperimentParameterList.Count;
            if (sceneList.Count <= 0)
            {
                MenuManager.DisplayErrorMessage("No scenes selected!", "Scene Configuration", "Launcher");
            }
            else
            {
                if (SessionParameters.Count != nParameters)
                {
                    MenuManager.DisplayErrorMessage("Session parameters are not set.", "Session Parameters Menu","Launcher");
                }
                else
                {
                    LoggingManager.LogSession(ExperimentSettings.Name, _participantId);
                    if (SessionParameters.ContainsKey("Labchart File Name"))
                        LoggingManager.SetLabChartFileName(SessionParameters["Labchart File Name"]);

                    if (LoggingManager.GetSensors().Contains("HL7Server"))
                        this.gameObject.GetComponent<HL7ServerStarter>().enabled = true;
                    StoreSessionParameters();
                    MenuManager.InstantiateAndShowMenu("Start Menu","Launcher");
                    //var startBaseMenu = GameObject.Find("Start Menu").GetComponent<BaseMenu>();
                    //MenuManager.ShowMenu(startBaseMenu);
                }
            }
        }

    }

    /// <summary>
    /// Transfers the session parameters to the database.
    /// </summary>
    private void StoreSessionParameters()
    {
        foreach (var entry in SessionParameters)
        {
            LoggingManager.LogSessionParameter(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// Update currently used session parameters.
    /// </summary>
    /// <param name="sessionParameter">Name of parameter.</param>
    /// <param name="value">Value to be assumed.</param>
    public void ChangeSessionsParameter(string sessionParameter, string value)
    {
        if (SessionParameters.ContainsKey(sessionParameter))
            SessionParameters.Remove(sessionParameter);
        SessionParameters.Add(sessionParameter, value);
    }

    /// <summary>
    /// Writes the Experiment Settings into an XML at the
    /// specified location.
    /// </summary>
    /// <remarks>
    /// This will overwrite previous settings.
    /// </remarks>
    /// <param name="folderPath">location where file is stored.</param>
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

    /// <summary>
    /// Transfers settings into database.
    /// </summary>
    public void LoadSettingsIntoDatabase()
    {
        var name = ExperimentSettings.Name;
        LoggingManager.LogExperiment(name);
        LoggingManager.RemoveExperimentSceneOrder(name);
        foreach (var scene in ExperimentSettings.SceneSettings.Scenes)
        {
            LoggingManager.AddScene(scene);
        }

        LoggingManager.SetExperimentSceneOrder(name,
            ExperimentSettings.SceneSettings.Scenes.ToArray());

    UpdateParameters();
        foreach (var sensor in ExperimentSettings.SensorSettings.Sensors)
        {
            LoggingManager.AddSensor(sensor);
        }
    }

    /// <summary>
    /// Ensure that scenes in the database and EVE are in the same state.
    /// </summary>
    public void SynchroniseScenesWithDatabase()
    {
        if (LoggingManager.CurrentSessionId > -1)
        {
            ExperimentSettings.SceneSettings.Scenes = new List<SceneEntry>(LoggingManager.GetSceneNamesInOrder(ExperimentSettings.Name));
        }
        else
        {
            Debug.LogError("Scenes cannot be synchonized because the database is not setup correctly.");
        }
    }
    
    /// <summary>
    /// Ensure that sensors in the database and EVE are in the same state.
    /// </summary>
    public void SynchroniseSensorsWithDatabase()
    {
        if (LoggingManager.CurrentSessionId> -1)
        {
            var sensors = LoggingManager.GetSensors();
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
        else
        {
            Debug.LogError("Sensors cannot be synchonized because the database is not setup correctly.");
        }
    }

    public void SynchroniseExperimentParametersWithDatabase()
    {
        if (LoggingManager.CurrentSessionId > -1)
        {
            var experimentParameters = LoggingManager.GetExperimentParameters(ExperimentName);
            ExperimentSettings.ParameterSettings.Parameters = experimentParameters;
        }
        else
        {
            Debug.LogError("Experiment parameters cannot be synchonized because the database is not setup correctly.");
        }
    }

    /// <summary>
    /// This method allows to establish a correspondence between the parameters
    /// described in the database and in the experiment settings. 
    /// 
    /// This should be called whenever manually experiment parameters have been
    /// added within the EVE UI or within the Experiment Settings XML file.
    /// </summary>
    public void UpdateParameters()
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
        ExperimentSettings.ParameterSettings.Parameters = requiredParams;
        MenuManager.SetActiveParameters(requiredParams);
    }

    /// <summary>
    /// Enable or disable the connection to MiddleVR.
    /// </summary>
    /// <param name="enable">Whether to use MiddleVR.</param>
    // ReSharper disable once InconsistentNaming
    public void SetActiveMiddleVR(bool enable)
    {
        throw new NotImplementedException();
    }
}