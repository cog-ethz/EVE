using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Menu;
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

    public int detailsInt =0;
    public int numberOfAddedSensors=0;
    public string fieldTitle = null;
    public string subjectId = null;
    private List<string> _activeSensors = new List<string>();
    private List<string> _experimentParameters;
    private string SceneFilePath;
    private GameObject menuObject;
    private GameObject oldMenuObject;
    
    public BaseMenu CurrentMenu;
    private bool loadEvalScene;
    private bool loadLoaderScene;
    private LaunchManager _launchManager;
    private LoggingManager _log;
    private SceneSettings _sceneSettings;
    private ErrorMenuButtons _errorBaseMenu;

    public string GetSceneFilePath() {
        if (SceneFilePath == null)
            SceneFilePath = Application.dataPath + "/Experiment/Scenes";
        return SceneFilePath;
    }
    public void SetSceneFilePath(string path)
    {
        SceneFilePath = path;
    }

    public void SetActiveParameters(List<string> parameters)
    {
        _experimentParameters = parameters;
    }

    public void Awake()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        //TODO delete this function
        _launchManager.SetMenuManager(this);
        _errorBaseMenu = GameObject.Find("ErrorMenu").GetComponent<ErrorMenuButtons>();
        _experimentParameters = new List<string>();
    }

    public void Start() {        
        ShowMenu(CurrentMenu);
        _log = _launchManager.GetLoggingManager();
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
        menuObject = Instantiate(Resources.Load("Prefabs/Menus/"+menuContext+"/" + menu)) as GameObject;
        CurrentMenu = menuObject.GetComponent<BaseMenu>();
    }

    public void CloseMenu(BaseMenu baseMenu)
    {
        CurrentMenu = baseMenu;
        CloseCurrentMenu();
    }

    public void CloseCurrentMenu()
    {

        CurrentMenu.isOpen = false;
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
    

    public void SetSubjectId(string subjectId)
    {
        this.subjectId = string.IsNullOrEmpty(subjectId) ? "" : subjectId;
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
            _log.RemoveExperimentSceneOrder(_launchManager.GetExperimentName());
            _log.SetExperimentSceneOrder(_launchManager.GetExperimentName(), _sceneSettings.Scenes.ToArray());
        }
    }
    
    public void DeleteSceneEntry(int i) {
        var removedScene = _sceneSettings.Scenes[i];
        _sceneSettings.Scenes.RemoveAt(i);
        _log.RemoveExperimentSceneOrder(_launchManager.GetExperimentName());
        if (!_sceneSettings.Scenes.Contains(removedScene))
            _log.RemoveScene(removedScene);
        _log.SetExperimentSceneOrder(_launchManager.GetExperimentName(), _sceneSettings.Scenes.ToArray());
    }

    public void PromoteSceneEntry(int i) {
        if (i != 0) {
            var scene = _sceneSettings.Scenes[i];
            _sceneSettings.Scenes.RemoveAt(i);
            _sceneSettings.Scenes.Insert(i - 1, scene);
        }
        _log.RemoveExperimentSceneOrder(_launchManager.GetExperimentName());
        _log.SetExperimentSceneOrder(_launchManager.GetExperimentName(), _sceneSettings.Scenes.ToArray());
    }

    public void RemoveSensor(string entryName)
    {
        _activeSensors.Remove(entryName);
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

    public void setDetailsInt(int i) {
        detailsInt = i;
    }

    public int getDetailsInt()
    {
        return detailsInt;
    }

    public void DisplayErrorMessage(string errorMessage)
    {
        _errorBaseMenu.setErrorOriginMenu(CurrentMenu);
        _errorBaseMenu.setErrorText(errorMessage);
        ShowMenu(_errorBaseMenu);
    }

    public void DisplayErrorMessage(string errorMessage, BaseMenu originBaseMenu)
    {
        _errorBaseMenu.setErrorOriginMenu(originBaseMenu);
        _errorBaseMenu.setErrorText(errorMessage);
        ShowMenu(_errorBaseMenu);
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
