using System.Linq;
using Assets.EVE.Scripts.Questionnaire;
using Assets.EVE.Scripts.XML;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    /// <summary>
    /// Buttons related to manage the database.
    /// </summary>
    public class DatabaseButtons : MonoBehaviour {
        private LaunchManager _launchManager;
        private LoggingManager _log;
        private DatabaseSettings _dbSettings;
        private Text _checkConnection;
        private Text _dbSchema;
        private Text _questionText;
        private Button _setupButton;
        private Button _questionButton;
        private Button _resetButton;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start () {
            
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;
            _dbSettings = _launchManager.ExperimentSettings.DatabaseSettings;

            var content = gameObject.transform.Find("Panel").Find("Content");
            _checkConnection = content.Find("ConnectionState").Find("Response").GetComponent<Text>();
            _dbSchema = content.Find("SchemaState").Find("Response").GetComponent<Text>();
            _questionText = content.Find("QuestionnaireState").Find("Response").GetComponent<Text>();
            _setupButton = content.Find("TableButtons").Find("SetupDatabaseTables").GetComponent<Button>();
            _resetButton = content.Find("TableButtons").Find("ResetDatabaseTables").GetComponent<Button>();
            _questionButton = content.Find("SetupQuestionnaires").GetComponent<Button>();

            _setupButton.onClick.AddListener(CreateDbSchema);
            _resetButton.onClick.AddListener(ResetDbSchema);
            _questionButton.onClick.AddListener(LoadQuestionnairesFromXML);
            content.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher"));

            CheckDatabase();
        }

        private void CheckDatabase()
        {
            var currentSessionId = _log.CurrentSessionID;
            switch (currentSessionId)
            {

                case -2:
                    _checkConnection.text = "<color=#ff0000ff>MySQL server not found</color>";
                    _dbSchema.text = "";
                    _questionText.text = "";
                    break;
                case -3:
                    _checkConnection.text = "<color=#ff0000ff>Invalid credentials</color>";
                    _dbSchema.text = "";
                    _questionText.text = "";
                    break;
                case -4:
                    _checkConnection.text = "MySQL server found";
                    _dbSchema.text = "<color=#ff0000ff>Database '" + _dbSettings.Schema + "' not found</color>";
                    _questionText.text = "";
                    _setupButton.interactable = true;
                    break;
                default:
                    _checkConnection.text = "MySQL server found";
                    _dbSchema.text = "<color=#008000ff>Database '" + _dbSettings.Schema + "' found</color>";
                    _setupButton.interactable = false;
                    if (_log.GetQuestionnaireNames().Any())
                    {
                        _questionText.text = "<color=#008000ff>Questions found (see log)</color>";
                        Debug.Log("Existing Questionnaires");
                        _log.GetQuestionnaireNames().ForEach(Debug.Log);
                        _questionButton.interactable = false;
                    }
                    else
                    {
                        _questionText.text = "<color=#ff0000ff>Questions not found</color>";
                        _questionButton.interactable = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates the standard EVE database schema with the name specified in the experiment settings.
        /// </summary>
        public void CreateDbSchema()
        {
            _log.ConnectToServerAndCreateSchema(_dbSettings);
            _log.LogExperiment(_launchManager.ExperimentName);
            _launchManager.SessionId = _log.CurrentSessionID;
            _launchManager.LoadSettingsIntoDB();
            CheckDatabase();
        }

        /// <summary>
        /// Drops the database and resets it completely.
        /// </summary>
        public void ResetDbSchema()
        {
            _launchManager.MenuManager.InstantiateAndShowMenu("Delete Database Menu","Launcher");
        }

        /// <summary>
        /// Loads the questionnaires specified in the experiment settings.
        /// </summary>
        public void LoadQuestionnairesFromXML()
        {
            var qf = new QuestionnaireFactory(_log, _launchManager.ExperimentSettings);
            qf.ImportAllQuestionnairesFromXml();
            CheckDatabase();
        }
    }
}