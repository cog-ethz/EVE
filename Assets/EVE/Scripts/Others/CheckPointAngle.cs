using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;


public class CheckPointAngle : MonoBehaviour {

    public string[] Names = { "Home", "Church", "Park"};
    public string[] Order = {"012", "201", "120"};

    public TextMesh FrontText;

    public Transform Target;
    public FadeOutScene Fader;
    public Text InstructionTextLabel;
    public bool RandomizeOrder;

    private bool _fadeOut, _first = true;
  
    private static readonly System.Random Rng = new System.Random();
    private int[] _usedOrder;

    private int _counter;
    private string _goalText, _posText, _lookText;

    private ReplayRoute _rpl;
    private JRD_FirstPersonController _fpc;
    private LoggingManager _log;
    private bool _once;


    // Use this for initialization
    void Start ()
    {
        var lauchManagerObject = GameObject.FindGameObjectWithTag("LaunchManager");
        if (lauchManagerObject != null)
        {
            var launchManager = lauchManagerObject.GetComponent<LaunchManager>();
            _log = launchManager.LoggingManager;
            _rpl = launchManager.FPC.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
        }
        else
        {
            Debug.LogError("LaunchManager not found");
        }

        _fpc = this.GetComponent<JRD_FirstPersonController>();

        // Randomize order if needed
        _usedOrder = RandomizeOrder?CreateRandomOrder(Order.Length): Enumerable.Range(0, Order.Length).ToArray(); 
        
	}

    private static int[] CreateRandomOrder(int count)
    {
        var n = count;
        int[] list = new int[n];
        for (var i = 0; i < n; i++)
        {
            list[i] = i;
        }
        while (n > 1)
        {
            n--;
            var k = Rng.Next(n + 1);
            var v = list[k];
            list[k] = list[n];
            list[n] = v;
        }
        return list;
    }
	
	// Update is called once per frame
	void Update () {
        if (!_fadeOut)
        {
            var locations = "";

            locations = Order[_usedOrder[_counter]];

            _goalText = Names[int.Parse(locations.Substring(2, 1))];
            _posText = Names[int.Parse(locations.Substring(0, 1))];
            _lookText = Names[int.Parse(locations.Substring(1, 1))];
            FrontText.text = _lookText;
            InstructionTextLabel.text = "Imagine you are standing at the " + _posText + ", facing the " + _lookText + ". Point to the " + _goalText + ".";
            if (!_first)
            {
                InstructionTextLabel.text += "\n\nUse the \"Enter\" key to confirm or the \"Escape\" key to change";
            }

            if (Input.GetButtonDown("Back"))
            {
                if (_first)
                {
                    _fpc.enabled = false;
                    _first = false;
                }
                else
                {
                    if (_log != null)
                    {
                        var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                        _log.insertMeasurement("JRD", locations, "Degree", (Target.eulerAngles.y).ToString(), timestamp);
                    }             

                    Target.eulerAngles.Set(0, 0, 0);
                    _fpc.transform.rotation = Quaternion.Euler(0,0,0);
                    _fpc.ResetRotation();
                    _counter++;
                    if (_counter < Order.Length)
                    {                        
                        _first = true;
                        _fpc.enabled = true;
                    }
                    else
                    {
                        _fadeOut = true;
                    }
                }
            }
            if (Input.GetButtonDown("Cancel") && !_first)
            {
                _first = true;
                _fpc.enabled = true;
            }
        }
        if (Fader.isFadedOut())
        {
            SceneManager.LoadScene("Loader");          
        }
	}

    void OnGUI()
    {
        if (_fadeOut)
           Fader.startFadeOut();
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            if (_once) return;
            _once = true;
            SceneManager.LoadScene("Loader");
        }

    }
}
