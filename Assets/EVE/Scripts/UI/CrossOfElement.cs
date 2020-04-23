using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using EVE.Scripts.LevelLoader;

public class CrossOfElement : MonoBehaviour {
    
    public GameObject DestinationParent;
    [Tooltip("Maximum duration of scene in minutes")]
    public int MaxDuration;

    private Image[] _strokes;
    private int _nReached = 0;
    private int _maxSecs = 10;
    private FadeOutScene _fader;
    private DateTime _start;
    private GameObject _fpc;
    private bool _once;
    private ReachDestination[] _destinations;

    // Use this for initialization
    void Start () {
	    _maxSecs = MaxDuration * 60;
        _start = DateTime.Now;

        int nChildren = DestinationParent.transform.childCount;
        _destinations = new ReachDestination[nChildren];
        for (int i = 0; i < nChildren; i++)
        {
            ReachDestination destination = DestinationParent.transform.GetChild(i).GetComponentInChildren<ReachDestination>();
            _destinations[i] = destination;
        }

		var index = 0;
		foreach( var destination in _destinations)
		{
		    GameObject uiDestination = Instantiate(Resources.Load("Prefabs/UI_Destination")) as GameObject;
            uiDestination.transform.SetParent(this.transform);
		    uiDestination.name = destination.getDestinationName();
            uiDestination.GetComponentInChildren<Text>().text = destination.getDestinationName();
			destination.SetIndex(index);
			destination.SetDestinationList(this.gameObject.GetComponent<CrossOfElement>());
			index++;
		}
        _strokes = this.gameObject.GetComponentsInChildren<Image>().Where(go => go.gameObject != this.gameObject).ToArray();
        foreach (var stroke in _strokes) {	        
			stroke.enabled = false;
		}

	    var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _fpc = launchManager.FirstPersonController;
        _fader = _fpc.GetComponent<FadeOutScene>();
    }

    void OnGUI()
    {
        if (_nReached == _strokes.Length | (DateTime.Now.Subtract(_start).TotalSeconds > _maxSecs))
        {
            _fader.startFadeOut();
        }

        if (!_fader.isFadedOut()) return;
        if (_once) return;
        _once = true;
        SceneManager.LoadScene("Launcher");

    }


	public void StrikeOff(int index){
		if (index < _strokes.Length) {
			_strokes[index].enabled = true;
            _nReached++;
		} else {
			Debug.LogError("Out of bound index. You are trying to strike off an element that does not exist.\nCheck whether you properly placed all destination elements.");
		}
	}

}