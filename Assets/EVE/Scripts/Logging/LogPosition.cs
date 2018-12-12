using UnityEngine;
using UnityEngine.SceneManagement;

public class LogPosition : MonoBehaviour
{

    // This script will log the position of the player as long as he is in this scene.

    public float timestep_in_sec;
    public GameObject player;

    //private int n;	// incremeant with the logs of position
    private LoggingManager log;

    private bool repeating = false;
    

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.LoggingManager;
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
        }
    }

    void Update()
    {
        if (this.gameObject.transform.parent.gameObject.activeSelf && !repeating && !player.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            repeating = true;
            InvokeRepeating("LogPositionAndViewDirection", 0, timestep_in_sec);
        }
    }

    private void LogPositionAndViewDirection()
    {
        if (this.gameObject.transform.parent.gameObject.activeSelf && !player.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            //Position
            float x = player.transform.position.x;
            float y = player.transform.position.y;
            float z = player.transform.position.z;

            //View angles
            float ex = player.transform.eulerAngles.x;
            float ey = player.transform.eulerAngles.y;
            float ez = player.transform.eulerAngles.z;

            if (log != null)
                log.LogPositionAndView(x, y, z, ex, ey, ez);

        } else
        {
            repeating = false;
            CancelInvoke();
        }
    }
}
