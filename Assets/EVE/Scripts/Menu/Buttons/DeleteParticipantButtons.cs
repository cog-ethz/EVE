using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class DeleteParticipantButtons : MonoBehaviour
    {
        private int _sid=-1;
        private string _pid = "";
        private GameObject _item;
        private LaunchManager _launchManager;
        private MenuManager _menuManager;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("YesButton").GetComponent<Button>().onClick.AddListener(ConfirmDelete);
            fields.Find("NoButton").GetComponent<Button>().onClick.AddListener(()=>_menuManager.InstantiateAndShowMenu("Participants Menu","Launcher"));

            DisplayDeleteQuestion();
        }
        
        public void DisplayDeleteQuestion() {
            _sid = _menuManager.ActiveSessionId;
            _pid = _menuManager.ActiveParticipantId;

            gameObject.transform.Find("Panel").Find("Fields").Find("Participant Details").GetComponent<Text>().text="Session: " + _sid + " Participant: " + _pid;
        }

        public void ConfirmDelete() {
            _launchManager.LoggingManager.removeSession(_sid);
            _launchManager.MenuManager.InstantiateAndShowMenu("Participants Menu", "Launcher");
        }
    }
}
