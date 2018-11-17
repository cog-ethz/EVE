using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class RemoveListItemButton : MonoBehaviour {

        private LaunchManager _launchManager;
        
        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        }

        public void RemoveExperimentParameter(GameObject item)
        {
            var nameOfEntry = item.transform.Find("FieldName").GetComponent<Text>().text;
            _launchManager.GetMenuManager().RemoveExperimentParameter(nameOfEntry);
            Destroy(item);
        }
    }
}
