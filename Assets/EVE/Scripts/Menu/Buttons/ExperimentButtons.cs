using EVE.Scripts.LevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ExperimentButtons : MonoBehaviour {

        private LaunchManager _launchManager;

        public Text SessionId, ExperimentName;
        private MenuManager _menuManager;

        void Awake()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            SetSessionId();
            SetExperimentName();

            var fields = transform.Find("Panel").Find("Fields");
            var input = fields.Find("Config Session").Find("ParticipantId").Find("InputField");
            if (!string.IsNullOrEmpty(_menuManager.ParticipantId))
            {
                input.Find("Placeholder").GetComponent<Text>().text = _menuManager.ParticipantId;
                input.Find("Text").GetComponent<Text>().text = _menuManager.ParticipantId;
            }
            input.GetComponent<InputField>()
                .onEndEdit.AddListener(SetParticipantId);
            fields.Find("SessionParametersButton").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Session Parameters Menu", "Launcher"); });
            fields.Find("StartButton").GetComponent<Button>().onClick.AddListener(() => { _launchManager.StartExperiment(); });
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher"); });
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

        public void SetParticipantId(string subjectId)
        {
            _menuManager.ParticipantId = string.IsNullOrEmpty(subjectId) ? "" : subjectId;
        }
    }
}
