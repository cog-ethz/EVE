using UnityEngine;

public class CharacterPlacement : MonoBehaviour {

    private Transform _controller;

    void Start()
    {
        var fpc = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>().FPC;
        fpc.SetActive(true);
        Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        Cursor.visible = false;
        _controller = fpc.transform;
        var aCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _controller.transform.position = this.transform.position;
        _controller.transform.eulerAngles = this.transform.eulerAngles;
        aCamera.transform.eulerAngles = this.transform.eulerAngles;

    }
}
