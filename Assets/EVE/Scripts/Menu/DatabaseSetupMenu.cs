using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseSetupMenu : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        setup();
    }

    // Update is called once per frame
    public void setup()
    {
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        LoggingManager log = launchManager.GetLoggingManager();
        Transform content = GameObject.Find("SetupDatabaseMenu").transform.Find("Panel").Find("Content");
        UnityEngine.UI.Text checkConnection = content.Find("CheckConnection").Find("Response").GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text dbSchema = content.Find("AddDBSchema").Find("Response").GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text questionText = content.Find("AddQuestionText").Find("Response").GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Button setupButton = content.Find("Button (8)").GetComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.Button questionButton = content.Find("Button (9)").GetComponent<UnityEngine.UI.Button>();
        int currentSessionId = log.GetCurrentSessionID();
        switch (currentSessionId)
        {

            case -2:
                checkConnection.text = "<color=#ff0000ff>MySQL server not found</color>";
                dbSchema.text = "";
                questionText.text = "";
                break;
            case -3:
                checkConnection.text = "<color=#ff0000ff>Invalid credentials</color>";
                dbSchema.text = "";
                questionText.text = "";
                break;
            case -4:
                checkConnection.text = "MySQL server found";
                dbSchema.text = "<color=#ff0000ff>Database '" + launchManager.ExperimentSettings.DatabaseSettings.Schema + "' not found</color>";
                questionText.text = "";
                setupButton.interactable = true;
                break;
            default:
                checkConnection.text = "MySQL server found";
                dbSchema.text = "<color=#008000ff>Database '" + launchManager.ExperimentSettings.DatabaseSettings.Schema + "' found</color>";                
                setupButton.interactable = false;
                if (!log.checkQuestionnaireExists("neighborhood_walk"))
                {
                    questionText.text = "<color=#ff0000ff>Questions not found</color>";
                    questionButton.interactable = true;
                } else
                {
                    questionText.text = "<color=#008000ff>Questions found</color>";
                    questionButton.interactable = false;
                }
                
                break;
        }
    }
}
