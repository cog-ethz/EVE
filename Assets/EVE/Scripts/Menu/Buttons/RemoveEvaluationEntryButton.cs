using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class RemoveEvaluationEntryButton : MonoBehaviour {

        public void RemoveEvaluationEntry(GameObject item)
        {
            string sessionNumber = item.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text;
            string participantNumber = item.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Text>().text;
            //Find("Text (1)").GetComponent<UnityEngine.UI.Text>().text;
            GameObject.Find("Delete Participant Menu").GetComponent<DeleteParticipantButtons>().SetSessionId(int.Parse(sessionNumber));
            GameObject.Find("Delete Participant Menu").GetComponent<DeleteParticipantButtons>().SetParticipantId(participantNumber);
            GameObject.Find("Delete Participant Menu").GetComponent<DeleteParticipantButtons>().SetItem(item);
            GameObject.Find("Delete Participant Menu").GetComponent<DeleteParticipantButtons>().DisplayDeleteQuestion();


            GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(GameObject.Find("Delete Participant Menu").GetComponent<BaseMenu>());
        }
    }
}
