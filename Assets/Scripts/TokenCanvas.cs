using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class for managing token and door operation
// One object is created and attached to the token canvas
public class TokenCanvas : MonoBehaviour {

    public List<TokenControl> tc;
	// Use this for initialization
	void Awake () {
        tc = new List<TokenControl>();
	}

    // Add a door
    public void add(QuestData.Door d)
    {
        tc.Add(new TokenControl(d));
    }

    // Add a token
    public void add(QuestData.Token t)
    {
        tc.Add(new TokenControl(t));
    }

    // Class for tokens and doors that will get the onClick event
    public class TokenControl
    {
        QuestData.Event e;

        // Initialise from a door
        public TokenControl(QuestData.Door d)
        {
            UnityEngine.UI.Button button = d.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { startEvent();  });
            e = d;
        }

        // Initialise from a token
        public TokenControl(QuestData.Token t)
        {
            UnityEngine.UI.Button button = t.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { startEvent(); });
            e = t;
        }

        // On click the tokens start an event
        public void startEvent()
        {
            // If we aren't visible ignore the click
            if (!e.getVisible())
                return;
            // If a dialog is open ignore
            if (GameObject.FindGameObjectWithTag("dialog") != null)
                return;
            // Spawn a window with the door/token info
            new DialogWindow(e);
        }

    }

}
