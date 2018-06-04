using Assets.EVE.Scripts.Questionnaire;
using UnityEngine;

public class AddQuestionsButtonScript : MonoBehaviour {

	// Use this for initialization
	public void loadQuestions () {
        Transform content = GameObject.Find("SetupDatabaseMenu").transform.Find("Panel").Find("Content");
        UnityEngine.UI.Button questionButton = content.Find("Button (9)").GetComponent<UnityEngine.UI.Button>();
        questionButton.interactable = false;

        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        LoggingManager log = launchManager.GetLoggingManager();
        Assets.EVE.Scripts.Questionnaire.QuestionnaireFactory  qf = new QuestionnaireFactory(log,launchManager.ExperimentSettings);
        qf.ImportAllQuestionnairesFromXml();
        //QuestionnaireBuilder b = new QuestionnaireBuilder(log);
        //b.saveAllQuestionsToDatabase("Assets/EVE/Resources/");
        //b.saveAllQuestionsToDatabase("Assets/Experiment/Resources/");
       
        UnityEngine.UI.Text checkConnection = content.Find("CheckConnection").Find("Response").GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text dbSchema = content.Find("AddDBSchema").Find("Response").GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text questionText = content.Find("AddQuestionText").Find("Response").GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Button setupButton = content.Find("Button (8)").GetComponent<UnityEngine.UI.Button>();        

        checkConnection.text = "MySQL server found";
        dbSchema.text = "<color=#008000ff>Database '" + launchManager.ExperimentSettings.DatabaseSettings.Schema + "' found</color>";
        questionText.text = "<color=#008000ff>Questions found</color>";
        questionButton.interactable = false;
        setupButton.interactable = false;        
    }		
}
