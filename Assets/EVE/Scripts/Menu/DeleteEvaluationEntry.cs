using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteEvaluationEntry : MonoBehaviour {

    public void deleteEvaluationEntry(GameObject button)
    {
        GameObject parentObject = button.transform.parent.gameObject;
        string sessionNumber = parentObject.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text;
        string participantNumber = parentObject.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Text>().text;
        //Find("Text (1)").GetComponent<UnityEngine.UI.Text>().text;
        GameObject.Find("MakeSureDelete").GetComponent<MakeSureDeleteScript>().setSessionNumber(int.Parse(sessionNumber));
        GameObject.Find("MakeSureDelete").GetComponent<MakeSureDeleteScript>().setParticipantNumber(participantNumber);
        GameObject.Find("MakeSureDelete").GetComponent<MakeSureDeleteScript>().setListObjectButton(button);
        GameObject.Find("MakeSureDelete").GetComponent<MakeSureDeleteScript>().updateWriting();


        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("MakeSureDelete").GetComponent<Menu>());
    }
}
