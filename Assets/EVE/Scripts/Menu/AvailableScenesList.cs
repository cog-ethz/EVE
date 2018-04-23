using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AvailableScenesList : MonoBehaviour {
    private LaunchManager _launchManager;


    // Use this for initialization
	void Start ()
    {
        _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        UpdateAvailableScenes();
	}

    public void UpdateAvailableScenes()
    {
        var path = _launchManager.GetMenuManager().GetSceneFilePath();
        var filenames = Directory.GetFiles(path);
        foreach (var filename in filenames)
        {

            //this block is a filter which filters files by the ".unity" or ".xml"-ending(questionnaires)
            var splittedFilename = filename.Split('.');
            if (!splittedFilename[splittedFilename.Length - 1].Equals("unity") && !splittedFilename[splittedFilename.Length - 1].Equals("xml"))
            {
                continue;
            }

            //this block adds the data to the menu
            var filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextAndAddButton")) as GameObject;
            var dynamicField = GameObject.Find("Scene Config").GetComponent<Menu>().getDynamicFields("DynFields");
            filenameObj.transform.SetParent(dynamicField);
            filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
            filenameObj.transform.localScale = new Vector3(1, 1, 1);
            var te = filenameObj.transform.Find("Text (1)");
            UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
            var fileComponents = filename.Split('/');
            tex.text = fileComponents[fileComponents.Length - 1];
        }
    }
	
}

