using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace EVE.Scripts.Menu.Buttons
{
    public class EvaluationButtons : MonoBehaviour {

        private LaunchManager _launchManager;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start ()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("Data Explorer Button").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Data Explorer Menu", "Launcher"));
            fields.Find("Participants Button").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Participants Menu", "Launcher"));
            fields.Find("Back Button").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher"));
        }
    }
}
