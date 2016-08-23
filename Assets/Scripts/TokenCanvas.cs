using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenCanvas : MonoBehaviour {

    public List<TokenControl> tc;
	// Use this for initialization
	void Awake () {
        tc = new List<TokenControl>();
	}

    public void add(QuestData.Door d)
    {
        tc.Add(new TokenControl(d));
    }

    public class TokenControl
    {
        QuestData.Event e;

        public TokenControl(QuestData.Door d)
        {
            UnityEngine.UI.Button button = d.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.interactable = true;
            button.onClick.AddListener(delegate { startEvent();  });
            e = d;
        }

        public void startEvent()
        {
            if (e.getVisible())
            {
                DialogWindow dw = new DialogWindow(e);
            }
        }

    }

}
