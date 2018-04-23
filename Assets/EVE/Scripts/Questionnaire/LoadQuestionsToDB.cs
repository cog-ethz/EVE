using UnityEngine;
using System.Collections;
using Assets.EVE.Scripts.XML;

public class LoadQuestionsToDB : MonoBehaviour {

    public string databaseServer, databaseSchema, databaseUser, databasePassword;

    public DatabaseSettings DatabaseSettings;

    public string[] additionalQuestions;

    void Start () {
		LoggingManager log = new LoggingManager ();
        log.ConnectToServer(DatabaseSettings);
        QuestionnaireBuilder b = new QuestionnaireBuilder (log);
        //b.saveAllQuestionsToDatabase("Assets/EVE/Scripts/Questionnaire/Resources/");
        foreach (var path in additionalQuestions)
        {
            if (path.EndsWith("/"))
            {
                b.saveAllQuestionsToDatabase(path);
            }
            else
            {
                b.saveAllQuestionsToDatabase(path+"/");
            }
        }
	}
}
