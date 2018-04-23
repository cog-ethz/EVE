using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteSensorFromList : MonoBehaviour {

    public void deleteSensorFromList(GameObject button)
    {
        GameObject parentObject=button.transform.parent.gameObject;
        string nameOfEntry = parentObject.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text;
            //Find("Text (1)").GetComponent<UnityEngine.UI.Text>().text;
        GameObject.Find("Canvas").GetComponent<MenuManager>().RemoveSensor(nameOfEntry);
    }

}
