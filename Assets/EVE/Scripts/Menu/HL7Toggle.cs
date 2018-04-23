using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HL7Toggle : MonoBehaviour {

    public Toggle button;
    private LoggingManager _log;

    // Use this for initialization
    void Start()
    {
        var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = launchManager.GetLoggingManager();
        gameObject.GetComponent<Toggle>().isOn = launchManager.ExperimentSettings.SensorSettings.H7Server;
    }

    public void changeActiveStatus()
    {
        if (!gameObject.GetComponent<Toggle>().isOn)
        {
            _log.RemoveSensor("HL7Server");
        }
        else
        {
            _log.AddSensor("HL7Server");
        }


    }
}
