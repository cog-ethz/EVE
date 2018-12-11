using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class DeleteDatabaseButtons : MonoBehaviour
    {
        private LaunchManager _launchManager;
        private MenuManager _menuManager;
        private LoggingManager _log;
        private Transform _fields;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _log = _launchManager.LoggingManager;
            _menuManager = _launchManager.MenuManager;
            _fields = transform.Find("Panel").Find("Fields");
            
            _fields.Find("YesButton").GetComponent<Button>().onClick.AddListener(ResetDatabase);
            _fields.Find("NoButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Database Configuration Menu", "Launcher"));

        }

        public void ResetDatabase() {
            _log.ConnectToServerAndResetSchema(_launchManager.ExperimentSettings.DatabaseSettings);
            _log.LogExperiment(_launchManager.ExperimentName);
            _launchManager.SessionId = _log.CurrentSessionId;
            _launchManager.LoadSettingsIntoDB();
            _launchManager.MenuManager.InstantiateAndShowMenu("Database Configuration Menu","Launcher");
        }
    }
}
