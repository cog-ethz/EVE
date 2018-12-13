using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class AddExperimentParameterButtons : MonoBehaviour {

        private LoggingManager _log;

        private string _experimentParameter;
        private LaunchManager _launchManager;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;


            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("InputField").GetComponent<InputField>().onEndEdit.AddListener(answer => _experimentParameter = answer);
            fields.Find("OkButton").GetComponent<Button>().onClick.AddListener(AddExperimentParameterToDatabase);
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Experiment Parameters Menu", "Launcher"));
        }

        public void AddExperimentParameterToDatabase()
        {
            if (string.IsNullOrEmpty(_experimentParameter))
            {
                _menuManager.DisplayErrorMessage("You must use a valid name for an experiment parameter", "Add Experiment Parameter Menu", "Launcher");
            }
            else
            {
                _menuManager.ExperimentParameterList.Add(_experimentParameter);
                _launchManager.SynchroniseExperimentParametersWithDB();
                _log.CreateExperimentParameter(_launchManager.ExperimentSettings.Name, _experimentParameter);
                _menuManager.InstantiateAndShowMenu("Experiment Parameters Menu", "Launcher");
            }
        }
    }
}
