using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {

    ImageHelper ih;

	// Use this for initialization
	void Start () {
        ih = new ImageHelper();

        ih.drawImage("GamePacks/D2E/img/DJ01_CoreSet.png", 0, 0);
    }
	
    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 100, 100), d2e);
        ih.drawGUI();
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("escape"))
           Application.Quit();
    }
}
