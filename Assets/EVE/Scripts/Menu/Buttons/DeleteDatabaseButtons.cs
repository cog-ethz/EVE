using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class DeleteDatabaseButtons : MonoBehaviour
    {
        private int _sid=-1;
        private string _pid = "";
        private GameObject _item;
        private LaunchManager _launchManager;
        private MenuManager _menuManager;
        private LoggingManager _log;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _log = _launchManager.LoggingManager;
            _menuManager = _launchManager.MenuManager;
        }

        public void ResetDatabase() {
            _log.ConnectToServerAndResetSchema(_launchManager.ExperimentSettings.DatabaseSettings);
            _log.LogExperiment(_launchManager.ExperimentName);
            _launchManager.SessionId = _log.CurrentSessionID;
            _launchManager.LoadSettingsIntoDB();
            _launchManager.MenuManager.ShowMenu(GameObject.Find("Database Configuration Menu").GetComponent<BaseMenu>());
        }
    }
}
