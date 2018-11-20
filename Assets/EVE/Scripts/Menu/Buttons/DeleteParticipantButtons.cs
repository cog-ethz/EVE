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
            DisplayDeleteQuestion();
        }
        
        public void DisplayDeleteQuestion() {
            _sid = _menuManager.ActiveSessionId;
            _pid = _menuManager.ActiveParticipantId;
            _item = _menuManager.ActiveListItem;

            gameObject.transform.Find("Panel").Find("Fields").Find("Participant Details").GetComponent<Text>().text="Session: " + _sid + " Participant: " + _pid;
        }

        public void ConfirmDelete() {
            Destroy(_item);
            _launchManager.MenuManager.ShowMenu(GameObject.Find("Participants Menu").GetComponent<BaseMenu>());
            _launchManager.LoggingManager.removeSession(_sid);
        }
    }
}
