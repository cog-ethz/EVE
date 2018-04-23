using UnityEngine;
using System.Collections;

public class QuestionnaireFactoryTester : MonoBehaviour {

	//public string[] fileNames = new string[3]{"Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/vr_perception.xml",
	//	"Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/social_demographics.xml",
	//	"Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/vr_experience.xml"};

    public string[] fileNames = new string[1] { "Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/test.xml" };

	public LoggingManager log;
	public GUISkin skin;

    public GUISkin centerAlignedskin;
    public GUISkin leftAlignedSkin;

	// Use this for initialization
	void Start () {
		log = new LoggingManager ();
        // First load the questionnaire
		QuestionnaireFactory qf = new QuestionnaireFactory (fileNames,log);
        // Use factory to load appropriate sets from DB
        //qf.prepareQuestionSet();
        qf.storeAllQuestionnairesInDB();
        // Use the Questionnaire builder to create the collection (return collection)
        Debug.Log("Factory tested");
	}
	
	// Update is called once per frame
	void Update () {
	  
	}
}
