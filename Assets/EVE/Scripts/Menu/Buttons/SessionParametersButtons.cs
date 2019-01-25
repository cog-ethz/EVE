using System.Collections.Generic;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class SessionParametersButtons : MonoBehaviour {

        private MenuManager _menuManager;
        private LaunchManager _launchManager;
        private Transform _dynamicField;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;


            _dynamicField = gameObject.transform.Find("Panel").Find("Fields").Find("DynFieldsWithScrollbar").Find("DynFields");

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("StartButton").GetComponent<Button>().onClick.AddListener(() => { _launchManager.StartExperiment(); });
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Experiment Menu", "Launcher"); });

            DisplaySessionParameters();
        }

        public void DisplaySessionParameters()
        {
            MenuUtils.ClearList(_dynamicField);

            var experimentParameters = _menuManager.ExperimentParameterList;


            foreach (var experimentParameter in experimentParameters)
            {
                var gObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/EditableSessionParameter");
                gObject.GetComponentInChildren<InputField>().onEndEdit.AddListener((text) =>
                {
                    StoreSessionParameter(experimentParameter,text);
                });
                MenuUtils.PlaceElement(gObject, _dynamicField);
                gObject.transform.Find("FieldName").GetComponent<Text>().text = experimentParameter;
                if (_launchManager.SessionParameters.ContainsKey(experimentParameter))
                    gObject.transform.Find("InputField").Find("Placeholder").GetComponent<Text>().text =
                        _launchManager.SessionParameters[experimentParameter];
            }
        }

        public void StoreSessionParameter(string sessionParameter,string value)
        {
            if (value.Equals(""))
            {
                Debug.LogWarning("Empty input for session parameter " + sessionParameter);
                return;
            }
            _launchManager.ChangeSessionsParameter(sessionParameter, value);
        }
    }
}
