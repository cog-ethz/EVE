using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class MainButtons : MonoBehaviour {
        
        private MenuManager _menuManager;

        void Start()
        {
            _menuManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>().MenuManager;

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("Experiment Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Experiment Menu", "Launcher"); });
            fields.Find("Evaluation Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Evaluation Menu", "Launcher"); });
            fields.Find("Configuration Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Configuration Menu", "Launcher"); });
            fields.Find("Database Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Database Configuration Menu", "Launcher"); });
            fields.Find("Startup Button").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Startup Menu", "Launcher"); });
            fields.Find("Exit Button").GetComponent<Button>().onClick.AddListener(Application.Quit);
        }
    }
}
