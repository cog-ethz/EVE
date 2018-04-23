using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentMenu : Menu {

    private LaunchManager _launchManager;

    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
    }

    public void setCurrentSessionIDOnUI() {
        var sessionID = _launchManager.getCurrentSessionID().ToString();
        GameObject.Find("SessionID").GetComponent<Text>().text = sessionID;
    }

    public void setCurrentExperimentOnUI()
    {
        var currentExperiment = _launchManager.ExperimentSettings.Name;
        GameObject.Find("ExperimentName").GetComponent<Text>().text = currentExperiment;
    }

}
