using UnityEngine;
using System.Collections;

public class PopupQuestionnaireManager : MonoBehaviour {

	void Start () {
		// find all PopUpQuestionnaires
		GameObject[] questionnaires = GameObject.FindGameObjectsWithTag ("PopupQuestionnaire");

		// find highest used Questionnaire number in Database
		//GameObject cam = GameObject.FindGameObjectWithTag ("MainCamera");
		//TakeGlobalParameters t = cam.GetComponent<TakeGlobalParameters> ();
		//LoggingManager log = t.log;

		//LoggingManager log = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<TakeGlobalParameters> ().log;
		
        //int id = 10;
		//if (log != null)
		//	id = Mathf.Max(id, log.getNextPopupQuestionnaireID ());

		// assign each one a unique questionnaire number
		for (int i = 0; i < questionnaires.Length; i++) {
			PopupQuestionnaire q = questionnaires[i].GetComponent<PopupQuestionnaire>();
			//q.questionnaireID = id;
			q.readyToUseParams = true;

			//id++;
		}
	}
}
