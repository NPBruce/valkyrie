using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorComponentMPlace : EditorComponent
{
    QuestData.MPlace mPlaceComponent;

    public EditorComponentMPlace(string nameIn) : base()
    {
        Game game = Game.Get();
        mPlaceComponent = game.quest.qd.components[nameIn] as QuestData.MPlace;
        component = mPlaceComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        CameraController.SetCamera(mPlaceComponent.location);
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), "MPlace", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), name.Substring("MPlace".Length), delegate { QuestEditorData.ListMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), "Position");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), "><", delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        string r = "Right";
        if (mPlaceComponent.rotate) r = "Down";
        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), "Rotate: " + r, delegate { Rotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        string mast = "Minion";
        if (mPlaceComponent.master) mast = "Master";
        tb = new TextButton(new Vector2(0, 6), new Vector2(8, 1), mast, delegate { MasterToggle(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        game.tokenBoard.AddHighlight(mPlaceComponent.location, "MonsterLoc", "editor");
    }

    public void Rotate()
    {
        mPlaceComponent.rotate = !mPlaceComponent.rotate;
        Update();
    }

    public void MasterToggle()
    {
        mPlaceComponent.master = !mPlaceComponent.master;
        Update();
    }
}
