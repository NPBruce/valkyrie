using UnityEngine;
using System.Collections;

// Next stage button is used by MoM to move between investigators and monsters
public class NextStageButton
{
    // Construct and display
    public NextStageButton()
    {
        if (Game.Get().gameType.DisplayHeroes()) return;
        TextButton qb = new TextButton(new Vector2(UIScaler.GetHCenter(10f), UIScaler.GetBottom(-2.5f)), new Vector2(4, 2), "->", delegate { Next(); });
        // Untag as dialog so this isn't cleared away
        qb.ApplyTag("questui");
        Update();
    }

    public void Update()
    {
        // Clean up everything marked as 'uiphase'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("uiphase"))
            Object.Destroy(go);

        DialogBox db;
        if (Game.Get().quest.phase == Quest.MoMPhase.horror)
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), UIScaler.GetBottom(-2.5f)), new Vector2(20, 2), "Horror Phase");
            db.SetFont(Game.Get().gameType.GetHeaderFont());
        }
        else if (Game.Get().quest.phase == Quest.MoMPhase.mythos)
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), UIScaler.GetBottom(-2.5f)), new Vector2(20, 2), "Mythos Phase");
            db.SetFont(Game.Get().gameType.GetHeaderFont());
        }
        else if (Game.Get().quest.phase == Quest.MoMPhase.monsters)
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), UIScaler.GetBottom(-2.5f)), new Vector2(20, 2), "Monster Phase");
            db.SetFont(Game.Get().gameType.GetHeaderFont());
        }
        else
        {
            db = new DialogBox(new Vector2(UIScaler.GetHCenter(-10f), UIScaler.GetBottom(-2.5f)), new Vector2(20, 2), "Investigator Phase");
            db.SetFont(Game.Get().gameType.GetHeaderFont());
        }
        db.ApplyTag("uiphase");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        db.AddBorder();
    }

    // Button pressed
    public void Next()
    {
        if (GameObject.FindGameObjectWithTag("dialog") != null)
        {
            return;
        }

        Game game = Game.Get();

        // Add to undo stack
        game.quest.Save();

        if (game.quest.phase == Quest.MoMPhase.horror)
        {
            game.roundControl.EndRound();
        }
        else
        {
            game.roundControl.HeroActivated();
        }
    }
}
