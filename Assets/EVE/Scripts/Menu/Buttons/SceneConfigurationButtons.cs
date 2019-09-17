using System.Collections.Generic;
using System.IO;
using Assets.EVE.Scripts.Utils;
using Assets.EVE.Scripts.XML.XMLHelper;
using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SceneConfigurationButtons : MonoBehaviour {

        private List<SceneEntry> _scenes;
        private LaunchManager _launchManager;
        private Transform _choosenScenesList;
        private Transform _availableScenesList;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _scenes = new List<SceneEntry>();

            _availableScenesList = transform.Find("Panel").Find("SceneSelection").Find("DynFieldsWithScrollbarRight").Find("DynFields");
            MenuUtils.ClearList(_availableScenesList);
            
            _choosenScenesList = transform.Find("Panel").Find("SceneSelection").Find("DynFieldsWithScrollbarLeft").Find("DynFields");

            var fields = transform.Find("Panel").Find("Folder Selection");
            fields.Find("FolderAndField").Find("PathField").GetComponent<InputField>().onEndEdit.AddListener(answer => _menuManager.SceneFilePath = answer);
            fields.Find("FolderAndField").Find("SelectPathButton").GetComponent<Button>().onClick.AddListener(OpenFolder);
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Configuration Menu", "Launcher"));

            UpdateAvailableScenes();
            UpdateChosenScenes();
        }

        /// <summary>
        /// Add elements to the available scenes list.
        /// </summary>
        public void UpdateAvailableScenes()
        {
            var path = _launchManager.MenuManager.SceneFilePath;
            var filenames = Directory.GetFiles(path);

            if (!transform.Find("Panel").Find("KeepScenesButton").Find("Button").GetComponent<Toggle>().isOn)
            {
                MenuUtils.ClearList(_availableScenesList);
            }

            foreach (var filename in filenames)
            {
                //this block is a filter which filters files by the ".unity" or ".xml"-ending(questionnaires)
                var splittedFilename = filename.Split('.');
                var fileType = splittedFilename[splittedFilename.Length - 1];
                if (!(fileType.Equals("unity") || fileType.Equals("xml"))) continue;

                //this block adds the data to the menu
                var filenameObj = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/AvailableSceneEntry");
                MenuUtils.PlaceElement(filenameObj, _availableScenesList);

                var localFilename = filename;
                filenameObj.transform.Find("AddButton").GetComponent<Button>().onClick.AddListener(()=> { AddToChoosenScenes(localFilename); });
                filenameObj.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() => { Destroy(filenameObj); });
                filenameObj.transform.Find("SceneName").GetComponent<Text>().text = RemovePathPrefix(filename);
            }
        }

        /// <summary>
        /// Add Elements to the choosen scenes list.
        /// </summary>
        public void UpdateChosenScenes()
        {
            _launchManager.SynchroniseScenesWithDatabase();
            MenuUtils.ClearList(_choosenScenesList);
            _scenes = _launchManager.ExperimentSettings.SceneSettings.Scenes;
            foreach (var scene in _scenes)
            {
                _menuManager.ActiveSceneId = scene.Name;
                var filenameObj = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/ChoosenSceneEntry");
                MenuUtils.PlaceElement(filenameObj, _choosenScenesList);
                filenameObj.transform.Find("SceneName").GetComponent<Text>().text = RemovePathPrefix(scene.Name);
                filenameObj.transform.Find("MoveUpButton").GetComponent<Button>().onClick.AddListener(() => { MoveUpChoosenSceneEntry(filenameObj); });
                filenameObj.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() =>  _menuManager.InstantiateAndShowMenu("Scene Options Menu", "Launcher"));
                filenameObj.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() => { RemoveSceneEntry(filenameObj); });
                
            }
        }

        /// <summary>
        /// Opens File menu to select a folder from where to read scenes.
        /// </summary>
        public void OpenFolder()
        {
            var scenePath = "";
#if UNITY_EDITOR
            scenePath = UnityEditor.EditorUtility.OpenFolderPanel("", Application.dataPath + "/Experiment/Scenes", "unity,xml");
#endif
            //call setscenepath as with the folder textfield
            if (scenePath != null && !scenePath.Equals(""))
            {
                _menuManager.SceneFilePath = scenePath;
            }
            UpdateAvailableScenes();
            transform.Find("Panel").Find("Folder Selection").Find("FolderAndField").Find("PathField").GetComponent<InputField>().text = scenePath;
        }

        /// <summary>
        /// Adds a scene from the choice list to the choosen list.
        /// </summary>
        /// <param name="fileName">Scene name to be added.</param>
        public void AddToChoosenScenes(string fileName)
        {
            var filenameCropped = RemovePathPrefix(fileName);

            if (!filenameCropped.Contains("xml"))
            {
                if (filenameCropped.Contains("."))
                {
                    filenameCropped = filenameCropped.Split('.')[0];
                }
            }
            else
            {
                if (_launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires == null)
                {
                    _launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires = new List<string>();
                }
                var helper = filenameCropped.Split('.');
                if (!_launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires.Contains(helper[0]))
                {
                    _launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires.Add(helper[0]);
                }
            }
            _menuManager.AddToBackOfSceneList(filenameCropped);
            UpdateChosenScenes();
        }

        /// <summary>
        /// Deletes scene entry from list.
        /// </summary>
        /// <param name="sceneEntry">Item to be deleted</param>
        public void RemoveSceneEntry(GameObject sceneEntry)
        {
            _menuManager.DeleteSceneEntry(GameObjectUtils.GetIndexOfGameObject(sceneEntry));
            UpdateChosenScenes();
        }

        /// <summary>
        /// Moves a scene entry up in the list.
        /// </summary>
        /// <param name="sceneEntry">Item to be moved.</param>
        public void MoveUpChoosenSceneEntry(GameObject sceneEntry)
        {
            _menuManager.MoveUpSceneEntry(GameObjectUtils.GetIndexOfGameObject(sceneEntry));
            UpdateChosenScenes();
        }

        /// <summary>
        /// Removes any folder prefixes from file paths.
        /// </summary>
        /// <param name="filePath">Long file path.</param>
        /// <returns>File name without prefix.</returns>
        private string RemovePathPrefix(string filePath)
        {
            var filenameCropped = filePath;
            //extract only the name and the ending without the path
            if (filenameCropped.Contains("\\"))
            {
                var splittedFilename = filenameCropped.Split('\\');
                filenameCropped = splittedFilename[splittedFilename.Length - 1];
            }
            else if (filenameCropped.Contains("/"))
            {
                var splittedFilename = filenameCropped.Split('/');
                filenameCropped = splittedFilename[splittedFilename.Length - 1];
            }

            return filenameCropped;
        }
    }
}

