using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Menu;
using UnityEngine;

public class UpdateAttributeList : MonoBehaviour {


    List<string> attributes = new List<string>();
    MenuManager _menuManager = null;
    private LaunchManager _launchManager;

    // Use this for initialization
    void Start()
    {
        _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _menuManager = _launchManager.GetMenuManager();
        //scenes = menumanager.getSceneList();
    }
    // Update is called once per frame
    void Update()
    {
        List<string> currentlist = new List<string>(_menuManager.GetAttributesList());
        if (!MenuManager.ScrambledEquals(attributes, currentlist))
        {
            attributes = new List<string>(currentlist);

            //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
            Transform dynamicFieldT = GameObject.Find("Experiment Config").GetComponent<BaseMenu>().getDynamicFields("DynFieldsA");
            List<GameObject> entriesObjects = new List<GameObject>();
            foreach (Transform entry in dynamicFieldT) entriesObjects.Add(entry.gameObject);
            foreach (GameObject entryObject in entriesObjects) Destroy(entryObject);


            foreach (string sensorName in attributes)
            {
                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextWithoutField")) as GameObject;
                Transform dynamicField = GameObject.Find("Experiment Config").GetComponent<BaseMenu>().getDynamicFields("DynFieldsA");
                filenameObj.transform.SetParent(dynamicField);
                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                Transform te = filenameObj.transform.Find("Text (1)");
                UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
                tex.text = sensorName;
            }

        }
    }
}
