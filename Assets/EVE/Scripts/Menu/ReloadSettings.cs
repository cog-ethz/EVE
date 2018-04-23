using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadSettings : MonoBehaviour {

    private LaunchManager _launchManager;

    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
    }

    /// <summary>
    /// Saves the experiment settings to a user selected location.
    /// </summary>
    public void ClickReloadSettings()
    {
        _launchManager.ReadExperimentSettings();
    }
}
