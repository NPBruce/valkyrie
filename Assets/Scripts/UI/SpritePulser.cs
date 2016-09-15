using UnityEngine;
using System.Collections;

public class SpritePulser : MonoBehaviour {

    UnityEngine.UI.Image image;

	// Use this for initialization
	void Start () {
        image = gameObject.GetComponent<UnityEngine.UI.Image>();

    }
	
	// Update is called once per frame
	void Update () {

        float factor = 1f + (0.2f * Mathf.Sin(Time.time * 4));
        image.rectTransform.sizeDelta = new Vector2(factor, factor);
    }
}
