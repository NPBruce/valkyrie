using UnityEngine;
using System.Collections;

// Class for creation of monster seleciton options
public class MonsterDialogMoM : MonsterDialog
{
    public Quest.Monster monster;

    public MonsterDialogMoM(Quest.Monster m) : base(m)
    {
    }

    public override void CreateWindow()
    {
        Game game = Game.Get();
        int index = 0;
        for (int i = 0; i < game.quest.monsters.Count; i++)
        {
            if (game.quest.monsters[i] == monster)
            {
                index = i;
            }
        }

        float offset = (index + 0.1f) * (MonsterCanvas.monsterSize + 0.5f);

        if (game.quest.horrorPhase)
        {
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), "Horror Check", delegate { OnCancel(); });
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Cancel", delegate { OnCancel(); });
        }
        else
        {
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset), new Vector2(10, 2), "Attack", delegate { OnCancel(); });
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 2.5f), new Vector2(10, 2), "Evade", delegate { OnCancel(); });
            new TextButton(new Vector2(UIScaler.GetRight(-10.5f - MonsterCanvas.monsterSize), offset + 5f), new Vector2(10, 2), "Cancel", delegate { OnCancel(); });
        }
    }
}
