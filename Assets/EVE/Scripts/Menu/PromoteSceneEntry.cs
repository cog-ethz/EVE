using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromoteSceneEntry : MonoBehaviour {

    public void promoteSceneEntry(GameObject button)
    {
        int numberOfEntry = 0;

        GameObject parentObject = button.transform.parent.gameObject;

        Transform dynamicFieldT = GameObject.Find("Scene Config").GetComponent<Menu>().getDynamicFields("DynFields2");
        List<GameObject> entriesObjects = new List<GameObject>();
        foreach (Transform entry in dynamicFieldT) entriesObjects.Add(entry.gameObject);
        int i = 0;
        foreach (GameObject entryObject in entriesObjects)
        {
            if (entryObject == parentObject)
            {
                numberOfEntry = i;
            }
            i++;
        };

        GameObject.Find("Canvas").GetComponent<MenuManager>().PromoteSceneEntry(numberOfEntry);
    }
}
