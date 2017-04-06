using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

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

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(4, 1), CommonStringKeys.MPLACE, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 0), new Vector2(15, 1), 
            new StringKey(name.Substring("MPlace".Length),false), 
            delegate { QuestEditorData.ListMPlace(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        StringKey rotateKey = new StringKey("val","RIGHT");
        if (mPlaceComponent.rotate)
        {
            rotateKey = new StringKey("val", "DOWN");
        }

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1),
            new StringKey("val","ROTATE_TO",rotateKey), 
            delegate { Rotate(); });

        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        StringKey mast = new StringKey("val","MINION");
        if (mPlaceComponent.master)
        {
            mast = new StringKey("val","MASTER");
        }
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
