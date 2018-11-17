using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SaveSettingsButton : MonoBehaviour
    {
        private LaunchManager _launchManager;

        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        }

    }
}