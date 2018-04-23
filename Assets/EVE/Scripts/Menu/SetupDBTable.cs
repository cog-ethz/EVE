using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupDBTable : MonoBehaviour {

	public void createDBschema()
    {
        var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        var log = launchManager.GetLoggingManager();
        var dbSettings = launchManager.ExperimentSettings.DatabaseSettings;
        log.ConnectToServerAndCreateSchema(dbSettings);

        var content = GameObject.Find("SetupDatabaseMenu").transform.Find("Panel").Find("Content");
        var checkConnection = content.Find("CheckConnection").Find("Response").GetComponent<Text>();
        var dbSchema = content.Find("AddDBSchema").Find("Response").GetComponent<Text>();
        var questionText = content.Find("AddQuestionText").Find("Response").GetComponent<Text>();
        var setupButton = content.Find("Button (8)").GetComponent<Button>();

        checkConnection.text = "MySQL server found";
        dbSchema.text = "<color=#008000ff>Database '" + launchManager.ExperimentSettings.DatabaseSettings.Schema + "' found</color>";
        questionText.text = "";
        setupButton.interactable = false;
        var questionButton = content.Find("Button (9)").GetComponent<Button>();
        questionButton.interactable = true;
        log.LogExperiment(launchManager.GetExperimentName());
        launchManager.setSessionId(log.GetCurrentSessionID());
    }
}
