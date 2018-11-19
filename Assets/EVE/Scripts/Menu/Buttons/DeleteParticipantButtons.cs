using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class DeleteParticipantButtons : MonoBehaviour
    {
        private int sessionNumber=-1;
        private string participantId = "";
        private GameObject _item;
        private LaunchManager _launchManager;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        }

        public void SetSessionId(int sessionNumber)
        {
            this.sessionNumber = sessionNumber;
        }

        public void SetParticipantId(string participantId)
        {
            this.participantId = participantId;
        }

        public void DisplayDeleteQuestion() {
            gameObject.transform.Find("Panel").Find("Fields").Find("Participant Details").GetComponent<Text>().text="Session: "+sessionNumber+" Participant: "+ participantId;
        }

        public void ConfirmDelete() {
            Destroy(_item);
            _launchManager.GetMenuManager().ShowMenu(GameObject.Find("Participants Menu").GetComponent<BaseMenu>());
            _launchManager.GetLoggingManager().removeSession(sessionNumber);
        }

        internal void SetItem(GameObject item)
        {
            _item = item;
        }
    }
}
