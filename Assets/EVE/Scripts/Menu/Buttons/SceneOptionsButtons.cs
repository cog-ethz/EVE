using System.Collections;
using System.Collections.Generic;
using Assets.EVE.Scripts.XML.XMLHelper;
using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

public class SceneOptionsButtons : MonoBehaviour {
    private LaunchManager _launchManager;
    private MenuManager _menuManager;

    // Use this for initialization
    void Start()
    {
        _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _menuManager = _launchManager.MenuManager;

        var fields = transform.Find("Panel").Find("Fields");
        fields.Find("SceneName").GetComponent<Text>().text = _menuManager.ActiveSceneId;
        fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Scene Configuration Menu", "Launcher"));
        fields.Find("CurtainButton").Find("Button").GetComponent<Toggle>().onValueChanged.AddListener(UpdateSceneCurtain);

    }

    private void UpdateSceneCurtain(bool enable)
    {
        //TODO Synchronize with database
        var sceneName = _menuManager.ActiveSceneId;
        var index = _launchManager.ExperimentSettings.SceneSettings.Scenes.FindIndex(a => a.Name == sceneName);
        _launchManager.ExperimentSettings.SceneSettings.Scenes[index] = new SceneEntry(sceneName,enable);
    }
}
