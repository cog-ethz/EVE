using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Menu;
using Assets.EVE.Scripts.Menu.Buttons;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using Assets.EVE.Scripts.XML;
using Assets.EVE.Scripts.XML.XMLHelper;
using UnityEngine.UI;

/// <summary>
/// this class manages everything related to menus but also keeps state and important information
/// this, combined with the launcher is crucial for the allover state. the reason why the manager also keeps part of the state is because 
/// some state variables are retrieved from the launcher instead of being pushed to the launcher.
/// it also makes debugging easier as updates on the launchers information can be pinpointed.
/// </summary>
/// <remarks>
/// inspired by 3DBuzz, https://www.youtube.com/watch?v=QxRAIjXdfFU
/// </remarks>
public class MenuManager : MonoBehaviour {

    /// <summary>
    /// The name of the participant as provided in the main menu.
    /// </summary>
    public string ParticipantId { get; set; }
    
    public BaseMenu CurrentMenu;
    private LaunchManager _launchManager;
    private LoggingManager _log;
    private SceneSettings _sceneSettings;
    private string _sceneFilePath;
    private GameObject _menuObject;

    /// <summary>
    /// Session id used to interact with the database.
    ///
    /// For example, in menus and replay.
    /// </summary>
    /// <remarks>
    /// Note that this ID is most likely different than the
    /// current session id!
    /// </remarks>
    public int ActiveSessionId { get; set; }

    /// <summary>
    /// Participant Id that is currently manipulated
    /// in a menu.
    /// </summary>
    public string ActiveParticipantId { get; set; }

    /// <summary>
    /// Scene Id that is currently manipulated
    /// in a menu.
    /// </summary>
    public string ActiveSceneId { get; set; }

    public string SceneFilePath
    {
        get
        {
            return _sceneFilePath ?? (_sceneFilePath = Application.dataPath + "/Experiment/Scenes");
        }

        set
        {
            _sceneFilePath = value;
        }
    }

    public void SetActiveParameters(List<string> parameters)
    {
        ExperimentParameterList = parameters;
    }

    public void Awake()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        ExperimentParameterList = new List<string>();
    }

    public void Start() {
        _log = _launchManager.LoggingManager;
        _sceneSettings = _launchManager.ExperimentSettings.SceneSettings;
    }

    public void ShowMenu(BaseMenu menu)
    {        
        if (CurrentMenu != null) CloseCurrentMenu();
        CurrentMenu = menu;
        CurrentMenu.isOpen = true;
    }

    /// <summary>
    /// Instantiates a Menu and shows it.
    /// </summary>
    /// <param name="menu">Name of the menu to be instantiated</param>
    /// <param name="menuContext">Context in which the menu is instantiated</param>
    public void InstantiateAndShowMenu(string menu, string menuContext)
    {
        if (CurrentMenu != null) CloseCurrentMenu();
        _menuObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + menuContext + "/" + menu);
        MenuUtils.PlaceElement(_menuObject.gameObject,transform);
        CurrentMenu = _menuObject.GetComponent<BaseMenu>();
        CurrentMenu.isOpen = true;
    }

    public void CloseMenu(BaseMenu baseMenu, float removalDelay = 1f)
    {
        CurrentMenu = baseMenu;
        CloseCurrentMenu(removalDelay);
    }

    public void CloseCurrentMenu(float removalDelay = 1f)
    {
        StartCoroutine(GameObjectUtils.RemoveGameObject(CurrentMenu.gameObject, removalDelay));
        CurrentMenu.isOpen = false;
    }


    public void AddToBackOfSceneList(string sceneName)
    {
        if (_sceneSettings != null)
        {
            var scene = new SceneEntry(sceneName, false);
            _sceneSettings.Scenes.Add(new SceneEntry(sceneName,false));
            _log.AddScene(scene);
            _log.RemoveExperimentSceneOrder(_launchManager.ExperimentName);
            _log.SetExperimentSceneOrder(_launchManager.ExperimentName, _sceneSettings.Scenes.ToArray());
        }
    }
    
    public void DeleteSceneEntry(int i) {
        var removedScene = _sceneSettings.Scenes[i];
        _sceneSettings.Scenes.RemoveAt(i);
        _log.RemoveExperimentSceneOrder(_launchManager.ExperimentName);
        if (!_sceneSettings.Scenes.Contains(removedScene))
            _log.RemoveScene(removedScene);
        _log.SetExperimentSceneOrder(_launchManager.ExperimentName, _sceneSettings.Scenes.ToArray());
    }

    public void PromoteSceneEntry(int i) {
        if (i != 0) {
            var scene = _sceneSettings.Scenes[i];
            _sceneSettings.Scenes.RemoveAt(i);
            _sceneSettings.Scenes.Insert(i - 1, scene);
        }
        _log.RemoveExperimentSceneOrder(_launchManager.ExperimentName);
        _log.SetExperimentSceneOrder(_launchManager.ExperimentName, _sceneSettings.Scenes.ToArray());
    }
    
    public void RemoveExperimentParameter(string experimentParameter)
    {
        _log.RemoveExperimentParameter(experimentParameter, _launchManager.ExperimentSettings.Name);
        ExperimentParameterList.Remove(experimentParameter);
        _launchManager.SessionParameters.Remove(experimentParameter);
    }

    public List<string> ExperimentParameterList { get; private set; }

    /// <summary>
    /// Display an error message in the menu system.
    /// </summary>
    /// <param name="errorMessage">The text to be displayed</param>
    /// <param name="originBaseMenu">Optional: set a return point other then the currently active menu.</param>
    /// <param name="originContext">Optional context of active menu</param>
    public void DisplayErrorMessage(string errorMessage, string originBaseMenu, string originContext)
    {
        if (_log != null)
            _log.InsertLiveSystemEvent("ErrorLog", originContext + "/" + originBaseMenu, null, errorMessage);
        var errorMenu = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/ErrorMenu");
        MenuUtils.PlaceElement(errorMenu.gameObject, transform);
        var errorBaseMenu = errorMenu.GetComponent<ErrorMenuButtons>();
        errorBaseMenu.SetErrorOriginMenu(originBaseMenu,originContext);
        errorBaseMenu.SetErrorText(errorMessage);
        ShowMenu(errorBaseMenu.gameObject.GetComponent<BaseMenu>());
    }
}
