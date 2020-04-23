using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace EVE.Scripts.Menu.Buttons
{
    public class StartUpButtons : MonoBehaviour
    {
        private LaunchManager _launchManager;
        private MenuManager _menuManager;
        private Toggle _rememberToggle;

        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            var fields = transform.Find("Panel").Find("Fields");
            _rememberToggle = fields.Find("Remember Button").Find("Button").GetComponent<Toggle>();
            fields.Find("Tiles").Find("Experiment Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                _launchManager.ExperimentSettings.MenuSettings.FirstMenu = _rememberToggle.isOn ? "Main Menu" : "Startup Menu";
                _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher");
            });
            fields.Find("Tiles").Find("Evaluation Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                _launchManager.ExperimentSettings.MenuSettings.FirstMenu = _rememberToggle.isOn ? "Data Explorer Menu" : "Startup Menu";
                _menuManager.InstantiateAndShowMenu("Data Explorer Menu", "Launcher");
            });
        }
    }
}
