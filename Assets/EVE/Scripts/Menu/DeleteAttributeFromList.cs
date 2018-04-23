using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAttributeFromList : MonoBehaviour {

    public void deleteAttributeFromList(GameObject button)
    {
        GameObject parentObject = button.transform.parent.gameObject;
        string nameOfEntry = parentObject.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text;
        //Find("Text (1)").GetComponent<UnityEngine.UI.Text>().text;
        GameObject.Find("Canvas").GetComponent<MenuManager>().RemoveExperimentParameter(nameOfEntry);
    }
}
