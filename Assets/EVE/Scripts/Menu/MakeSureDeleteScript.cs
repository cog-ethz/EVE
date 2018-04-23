using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MakeSureDeleteScript : MonoBehaviour
{
    private int sessionNumber=-1;
    private string participantId = "";
    private GameObject listObjectButton;

    public void setSessionNumber(int sessionNumber)
    {
        this.sessionNumber = sessionNumber;
    }

    public void setParticipantNumber(string participantId)
    {
        this.participantId = participantId;
    }

    public void updateWriting() {
        gameObject.transform.Find("Panel").GetChild(1).gameObject.GetComponent<UnityEngine.UI.Text>().text="Session: "+sessionNumber+" Participant: "+ participantId;
    }

    public void clickYes() {
        Destroy(listObjectButton.transform.parent.gameObject);
        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("Evaluation Menu").GetComponent<Menu>());
        LoggingManager log = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>().GetLoggingManager();
        log.removeSession(sessionNumber);

    }

    internal void setListObjectButton(GameObject button)
    {
        this.listObjectButton = button;
    }
}
