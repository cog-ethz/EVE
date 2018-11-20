using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ExperimentParametersButtons : MonoBehaviour {

        private MenuManager _menuManager;
        private LaunchManager _launchManager;
        private Transform _dynamicField;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.GetMenuManager();


            _dynamicField = gameObject.transform.Find("Panel").Find("Fields").Find("DynFieldsWithScrollbar").Find("DynFields");

            DisplayExperimentParameters();
        }

        public void DisplayExperimentParameters()
        {
            MenuUtils.ClearList(_dynamicField);

            var experimentParameters = _menuManager.GetExperimentParameterList();


            foreach (var experimentParameter in experimentParameters)
            {
                var gObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/TextWithoutField");
                MenuUtils.PlaceElement(gObject, _dynamicField);
                gObject.transform.Find("FieldName").GetComponent<Text>().text = experimentParameter;
                if (_launchManager.SessionParameters.ContainsKey(experimentParameter))
                    gObject.transform.Find("InputField").Find("Placeholder").GetComponent<Text>().text =
                        _launchManager.SessionParameters[experimentParameter];
            }
        }
        
    }
}
