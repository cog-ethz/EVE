using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableIfEditor : MonoBehaviour {

	// Use this for initialization
	void Awake () {

#if UNITY_EDITOR
	    gameObject.GetComponent<Button>().interactable = true;
#endif
	}
}
