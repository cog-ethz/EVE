using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class StartMenuButtons : MonoBehaviour {

        private LaunchManager _launchManager;

        // Use this for initialization
        void Start () {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            
            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("StartButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                _launchManager.MenuManager.CloseCurrentMenu(0);
                _launchManager.LoadCurrentScene();                
            });            
        }
    }
}
