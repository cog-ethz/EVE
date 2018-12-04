using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Menu;
using Assets.EVE.Scripts.Menu.Buttons;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using Assets.EVE.Scripts.XML;
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

    private Dictionary<string,string> _menuState;


    public BaseMenu CurrentMenu;
    private LaunchManager _launchManager;
    private LoggingManager _log;
    private SceneSettings _sceneSettings;
    private List<string> _experimentParameters;
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
    /// Representation of a list item that is manipulated.
    /// </summary>
    public GameObject ActiveListItem { get; set; }



    public string GetSceneFilePath() {
        if (_sceneFilePath == null)
            _sceneFilePath = Application.dataPath + "/Experiment/Scenes";
        return _sceneFilePath;
    }
    public void SetSceneFilePath(string path)
    {
        _sceneFilePath = path;
    }

    public void SetActiveParameters(List<string> parameters)
    {
        _experimentParameters = parameters;
    }

    public void Awake()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _experimentParameters = new List<string>();
        _menuState = new Dictionary<string, string>();
    }

    public void Start() {        
        //ShowMenu(CurrentMenu);
        InstantiateAndShowMenu("Main Menu", "Launcher");
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

    public void CloseMenu(BaseMenu baseMenu)
    {
        CurrentMenu = baseMenu;
        CloseCurrentMenu();
    }

    public void CloseCurrentMenu()
    {
        StartCoroutine(RemoveWithDelay(CurrentMenu.gameObject,1));
        CurrentMenu.isOpen = false;
    }

    private IEnumerator RemoveWithDelay(GameObject currentMenu, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(currentMenu);
    }

    public void AddExperimentParameter(string experimentParameter)
    {
        if (_experimentParameters.Contains(experimentParameter))
        {
            Debug.LogWarning("Parameter exists! " + experimentParameter);
            return;
        }
        _experimentParameters.Add(experimentParameter);
        AddExperimentParameterToDb(experimentParameter);
    }

    public void AddExperimentParameter(Text text)
    {
        var experimentParameter = text.text;
        if (_experimentParameters.Contains(experimentParameter))
        {
            Debug.LogWarning("Parameter exists! " + experimentParameter);
            return;
        }
        _experimentParameters.Add(experimentParameter);
        AddExperimentParameterToDb(experimentParameter);
    }
    
    public void AddExperimentParameterToDb(string attributeName)
    {       
        _log.CreateExperimentParameter(_launchManager.ExperimentSettings.Name, attributeName);
    }
    
    
    
    
    //no longer used?
    //public void deleteParentObject(GameObject deleteButton) {
    //    Destroy(deleteButton.transform.parent.gameObject);
    //}

    public void AddToBackOfSceneList(string sceneName)
    {
        if (_sceneSettings != null)
        {
            _sceneSettings.Scenes.Add(sceneName);
            _log.AddScene(sceneName);
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
        _experimentParameters.Remove(experimentParameter);
        _launchManager.SessionParameters.Remove(experimentParameter);
    }
    
    public List<string> GetExperimentParameterList()
    {
        return _experimentParameters;
    }
    
    /// <summary>
    /// Display an error message in the menu system.
    /// </summary>
    /// <param name="errorMessage">The text to be displayed</param>
    /// <param name="originBaseMenu">Optional: set a return point other then the currently active menu.</param>
    public void DisplayErrorMessage(string errorMessage, BaseMenu originBaseMenu = null)
    {
        var errorMenu = GameObjectUtils.InstatiatePrefab("Prefabs/Menu/ErrorMenu");
        var errorBaseMenu = errorMenu.GetComponent<ErrorMenuButtons>();
        errorBaseMenu.SetErrorOriginMenu(originBaseMenu == null ? CurrentMenu : originBaseMenu);
        errorBaseMenu.SetErrorText(errorMessage);
        ShowMenu(errorBaseMenu.gameObject.GetComponent<BaseMenu>());
    }
    
    /// <summary>
    /// Compares whether two lists contain the same elements ignoring order.
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order
    /// </remarks>
    /// <typeparam name="T">Type in List</typeparam>
    /// <param name="list1">First List</param>
    /// <param name="list2">Second List</param>
    /// <returns>Whether both lists contain the same elements ignoring order</returns>
    public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
        var cnt = new Dictionary<T, int>();
        foreach (T s in list1)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]++;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]--;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
    }
}
