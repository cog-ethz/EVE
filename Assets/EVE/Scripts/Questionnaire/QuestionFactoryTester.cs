using UnityEngine;
using System.Collections;

public class QuestionFactoryTester : MonoBehaviour {

	public string[] fileNames = new string[3]{"Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/vr_perception.xml",
		"Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/social_demographics.xml",
		"Assets/SpatialExperimentFramework/K_scripts/Questionnaire/Resources/Questionnaires/vr_experience.xml"};

	private LoggingManager log;
	public GUISkin skin;

	// Use this for initialization
	void Start () {
		log = new LoggingManager ();
		QuestionnaireFactory qf = new QuestionnaireFactory (fileNames,log);
        Debug.Log(qf.ToString());
	}
	
	// Update is called once per frame
	void Update () {
	  
	}
}
