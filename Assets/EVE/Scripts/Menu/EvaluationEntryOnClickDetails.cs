using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluationEntryOnClickDetails : MonoBehaviour {

    public void onClickShowDetails(){
        string sessionIdString=gameObject.transform.parent.Find("Text (1)").GetComponent<UnityEngine.UI.Text>().text;
        int sessionId = int.Parse(sessionIdString);
        //Debug.Log(sessionId);
        GameObject objectContainingMenuManager = GameObject.Find("Canvas");
        var menumanager = objectContainingMenuManager.GetComponent<MenuManager>();
        menumanager.setDetailsInt(sessionId);
        menumanager.ShowMenu(GameObject.Find("Evaluation Details").GetComponent<Menu>());

    }

}
