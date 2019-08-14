using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

public class CheckJRD : MonoBehaviour {

    public GameObject ImageA, ImageB, ImageC;
    public JRDImageRemover JrdImageRemover;
    //public Sprite[] BuildingsA, BuildingsB, BuildingsC;
    public Sprite[] Landmarks;
    
    public string[] Order = {"012", "201", "120"};

    public bool RandomizeOrder;
    
    
    public int MaxRepetitions = -1;

    private GameObject[] _boxes;
    private int _imageCounter;
    private bool _answered, _loading;
    private static readonly System.Random Rng = new System.Random();
    
    private int[] _usedOrder;
    private Sprite _goalDir, _inFrontOf, _lookAt;

    private int _counter;

    private LoggingManager _log;

    // Use this for initialization
    void Start ()
    {
        var launchManagerObject = GameObject.FindWithTag("LaunchManager");
        if (launchManagerObject != null)
        {
            var launchManager = launchManagerObject.GetComponent<LaunchManager>();
            _log = launchManager.LoggingManager;
        }
        else
        {
            Debug.LogError("LaunchManager not found");
        }

        var jrdBoxes = GameObject.Find("JRDBoxes");
        _boxes = new GameObject[7];
        for(int i = 0; i < 7; i++)
        {
            int num = i + 1;
            _boxes[i] = jrdBoxes.transform.Find($"Drop Box ({num})").gameObject;
        }
        
        _usedOrder = RandomizeOrder?CreateRandomOrder(Order.Length): Enumerable.Range(0, Order.Length).ToArray();
        
        var locations = "";
        locations = Order[_usedOrder[_imageCounter]];

        _goalDir = Landmarks[int.Parse(locations.Substring(2, 1))];
        _inFrontOf = Landmarks[int.Parse(locations.Substring(0, 1))];
        _lookAt = Landmarks[int.Parse(locations.Substring(1, 1))];
        ImageA.GetComponent<UnityEngine.UI.Image>().sprite = _inFrontOf;
        ImageB.GetComponent<UnityEngine.UI.Image>().sprite = _lookAt;
        ImageC.GetComponent<UnityEngine.UI.Image>().sprite = _goalDir;
        
        MaxRepetitions = MaxRepetitions < 0 ? Order.Length : MaxRepetitions;
    }

    void OnGUI()
    {
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Launcher");
        }
    }

    public void CheckSolution()
    {
        for (var i = 0; i < 7; i++)
        {
            var boxContent = _boxes[i].transform.Find("Drop Image").GetComponent<UnityEngine.UI.Image>().overrideSprite;
            if (boxContent != null)
            {
                //record to database
                if (_log != null)
                {
                    _log.InsertLiveMeasurement("JRD", _inFrontOf.name + "," + _lookAt.name + "," + _goalDir.name, "", (i + 1).ToString());
                }               
                _answered = true;
            }
                
        }
        if (_answered)
        {
            if (JrdImageRemover != null)
                JrdImageRemover.resetImages();
            _answered = false;
            _imageCounter++;
            if (_imageCounter < MaxRepetitions)
            {
                var locations = "";
                locations = Order[_usedOrder[_imageCounter]];

                _goalDir = Landmarks[int.Parse(locations.Substring(2, 1))];
                _inFrontOf = Landmarks[int.Parse(locations.Substring(0, 1))];
                _lookAt = Landmarks[int.Parse(locations.Substring(1, 1))];
                ImageA.GetComponent<UnityEngine.UI.Image>().sprite = _inFrontOf;
                ImageB.GetComponent<UnityEngine.UI.Image>().sprite = _lookAt;
                ImageC.GetComponent<UnityEngine.UI.Image>().sprite = _goalDir;
            }
            else
            {
                if (!_loading)
                {
                    _loading = true;
                    SceneManager.LoadScene("Launcher");
                }
            }
        }
        
        
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
}
