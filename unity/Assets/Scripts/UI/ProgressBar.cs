using UnityEngine;
using System.Collections;

// This class is used to make a sprite change size over time
// It can be attached to a unity gameobject
public class ProgressBar : MonoBehaviour {

    RectTransform rect;
    float xEdge = 0;
    float size = 0;
    WWW download;

    /// <summary>
    /// Set the WWW object to monitor</summary>
    /// <param name="d">WWW object</param>
    public void SetDownload(WWW d)
    {
        download = d;
    }

    /// <summary>
    /// Called at init by unity.</summary>
	void Start () {
        // Get the image attached to this game object
        rect = gameObject.GetComponent<RectTransform>();
        size = rect.sizeDelta.x;
        xEdge = rect.anchoredPosition.x - (size / 2);
    }
	
    /// <summary>
    /// Called once per frame by Unity.</summary>
    /// <param name="height">Vertical size.</param>
	void Update () {
        float fill = 0;
        if (download != null && download.error == null)
        {
            fill = download.progress * size;
        }
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, xEdge, fill);
    }
}
