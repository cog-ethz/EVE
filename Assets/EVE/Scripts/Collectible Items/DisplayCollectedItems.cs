using UnityEngine;
using System.Collections;

public class DisplayCollectedItems : MonoBehaviour {

	public int texW;
	public int texH;
	public Texture tex;
	public int marginTop	= 20;
	public int marginLeft	= 20;
	public GUIStyle counterStyle = new GUIStyle ();


	void OnGUI() {
		int dist = 10;
		int left = marginLeft;
		int collectedItems = CollectibleItem.getCollectedItems ();

		if (collectedItems > 0) {
			GUI.DrawTexture (new Rect (left, marginTop, texW, texH), tex);
			left = left + texW + dist;
			GUI.Label(new Rect(left, marginTop+5, 200, texH), " x " + collectedItems, counterStyle);
		}
	}
}
