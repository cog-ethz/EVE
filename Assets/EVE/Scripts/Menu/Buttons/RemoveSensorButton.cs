using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveSensorButton : MonoBehaviour {
    private LoggingManager _log;
    private LaunchManager _launchManager;


    // Use this for initialization
    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = _launchManager.GetLoggingManager();
    }

    public void RemoveSensor(GameObject item)
    {
        _log.RemoveSensor(transform.parent.Find("SensorName").GetComponent<Text>().text);
        _launchManager.SynchroniseSensorListWithDB();
        Destroy(item);
    }
}
