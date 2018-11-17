using System.Collections.Generic;
using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SessionParametersButtons : MonoBehaviour {


        List<string> attributesF = new List<string>();
        private MenuManager _menuManager;
        private LaunchManager _launchManager;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.GetMenuManager();

            DisplaySessionParameters();
        }

        public void DisplaySessionParameters()
        {
            var currentlist = new List<string>(_menuManager.GetAttributesList());
            if (!MenuManager.ScrambledEquals(attributesF, currentlist))
            {
                attributesF = new List<string>(currentlist);

                //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
                var attributeForm = GameObject.Find("Attribute Form");
                if (attributeForm != null)
                {
                    var dynamicFieldT = attributeForm.GetComponent<BaseMenu>().getDynamicFields("DynFieldsAF");
                    var entriesObjects = new List<GameObject>();
                    foreach (Transform entry in dynamicFieldT) entriesObjects.Add(entry.gameObject);
                    foreach (var entryObject in entriesObjects) Destroy(entryObject);

                    foreach (var sensorName in attributesF)
                    {
                        var filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextAndFieldNoXButton")) as GameObject;
                        var dynamicField = GameObject.Find("Attribute Form").GetComponent<BaseMenu>().getDynamicFields("DynFieldsAF");
                        filenameObj.transform.SetParent(dynamicField);
                        filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
                        filenameObj.transform.localScale = new Vector3(1, 1, 1);
                        var te = filenameObj.transform.Find("Text (1)");
                        var tex = te.GetComponent<UnityEngine.UI.Text>();
                        tex.text = sensorName;
                    }
                }
            }
        }

    }
}
