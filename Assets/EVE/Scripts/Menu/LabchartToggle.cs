using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabchartToggle : MonoBehaviour {
    
    private MenuManager _menuManager;
    private LaunchManager _launchManager;

    // Use this for initialization
    void Start ()
    {
        _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _menuManager = _launchManager.GetMenuManager();
        gameObject.GetComponent<Toggle>().isOn = _launchManager.ExperimentSettings.SensorSettings.Labchart;
    }
	
    public void ChangeActiveStatus()
    {
        var scenes = _launchManager.ExperimentSettings.SceneSettings.Scenes;
        if (!gameObject.GetComponent<Toggle>().isOn)
        {
            _menuManager.RemoveExperimentParameter("Labchart File Name");
            _launchManager.ExperimentSettings.SensorSettings.Labchart = false;
            _launchManager.SynchroniseSceneListWithDB();
            _menuManager.DeleteSceneEntry(_launchManager.ExperimentSettings.SceneSettings.Scenes.FindIndex(a => a == "LabchartStartScene"));
            _launchManager.GetLoggingManager().RemoveSensor("Labchart");

		} 
		else if (!scenes.Contains("LabchartStartScene"))
        {
            _launchManager.GetLoggingManager().AddSensor("Labchart");
            _menuManager.AddToBackOfSceneList("LabchartStartScene");
            _menuManager.AddExperimentParameter("Labchart File Name");
            _launchManager.ExperimentSettings.SensorSettings.Labchart = true;
        }
    }	
}
