using Assets.EVE.Scripts.Utils;
using EVE.Scripts.LevelLoader;
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
            _menuManager = _launchManager.MenuManager;



            var fields = transform.Find("Panel").Find("Fields");
            _dynamicField = fields.Find("DynFieldsWithScrollbar").Find("DynFields");
            fields.Find("AddButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Add Experiment Parameter Menu", "Launcher"));
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Configuration Menu", "Launcher"));
            DisplayExperimentParameters();
        }

        public void DisplayExperimentParameters()
        {
            MenuUtils.ClearList(_dynamicField);

            var experimentParameters = _menuManager.ExperimentParameterList;


            foreach (var experimentParameter in experimentParameters)
            {
                var gObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/ExperimentParameterEntry");
                MenuUtils.PlaceElement(gObject, _dynamicField);
                gObject.transform.Find("FieldName").GetComponent<Text>().text = experimentParameter;
                if (_launchManager.SessionParameters.ContainsKey(experimentParameter))
                    gObject.transform.Find("InputField").Find("Placeholder").GetComponent<Text>().text =
                        _launchManager.SessionParameters[experimentParameter];

                gObject.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    RemoveExperimentParameter(gObject);

                });
            }
        }

        public void RemoveExperimentParameter(GameObject item)
        {
            var nameOfEntry = item.transform.Find("FieldName").GetComponent<Text>().text;
            _launchManager.MenuManager.RemoveExperimentParameter(nameOfEntry);
            Destroy(item);
        }

    }
}
