using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.EVE.Scripts.Menu;
using UnityEngine;
using UnityEngine.UI;

public class ChosenScenesList : MonoBehaviour {

    private List<string> _scenes;
    private LaunchManager _launchManager;

    // Use this for initialization
    void Start () {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _scenes = new List<string>();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    _launchManager.SynchroniseSceneListWithDB();
        var currentlist = new List<string>(_launchManager.ExperimentSettings.SceneSettings.Scenes);
	    if (_scenes.SequenceEqual(currentlist)) return;

	    _scenes = new List<string>(currentlist);

	    //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
	    var dynamicFieldT = GameObject.Find("Scene Config").GetComponent<BaseMenu>().getDynamicFields("DynFields2");
	    var entriesObjects = (from Transform entry in dynamicFieldT select entry.gameObject).ToList();
	    foreach (var entryObject in entriesObjects) Destroy(entryObject);


	    foreach (var filename in _scenes)
	    {
	        var filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextAndButtons")) as GameObject;
	        var dynamicField = GameObject.Find("Scene Config").GetComponent<BaseMenu>().getDynamicFields("DynFields2");
	        filenameObj.transform.SetParent(dynamicField);
	        filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
	        filenameObj.transform.localScale = new Vector3(1, 1, 1);
	        var te = filenameObj.transform.Find("Text (1)");
	        var tex = te.GetComponent<Text>();
	        var filenameCropped = filename;

	        //extract only the name and the ending without the path
	        if (filename.Contains("\\"))
	        {
	            filenameCropped = filename.Split('\\')[filename.Split('\\').Length-1];
	        }
	        else  if (filename.Contains("/"))
	        {
	            filenameCropped = filename.Split('/')[filename.Split('/').Length - 1];
	        }
	        tex.text = filenameCropped;
	    }
	}
}
