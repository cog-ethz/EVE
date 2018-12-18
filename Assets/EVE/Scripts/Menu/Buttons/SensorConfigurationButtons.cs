using System;
using System.Collections.Generic;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SensorConfigurationButtons : MonoBehaviour {

        private MenuManager _menuManager;
        private LaunchManager _launchManager;
        private HL7ServerStarter _hl7ServerStarter;
        private LoggingManager _log;
        private Toggle _labchartButton, _hl7ServerButton, _middleVRButton;
        private Transform _dynFields;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            _log = _launchManager.LoggingManager;
            _hl7ServerStarter = _launchManager.gameObject.GetComponent<HL7ServerStarter>();


            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("AddButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Add Sensor Menu", "Launcher"));
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Configuration Menu", "Launcher"));

            _labchartButton = fields.Find("LabchartButton").Find("Button").GetComponent<Toggle>();
            _labchartButton.isOn = _launchManager.ExperimentSettings.SensorSettings.Labchart;
            _labchartButton.onValueChanged.AddListener(LabchartToggle);

            _hl7ServerButton = fields.Find("HL7Button").Find("Button").GetComponent<Toggle>();
            _hl7ServerButton.isOn = _launchManager.ExperimentSettings.SensorSettings.H7Server;
            _hl7ServerButton.onValueChanged.AddListener(HL7ServerToggle);

            _middleVRButton = fields.Find("MiddleVRButton").Find("Button").GetComponent<Toggle>();
            _middleVRButton.isOn = _launchManager.ExperimentSettings.SensorSettings.MiddleVR;
            _middleVRButton.onValueChanged.AddListener((enabled)=>_launchManager.SetActiveMiddleVR(enabled));

            _dynFields = fields.Find("DynFieldsWithScrollbar").Find("DynFields");


            DisplaySensorList();
        }

        /// <summary>
        /// Display the current list of sensors.
        /// </summary>
        public void DisplaySensorList()
        {
            MenuUtils.ClearList(_dynFields);

            if (_launchManager.SessionId < 0) return;

            var sensors = _log.GetSensors();

            if (sensors.Contains("Labchart"))
            {
                _labchartButton.isOn = true;
                sensors.Remove("Labchart");
            }

            if (sensors.Contains("HL7Server"))
            {
                _hl7ServerButton.isOn = true;
                sensors.Remove("HL7Server");
            }

            if (sensors.Contains("MiddleVR"))
            {
                _middleVRButton.isOn = true;
                sensors.Remove("MiddleVR");
            }

            foreach (var sensorName in sensors)
            {

                var sensorDisplay = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/SensorEntry");
                sensorDisplay.transform.Find("RemoveSensor").GetComponent<Button>().onClick.AddListener(() => { RemoveSensor(sensorDisplay); });
                MenuUtils.PlaceElement(sensorDisplay, _dynFields);
                sensorDisplay.transform.Find("SensorName").GetComponent<Text>().text = sensorName;
            }
            _launchManager.SynchroniseSensorsWithDatabase();
        }

        public void LabchartToggle(bool enable)
        {
            var scenes = _launchManager.ExperimentSettings.SceneSettings.Scenes;
            if (!enable)
            {
                _menuManager.RemoveExperimentParameter("Labchart File Name");
                _launchManager.ExperimentSettings.SensorSettings.Labchart = false;
                _launchManager.SynchroniseScenesWithDatabase();
                _menuManager.DeleteSceneEntry(
                    _launchManager.ExperimentSettings.SceneSettings.Scenes.FindIndex(entry => entry.Name == "LabchartStartScene"));
                _log.RemoveSensor("Labchart");
            }
            else if (!scenes.Exists(entry => entry.Name == "LabchartStartScene"))//Contains(()=>"LabchartStartScene"))
            {
                _launchManager.ExperimentSettings.SensorSettings.Labchart = true;
                _log.AddSensor("Labchart");
                _menuManager.AddToBackOfSceneList("LabchartStartScene");
                
            }

        }


        public void HL7ServerToggle(bool enable)
        {
            if (!enable)
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
        
        /// <summary>
        /// Removes a sensor from the list.
        /// </summary>
        /// <param name="item"></param>
        public void RemoveSensor(GameObject item)
        {
            _log.RemoveSensor(item.transform.Find("SensorName").GetComponent<Text>().text);
            _launchManager.SynchroniseSensorsWithDatabase();
            Destroy(item);
        }
    }
}
