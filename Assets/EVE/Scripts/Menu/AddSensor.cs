using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSensor : MonoBehaviour {

    private LoggingManager _log;

    public string SensorName;
    private LaunchManager _launchManager;

    // Use this for initialization
    void Start () {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = _launchManager.GetLoggingManager();
    }

    public void ClickAddSensor()
    {
        _log.AddSensor(SensorName);
        _launchManager.SynchroniseSensorListWithDB();
    }

    public void UpdateSensorName(string name)
    {
        SensorName = name;
    }
}