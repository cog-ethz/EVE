using UnityEngine;
using System.Collections;

public class RandomCharPlacement : MonoBehaviour
{

    public CharacterController controller;
    public Transform[] placementPositions;
    private LoggingManager log;

    void Start()
    {
        // get Logging Manager
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
         log = launchManager.GetLoggingManager();

        // set random route and log number
        System.Random rnd = new System.Random();
        int r = rnd.Next(0, placementPositions.Length);

        controller.transform.position = placementPositions[r].position;
        controller.transform.rotation = placementPositions[r].rotation;

        log.insertLiveMeasurement("CharacterPlacement", "arrayIndex", null, r.ToString());
    }
}
