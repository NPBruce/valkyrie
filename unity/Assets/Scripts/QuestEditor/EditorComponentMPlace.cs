using UnityEngine;
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
    
    override public float AddSubComponents(float offset)
    {
        CameraController.SetCamera(mPlaceComponent.location);
        Game game = Game.Get();

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(4, offset), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        StringKey rotateKey = new StringKey("val","RIGHT");
        if (mPlaceComponent.rotate)
        {
            rotateKey = new StringKey("val", "DOWN");
        }

        tb = new TextButton(new Vector2(0, offset), new Vector2(8, 1),
            new StringKey("val","ROTATE_TO",rotateKey), 
            delegate { Rotate(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        StringKey mast = new StringKey("val","MONSTER_MINION");
        if (mPlaceComponent.master)
        {
            mast = new StringKey("val","MONSTER_MASTER");
        }
        tb = new TextButton(new Vector2(0, offset), new Vector2(8, 1), mast, delegate { MasterToggle(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        offset += 2;

        game.tokenBoard.AddHighlight(mPlaceComponent.location, "MonsterLoc", Game.EDITOR);

        return offset;
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
