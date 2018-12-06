using System.Collections.Generic;
using System.IO;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SceneConfigurationButtons : MonoBehaviour {

        private List<string> _scenes;
        private LaunchManager _launchManager;
        private Transform _choosenScenesList;
        private Transform _availableScenesList;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _scenes = new List<string>();

            _availableScenesList = transform.Find("Panel").Find("SceneSelection").Find("DynFieldsWithScrollbarRight").Find("DynFields");
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

            MenuUtils.ClearList(_availableScenesList);

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
                filenameObj.transform.Find("AddButton").GetComponent<Button>().onClick.AddListener(()=>
                {
                    AddToChoosenScenes(localFilename);
                });

                filenameObj.transform.Find("SceneName").GetComponent<Text>().text = GetFileNameOnly(filename);
            }
        }

        /// <summary>
        /// Add Elements to the choosen scenes list.
        /// </summary>
        public void UpdateChosenScenes()
        {
            _launchManager.SynchroniseSceneListWithDB();
            MenuUtils.ClearList(_choosenScenesList);
            _scenes = _launchManager.ExperimentSettings.SceneSettings.Scenes;
            foreach (var filename in _scenes)
            {
                var filenameObj = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/ChoosenSceneEntry");
                MenuUtils.PlaceElement(filenameObj, _choosenScenesList);
                filenameObj.transform.Find("SceneName").GetComponent<Text>().text = GetFileNameOnly(filename);
                filenameObj.transform.Find("MoveUpButton").GetComponent<Button>().onClick.AddListener(() => { MoveUpChoosenSceneEntry(filenameObj); });
                filenameObj.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() =>  _menuManager.InstantiateAndShowMenu("Edit Scene Settings Menu", "Launcher"));
                filenameObj.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() => { RemoveChoosenSceneEntry(filenameObj); });
                
            }
        }

        /// <summary>
        /// Opens File menu to select a folder from where to read scenes.
        /// </summary>
        public void OpenFolder()
        {
            var scenePath = "";
#if UNITY_EDITOR
            scenePath = UnityEditor.EditorUtility.OpenFilePanel("", "D:/git/EVE/Assets/Experiment/Scenes", "unity,xml");
#endif
            //call setscenepath as with the folder textfield
            if (scenePath != null && !scenePath.Equals(""))
            {
                _menuManager.SceneFilePath = scenePath;
            }
            UpdateAvailableScenes();
            transform.Find("Panel").Find("Folder Selection").Find("PathField").GetComponent<InputField>().text = scenePath;
        }

        public void AddToChoosenScenes(string fileName)
        {
            var filenameCropped = GetFileNameOnly(fileName);

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

        public void RemoveChoosenSceneEntry(GameObject item)
        {
            var numberOfEntry = 0;
            
            var entriesObjects = new List<GameObject>();
            foreach (Transform entry in _choosenScenesList) entriesObjects.Add(entry.gameObject);
            var i = 0;
            foreach (var entryObject in entriesObjects)
            {
                if (entryObject == item)
                {
                    numberOfEntry = i;
                }
                i++;
            };

            Destroy(item);

            _menuManager.DeleteSceneEntry(numberOfEntry);
            UpdateChosenScenes();
        }

        public void MoveUpChoosenSceneEntry(GameObject item)
        {
            var numberOfEntry = 0;
            var entriesObjects = new List<GameObject>();
            foreach (Transform entry in _choosenScenesList) entriesObjects.Add(entry.gameObject);
            var i = 0;
            foreach (var entryObject in entriesObjects)
            {
                if (entryObject == item)
                {
                    numberOfEntry = i;
                }
                i++;
            };

            _menuManager.PromoteSceneEntry(numberOfEntry);

            UpdateChosenScenes();
        }

        private string GetFileNameOnly(string filePath)
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

