using UnityEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Assets.EVE.Scripts.XML;
using UnityEngine.SceneManagement;
using Assets.EVE.Scripts.Questionnaire2;
using Assets.EVE.Scripts.Questionnaire2.Enums;
using Assets.EVE.Scripts.Questionnaire2.Questions;
using Assets.EVE.Scripts.Questionnaire2.XMLHelper;
using QuestionSet = Assets.EVE.Scripts.Questionnaire2.QuestionSet;

public class LaunchManager : MonoBehaviour
{
    public ExperimentSettings ExperimentSettings;
    public GameObject FPC;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        ReadExperimentSettings();
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
        _menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
        _sessionParameters = new Dictionary<string, string>();
        LoadSettingsIntoDB();
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
            Cursor.lockState = UnityEngine.CursorLockMode.None;
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
                _menuManager.ShowMenu(GameObject.Find("Finished").GetComponent<Menu>());
            }
        }
        else if (activeSceneName == "Loader" && previousSceneName == "Evaluation")
        {
            _menuManager.ShowMenu(GameObject.Find("Evaluation Menu").GetComponent<Menu>());
        }
        else if (activeSceneName == "Evaluation" && previousSceneName != "Loader")
        {
            Cursor.lockState = UnityEngine.CursorLockMode.None;
            Cursor.visible = true;
            FPC.SetActive(false);
            this.enabled = true;
            _menuManager.setDetailsInt(_replaySessionId);
            _menuManager.ShowMenu(GameObject.Find("Evaluation Details").GetComponent<Menu>());
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
            var databaseSetupMenu = GameObject.Find("SetupDatabaseMenu").GetComponent<Menu>();
            _menuManager.DisplayErrorMessage("Unable to connect to the database! Press ok to check the database status", databaseSetupMenu);
        }
        else
        {
            LoadSettingsIntoDB();
        }
    }


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

	//error checking before running an experiment
	//the part with the logsessionparameters is from the old implementation and is not suited for everchanging sensors
    public void startExperiment() {
        
        _subjectId = _menuManager.subjectId;

        if (_subjectId.Length < 1)
        {
            Menu originMenu = GameObject.Find("Experiment Menu").GetComponent<Menu>();
            _menuManager.DisplayErrorMessage("The Subject ID is invalid!", originMenu);
        }
        else
        {
            SynchroniseSceneListWithDB();

            var sceneList = ExperimentSettings.SceneSettings.Scenes;
            Console.Write(sceneList.Count);
            int nParameters = _menuManager.GetAttributesList().Count;
            if (sceneList.Count <= 0)
            {
                Menu originMenu = GameObject.Find("Scene Config").GetComponent<Menu>();
                _menuManager.DisplayErrorMessage("No scenes selected!", originMenu);
            }
            else
            {
                if (_sessionParameters.Count != nParameters)
                {

                    Menu originMenu = GameObject.Find("Attribute Form").GetComponent<Menu>();
                    _menuManager.DisplayErrorMessage("Session parameters are not set.",originMenu);
                }
                else
                {
                    _log.LogSession(ExperimentSettings.Name, _subjectId);
                    if (_sessionParameters.ContainsKey("Labchart File Name"))
                        _log.SetLabChartFileName(_sessionParameters["Labchart File Name"]);

                    if (_log.getSensors().Contains("HL7Server"))
                        this.gameObject.GetComponent<HL7ServerStarter>().enabled = true;
                    storeSessionParameters();
                    Menu startMenu = GameObject.Find("Start Menu").GetComponent<Menu>();
                    _menuManager.ShowMenu(startMenu);
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

