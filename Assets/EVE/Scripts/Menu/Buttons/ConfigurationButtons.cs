using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ConfigurationButtons : MonoBehaviour {

        private LaunchManager _launchManager;

        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        }


        /// <summary>
        /// Saves the experiment settings to a user selected location.
        /// </summary>
        public void SaveSettings()
        {
            var path = Application.persistentDataPath;
#if UNITY_EDITOR
            path = UnityEditor.EditorUtility.SaveFilePanel("Save Experiment Settings", "", "experiment_settings", "xml");
#endif
            _launchManager.WriteExperimentSettings(path);
        }

        /// <summary>
        /// Reloads the experiment settings from the standard location..
        /// </summary>
        public void ReloadSettings()
        {
            _launchManager.ReadExperimentSettings();
        }
    }
}
