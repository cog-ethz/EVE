using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Menu;
using UnityEngine;

public class UpdateAttributeForm : MonoBehaviour {


    List<string> attributesF = new List<string>();
    private MenuManager _menuManager;
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
        if (!MenuManager.ScrambledEquals(attributesF, currentlist))
        {
            attributesF = new List<string>(currentlist);

            //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
            GameObject attributeForm = GameObject.Find("Attribute Form");
            if (attributeForm != null)
            {
                Transform dynamicFieldT = attributeForm.GetComponent<BaseMenu>().getDynamicFields("DynFieldsAF");
                List<GameObject> entriesObjects = new List<GameObject>();
                foreach (Transform entry in dynamicFieldT) entriesObjects.Add(entry.gameObject);
                foreach (GameObject entryObject in entriesObjects) Destroy(entryObject);
                
                foreach (string sensorName in attributesF)
                {
                    GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextAndFieldNoXButton")) as GameObject;
                    Transform dynamicField = GameObject.Find("Attribute Form").GetComponent<BaseMenu>().getDynamicFields("DynFieldsAF");
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
}
