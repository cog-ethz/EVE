using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JRDImageRemover : MonoBehaviour {

    public UnityEngine.UI.Image[] dropImages;

	public void resetImages()
    {
        foreach (UnityEngine.UI.Image image in dropImages)
        {
            image.overrideSprite = null;
        }
    }
}
