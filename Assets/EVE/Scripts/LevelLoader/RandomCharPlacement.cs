using UnityEngine;
using System.Collections;

public class RandomCharPlacement : MonoBehaviour
{
    public Transform[] PlacementPositions;

    void Start()
    {
        var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        var fpc = launchManager.FPC;
        fpc.SetActive(true);
        Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        Cursor.visible = false;
        var controller = fpc.transform;
        var aCamera = fpc.GetComponentInChildren<Camera>(); ;
        // set random route and log number
        System.Random rnd = new System.Random();
        int r = rnd.Next(0, PlacementPositions.Length);

        controller.transform.position = PlacementPositions[r].position;
        controller.transform.eulerAngles = PlacementPositions[r].eulerAngles;
        aCamera.transform.eulerAngles = PlacementPositions[r].eulerAngles;

        if (!fpc.GetComponentInChildren<ReplayRoute>().isActivated())
            launchManager.LoggingManager.insertLiveMeasurement("CharacterPlacement", "arrayIndex", null, r.ToString());
    }
}
