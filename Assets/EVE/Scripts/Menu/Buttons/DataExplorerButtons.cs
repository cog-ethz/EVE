using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace EVE.Scripts.Menu.Buttons
{
    public class DataExplorerButtons : MonoBehaviour
    {
        private LaunchManager _launchManager;
        private MenuManager _menuManager;
        // Start is called before the first frame update
        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            var fields = transform.Find("Panel").Find("Fields");
            
            fields.Find("Back Button").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Evaluation Menu", "Launcher"));
        }
    }
}
