using EVE.Scripts.LevelLoader;
using UnityEngine;

public class CharacterPlacement : MonoBehaviour {

    private Transform _controller;

    void Start()
    {
        var fpc = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>().FirstPersonController;        
        Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        Cursor.visible = false;
        _controller = fpc.transform;
        var aCamera = fpc.GetComponentInChildren<Camera>();
        _controller.transform.position = this.transform.position;
        _controller.transform.eulerAngles = this.transform.eulerAngles;
        aCamera.transform.eulerAngles = this.transform.eulerAngles;
        fpc.SetActive(true);

    }
}
