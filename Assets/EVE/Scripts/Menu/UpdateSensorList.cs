using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpdateSensorList : MonoBehaviour {

    List<string> sensors = new List<string>();
    MenuManager _menuManager;
    private LaunchManager _launchManager;
    private LoggingManager _log;

    // Use this for initialization
    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _menuManager = _launchManager.GetMenuManager();
        _log = _launchManager.GetLoggingManager();
        sensors = new List<string>();
    }
    // Update is called once per frame
    void Update()
    {
        if (_launchManager.getCurrentSessionID() >= 0)
        {
            List<string> currentlist = new List<string>(_log.getSensors());
            if (MenuManager.ScrambledEquals(sensors, currentlist)) return;

            sensors = new List<string>(currentlist);

            //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
            //var menu = GameObject.Find("Sensor Configuration").GetComponent<Menu>();
            //Transform dynamicFieldT = GameObject.Find("Sensor Configuration").GetComponent<Menu>().getDynamicFields("DynFields");
            var dynamicFieldT = transform
                    .Find("DynFields").transform;
            List<GameObject> entriesObjects = new List<GameObject>();
            foreach (Transform entry in dynamicFieldT)
            {
                if (!(entry.name == "LabchartButton" || entry.name == "HL7Button"))
                    entriesObjects.Add(entry.gameObject);
            }

            foreach (GameObject entryObject in entriesObjects) Destroy(entryObject);

            if (sensors.Contains("Labchart"))
            {
                // activate toggle
                dynamicFieldT.Find("LabchartButton").Find("Button").GetComponent<UnityEngine.UI.Toggle>().isOn = true;
                sensors.Remove("Labchart");
            }

            if (sensors.Contains("HL7Server"))
            {
                // activate toggle
                dynamicFieldT.Find("HL7Button").Find("Button").GetComponent<UnityEngine.UI.Toggle>().isOn = true;
                sensors.Remove("HL7Server");
            }

            foreach (string sensorName in sensors)
            {
                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/SensorDisplay")) as GameObject;
                
                filenameObj.transform.SetParent(dynamicFieldT);
                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x,
                    filenameObj.transform.localPosition.y, dynamicFieldT.localPosition.z);
                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                Transform te = filenameObj.transform.Find("SensorName");
                UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
                tex.text = sensorName;
            }

            _launchManager.SynchroniseSensorListWithDB();
        }
    }
}