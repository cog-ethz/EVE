using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu
{
    public class FieldInput : MonoBehaviour {

        private LaunchManager _launchManager;

        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        }

        public void StoreSessionParameter(string input)
        {
            if (input.Equals("")) return;
            var parentObject = gameObject.transform.parent;
            var fieldName = parentObject.GetChild(0).gameObject.GetComponent<Text>().text;
            _launchManager.changeSessionsParameter(fieldName, input);
        }

    }
}
