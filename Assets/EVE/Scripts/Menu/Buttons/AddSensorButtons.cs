using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class AddSensorButtons : MonoBehaviour {

        private LoggingManager _log;

        private string _sensorName;
        private LaunchManager _launchManager;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;


            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("InputField").GetComponent<InputField>().onEndEdit.AddListener(answer => _sensorName = answer);
            fields.Find("OkButton").GetComponent<Button>().onClick.AddListener(AddSensorToDatabase);
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Sensor Configuration Menu", "Launcher"));
        }

        public void AddSensorToDatabase()
        {
            if (string.IsNullOrEmpty(_sensorName))
            {
                _menuManager.DisplayErrorMessage("You must use a valid name for a sensor", "Add Sensor Menu","Launcher");
            }
            else
            {
                _log.AddSensor(_sensorName);
                _launchManager.SynchroniseSensorListWithDB();
                _menuManager.InstantiateAndShowMenu("Sensor Configuration Menu", "Launcher");
            }
        }

        /// <summary>
        /// Stores name of a sensor to be submitted.
        /// </summary>
        /// <param name="name"></param>
        public void UpdateSensorName(string name)
        {
            _sensorName = name;
        }
    }
}
