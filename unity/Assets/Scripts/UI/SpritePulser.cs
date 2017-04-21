using UnityEngine;
using System.Collections;

// This class is used to make a sprite change size over time
// It can be attached to a unity gameobject
public class SpritePulser : MonoBehaviour {

    UnityEngine.UI.Image image;

	// Use this for initialization (called at creation)
	void Start () {
        // Get the image attached to this game object
        image = gameObject.GetComponent<UnityEngine.UI.Image>();
    }
	
	// Update is called once per frame
	void Update () {
        // Use sin function to determine scale
        // Varies from 80% to 120%
        float factor = 1f + (0.2f * Mathf.Sin(Time.time * 4));
        // Apply scale
        image.rectTransform.sizeDelta = new Vector2(factor, factor);
    }
}
