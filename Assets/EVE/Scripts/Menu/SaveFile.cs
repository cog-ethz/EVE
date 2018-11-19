using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFile : MonoBehaviour
{
    private LaunchManager _launchManager;

    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
    }

    /// <summary>
    /// Saves the experiment settings to a user selected location.
    /// </summary>
    public void ClickSaveFile()
    {
        var path = Application.persistentDataPath;
#if UNITY_EDITOR
        path = UnityEditor.EditorUtility.SaveFilePanel("Save Experiment Settings", Application.dataPath + "/Experiment/Resources", "experiment_settings", "xml");
#endif
        _launchManager.WriteExperimentSettings(path);
    }
}