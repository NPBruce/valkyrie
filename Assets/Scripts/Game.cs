using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {

    public ContentData cd;

	// Use this for initialization
	void Awake () {
        cd = new ContentData(Application.dataPath + "/../../valkyrie-contentpacks/");
        foreach(string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 100, 100), d2e);
        //ih.drawGUI();
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("escape"))
           Application.Quit();
    }
}
