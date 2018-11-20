using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ExperimentButtons : MonoBehaviour {

        private LaunchManager _launchManager;

        public Text SessionId, ExperimentName;

        void Awake()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            SetSessionId();
            SetExperimentName();
        }
        
        /// <summary>
        /// Starts Experiment.
        /// </summary>
        public void StartExperiment()
        {
            _launchManager.StartExperiment();
        }

        public void SetSessionId()
        {
            SessionId.text = _launchManager.SessionId.ToString();
        }

        public void SetExperimentName()
        {
            ExperimentName.text = _launchManager.ExperimentSettings.Name;
        }
    }
}
