using System.Collections.Generic;
using UnityEngine;

public class AddToSceneList : MonoBehaviour
{
    private LaunchManager _launchManager;

    void Start()
    {
        _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
    }

    public void addToSceneList()
    {
        var objectContainingMenuManager=GameObject.Find("Canvas");
        GameObject tex= gameObject.transform.parent.Find("Text (1)").gameObject;
        string filename = tex.GetComponent<UnityEngine.UI.Text>().text;
        string filenameCropped = filename;

        //extract only the name and the ending without the path
        if (filename.Contains("\\"))
        {
            filenameCropped = filename.Split('\\')[filename.Split('\\').Length - 1];
        }
        else
        {
            if (filename.Contains("/"))
            {
                filenameCropped = filename.Split('/')[filename.Split('/').Length - 1];
            }
        }
        if (!filenameCropped.Contains("xml"))
        {
            if (filenameCropped.Contains("."))
            {
                filenameCropped = filenameCropped.Split('.')[0];
            }
        }
        else
        {
            if (_launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires == null)
            {
                _launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires = new List<string>();
            }
            var helper  = filenameCropped.Split('.');
            if (!_launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires.Contains(helper[0]))
            {
                _launchManager.ExperimentSettings.QuestionnaireSettings.Questionnaires.Add(helper[0]);
            }
        }
        objectContainingMenuManager.GetComponent<MenuManager>().AddToBackOfSceneList(filenameCropped);
    }

}
