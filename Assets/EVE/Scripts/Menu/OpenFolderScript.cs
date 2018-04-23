using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenFolderScript : MonoBehaviour {
    
    public void clickOpenFolder()
    {
        string scenePath = "";
#if UNITY_EDITOR
        scenePath = UnityEditor.EditorUtility.OpenFilePanel("", "D:/git/EVE/Assets/Experiment/Scenes", "unity,xml");
#endif
        //call setscenepath as with the folder textfield
        if (scenePath != null && !scenePath.Equals(""))
        {
            GameObject.Find("Canvas").GetComponent<MenuManager>().SetSceneFilePath(scenePath);
        }
        GameObject.Find("DynFieldsWithScrollbar (1)").GetComponent<AvailableScenesList>().UpdateAvailableScenes();
        GameObject.Find("PathField").GetComponent<InputField>().text = scenePath;
    }

}
