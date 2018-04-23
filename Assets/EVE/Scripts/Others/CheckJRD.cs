using UnityEngine.SceneManagement;
using UnityEngine;

public class CheckJRD : MonoBehaviour {

    public GameObject ImageA, ImageB;
    public JRDImageRemover JrdImageRemover;
    public Sprite[] BuildingsA, BuildingsB;

    private GameObject[] _boxes;
    private int _imageCounter;
    private bool _answered, _loading;
    

    private LoggingManager _log;

    // Use this for initialization
    void Start ()
    {
        var launchManagerObject = GameObject.FindWithTag("LaunchManager");
        if (launchManagerObject != null)
        {
            var launchManager = launchManagerObject.GetComponent<LaunchManager>();
            _log = launchManager.GetLoggingManager();
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
            _boxes[i] = jrdBoxes.transform.Find("Drop Box (" + num + ")").gameObject;
        }
        ImageA.GetComponent<UnityEngine.UI.Image>().sprite = BuildingsA[0];
        ImageB.GetComponent<UnityEngine.UI.Image>().sprite = BuildingsB[0];
    }

    void OnGUI()
    {
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
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
                    _log.insertLiveMeasurement("JRD", BuildingsA[_imageCounter].name + "," + BuildingsB[_imageCounter].name, "", (i + 1).ToString());
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
            if (_imageCounter < BuildingsA.Length)
            {
                ImageA.GetComponent<UnityEngine.UI.Image>().sprite = BuildingsA[_imageCounter];
                ImageB.GetComponent<UnityEngine.UI.Image>().sprite = BuildingsB[_imageCounter];
            }
            else
            {
                if (!_loading)
                {
                    _loading = true;
                    SceneManager.LoadScene("Loader");
                }
            }
        }
        
        
    }
}
