using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SensorConfigurationButtons : MonoBehaviour {

        private MenuManager _menuManager;
        private LaunchManager _launchManager;
        private HL7ServerStarter _hl7ServerStarter;
        private LoggingManager _log;
        public Toggle LabchartButton;
        public Toggle HL7ServerButton;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.GetMenuManager();

            _log = _launchManager.GetLoggingManager();
            _hl7ServerStarter = _launchManager.gameObject.GetComponent<HL7ServerStarter>();
            HL7ServerButton.isOn = _launchManager.ExperimentSettings.SensorSettings.H7Server;
            LabchartButton.isOn = _launchManager.ExperimentSettings.SensorSettings.Labchart;

            DisplaySensorList();
        }

        void DisplaySensorList()
        {
            if (_launchManager.getCurrentSessionID() >= 0)
            {
                var sensors = _log.getSensors();

                if (sensors.Contains("Labchart"))
                {
                    LabchartButton.isOn = true;
                    sensors.Remove("Labchart");
                }

                if (sensors.Contains("HL7Server"))
                {
                    HL7ServerButton.isOn = true;
                    sensors.Remove("HL7Server");
                }

                var dynamicField = LabchartButton.transform.parent.parent;
                foreach (var sensorName in sensors)
                {
                    var sensorDisplay = Instantiate(Resources.Load("Prefabs/Menus/SensorDisplay")) as GameObject;
                    Utils.PlaceElement(sensorDisplay,dynamicField);
                    sensorDisplay.transform.Find("SensorName").GetComponent<Text>().text = sensorName;
                }
                _launchManager.SynchroniseSensorListWithDB();
            }
        }

        public void LabchartToggle()
        {
            var scenes = _launchManager.ExperimentSettings.SceneSettings.Scenes;
            if (!LabchartButton.isOn)
            {
                _menuManager.RemoveExperimentParameter("Labchart File Name");
                _launchManager.ExperimentSettings.SensorSettings.Labchart = false;
                _launchManager.SynchroniseSceneListWithDB();
                _menuManager.DeleteSceneEntry(
                    _launchManager.ExperimentSettings.SceneSettings.Scenes.FindIndex(a => a == "LabchartStartScene"));
                _log.RemoveSensor("Labchart");
            }
            else if (!scenes.Contains("LabchartStartScene"))
            {
                _launchManager.ExperimentSettings.SensorSettings.Labchart = true;
                _log.AddSensor("Labchart");
                _menuManager.AddToBackOfSceneList("LabchartStartScene");
                
            }

        }

        public void HL7ServerToggle()
        {
            if (!HL7ServerButton.isOn)
            {
                _hl7ServerStarter.enabled = false;
                _launchManager.ExperimentSettings.SensorSettings.H7Server = false;
                _log.RemoveSensor("HL7Server");

            }
            else
            {
                _hl7ServerStarter.enabled = true;
                _launchManager.ExperimentSettings.SensorSettings.H7Server = true;
                _log.AddSensor("HL7Server");
            }
        }
    }
}
