using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class CrossOfElement : MonoBehaviour {

    public ReachDestination[] Destinations;
    public int MaxMin;

    private Image[] _strokes;
	private Text[] _names;
	private int[] _nameLengths;
    private int _nReached = 0;
    private int _maxSecs;
    private FadeOutScene _fader;
    private DateTime _start;
    private GameObject _fpc;
    private bool _once;

    // Use this for initialization
	void Start () {
	    _maxSecs = MaxMin * 60;
        _start = DateTime.Now;
      
        _strokes = this.gameObject.GetComponentsInChildren<Image>().Where(go => go.gameObject != this.gameObject).ToArray();
        _names = this.gameObject.GetComponentsInChildren<Text> ();
		var size = _strokes.Length;
		_nameLengths = new int[ size];
		var index = 0;
		foreach( var destination in Destinations){
			_names[index].text = destination.destinationName;
			_nameLengths[index] = destination.destinationName.Length;
			destination.setIndex(index);
			destination.setDestinationList(this.gameObject.GetComponent<CrossOfElement>());
			index++;
		}
		foreach (var stroke in _strokes) {	        
			stroke.enabled = false;
		}

	    var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _fpc = launchManager.FPC;
        _fader = _fpc.GetComponent<FadeOutScene>();
    }

    void OnGUI()
    {
        
                if (_nReached == _strokes.Length | (DateTime.Now.Subtract(_start).TotalSeconds > _maxSecs))
                {
                    _fader.startFadeOut();
                }

                if (_fader.isFadedOut())
                {
                    if (_once) return;
                    _once = true;
                    var fpc = _fader.gameObject;
                    if (fpc.transform.Find("PositionLogger").GetComponent<ReplayRoute>().isActivated())
                        SceneManager.LoadScene("Evaluation");
                    else
                        SceneManager.LoadScene("Loader");
                }
      
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