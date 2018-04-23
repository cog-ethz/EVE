using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBackFieldInput : MonoBehaviour {

    private LaunchManager _launchManager;

    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
    }

    public void saveBackFieldInput(string input)
    {
        if (input.Equals("")) return;
        var parentObject = gameObject.transform.parent;
        var fieldName = parentObject.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text;
        _launchManager.changeSessionsParameter(fieldName, input);
    }

}
