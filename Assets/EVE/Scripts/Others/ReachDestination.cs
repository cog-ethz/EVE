using UnityEngine;
using UnityEngine.UI;

public class ReachDestination : MonoBehaviour {
    
	private CrossOfElement _destinationlist;
	private int _index;
    private bool _reached;
    private string _destinationName;
    private Text _popUpText;

    // Use this for initialization
    void Start()
    {
        _reached = false;
        _destinationName = this.transform.parent.gameObject.name;
        _popUpText = GameObject.Find("PopUpText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_reached)
        {
            _popUpText.text = "You have reached the " + _destinationName;
            transform.gameObject.SetActive(false);
        }
    }


    void OnTriggerEnter(Collider other){
	    _reached = true;
        if (other.tag == "Player" && _destinationlist != null)
		{
		    _destinationlist.StrikeOff(_index);
		}
	}

	public void SetIndex(int i){
		_index = i;
	}

	public void SetDestinationList(CrossOfElement destinationList){
		_destinationlist = destinationList;
	}

    public bool IsReached()
    {
        return _reached;
    }

    public string getDestinationName()
    {
        if (_destinationName == null)
            _destinationName = this.transform.parent.gameObject.name;
        return _destinationName;
    }
}
