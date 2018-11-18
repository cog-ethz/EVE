using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class AddSensorButtons : MonoBehaviour {

        private LoggingManager _log;

        public string SensorName;
        private LaunchManager _launchManager;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _log = _launchManager.GetLoggingManager();
        }

        public void AddSensorToDatabase()
        {
            _log.AddSensor(SensorName);
            _launchManager.SynchroniseSensorListWithDB();
        }

        /// <summary>
        /// Stores name of a sensor to be submitted.
        /// </summary>
        /// <param name="name"></param>
        public void UpdateSensorName(string name)
        {
            SensorName = name;
        }
    }
}
