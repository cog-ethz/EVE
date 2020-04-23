using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class FinishMenuButtons : MonoBehaviour {

        private LaunchManager _launchManager;

        // Use this for initialization
        void Start () {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            
            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("OkButton").GetComponent<Button>().onClick.AddListener(() => _launchManager.MenuManager.InstantiateAndShowMenu("Main Menu","Launcher"));
        }
    }
}
