using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace EVE.Scripts.Menu.Buttons
{
    public class StartUpButtons : MonoBehaviour
    {
        // Start is called before the first frame update
        private MenuManager _menuManager;

        void Start()
        {
            _menuManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>().MenuManager;

            var fields = transform.Find("Panel").Find("Fields").Find("Tiles");
            fields.Find("Experiment Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher"); });
            fields.Find("Evaluation Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Data Explorer Menu", "Launcher"); });
        }
    }
}
