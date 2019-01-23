using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ConfigurationButtons : MonoBehaviour {

        private LaunchManager _launchManager;
        private MenuManager _menuManager;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("Sensor Configuration").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Sensor Configuration Menu", "Launcher"));
            fields.Find("Experiment Parameters").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Experiment Parameters Menu", "Launcher"));
            fields.Find("Questionnaires").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Questionnaire Menu", "Launcher"));
            fields.Find("Scene Setup").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Scene Configuration Menu", "Launcher"));
            fields.Find("Save Settings").GetComponent<Button>().onClick.AddListener(SaveSettings);
            fields.Find("Reload Settings").GetComponent<Button>().onClick.AddListener(() => _launchManager.ReadExperimentSettings());
            fields.Find("Return").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher"));

        }


        /// <summary>
        /// Saves the experiment settings to a user selected location.
        /// </summary>
        public void SaveSettings()
        {
            var path = Application.persistentDataPath;
#if UNITY_EDITOR        
            path = UnityEditor.EditorUtility.SaveFilePanel("Save Experiment ExperimentSettings", Application.dataPath, "experiment_settings", "xml");
#endif
            _launchManager.WriteExperimentSettings(path);
        }
    }
}
