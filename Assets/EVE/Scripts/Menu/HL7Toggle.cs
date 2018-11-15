using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HL7Toggle : MonoBehaviour {

    public Toggle button;
    private LoggingManager _log;
    private HL7ServerStarter hl7ServerStarter;
    private LaunchManager launchManager;

    // Use this for initialization
    void Start()
    {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = launchManager.GetLoggingManager();
        hl7ServerStarter = launchManager.gameObject.GetComponent<HL7ServerStarter>();
        gameObject.GetComponent<Toggle>().isOn = launchManager.ExperimentSettings.SensorSettings.H7Server;
    }

    public void changeActiveStatus()
    {
        if (!gameObject.GetComponent<Toggle>().isOn)
        {
            hl7ServerStarter.enabled = false;
            launchManager.ExperimentSettings.SensorSettings.H7Server = false;
            _log.RemoveSensor("HL7Server");

        }
        else
        {
            hl7ServerStarter.enabled = true;
            launchManager.ExperimentSettings.SensorSettings.H7Server = true;
            _log.AddSensor("HL7Server");
        }


    }
}
