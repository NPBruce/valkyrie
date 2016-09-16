using System.Collections.Generic;
using UnityEngine;

public class QuestMonster : MonsterData
{
    public bool useMonsterTypeActivations = false;
    public string derivedType = "";

    public QuestMonster(QuestData.UniqueMonster qm) : base()
    {
        Game game = Game.Get();

        // Get base derived monster type
        MonsterData baseObject = null;

        if (game.cd.monsters.ContainsKey(qm.baseMonster))
        {
            derivedType = qm.baseMonster;
            baseObject = game.cd.monsters[qm.baseMonster];
        }

        name = qm.monsterName;
        if (name.Length == 0 && baseObject != null)
        {
            name = baseObject.name;
        }

        sets = new List<string>();
        sectionName = qm.name;
        priority = 0;

        traits = qm.traits;
        if (traits.Length == 0 && baseObject != null)
        {
            traits = baseObject.traits;
        }

        info = qm.info;
        if (info.Length == 0 && baseObject != null)
        {
            info = baseObject.info;
        }

        image = qm.GetImagePath();
        if (image.Length == 0 && baseObject != null)
        {
            image = baseObject.image;
        }

        imagePlace = qm.GetImagePlacePath();
        if (imagePlace.Length == 0 && baseObject != null)
        {
            imagePlace = baseObject.image;
        }
        if (imagePlace.Length == 0) imagePlace = image;


        activations = qm.activations;
        if (activations.Length == 0 && baseObject != null)
        {
            useMonsterTypeActivations = true;
        }
    }
}