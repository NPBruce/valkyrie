using System.Collections.Generic;
using UnityEngine;

// A monster quest class that is defined by the quest
public class QuestMonster : MonsterData
{
    public bool useMonsterTypeActivations = false;
    public string derivedType = "";

    // Construct with quest data
    public QuestMonster(QuestData.UniqueMonster qm) : base()
    {
        Game game = Game.Get();

        // Get base derived monster type
        MonsterData baseObject = null;

        // Check for content data monster defined as base
        if (game.cd.monsters.ContainsKey(qm.baseMonster))
        {
            // Set base monster type
            derivedType = qm.baseMonster;
            baseObject = game.cd.monsters[qm.baseMonster];
        }

        // Set name
        name = qm.monsterName;
        // If name not set use base type
        if (name.Length == 0 && baseObject != null)
        {
            name = baseObject.name;
        }

        // Initialise sets
        sets = new List<string>();

        // define data
        sectionName = qm.name;
        priority = 0;

        // Read traits from quest data or base type
        traits = qm.traits;
        if (traits.Length == 0 && baseObject != null)
        {
            traits = baseObject.traits;
        }

        // Read info from quest data or base type
        info = EventManager.SymbolReplace(qm.info);
        if (info.Length == 0 && baseObject != null)
        {
            info = baseObject.info;
        }

        // Read image from quest data or base type
        image = qm.GetImagePath();
        if (image.Length == 0 && baseObject != null)
        {
            image = baseObject.image;
        }

        // Read placement image from quest data or base type
        imagePlace = qm.GetImagePlacePath();
        if (imagePlace.Length == 0 && baseObject != null)
        {
            imagePlace = baseObject.image;
        }
        if (imagePlace.Length == 0) imagePlace = image;

        // Read activations  from quest data or base type
        activations = qm.activations;
        if (activations.Length == 0 && baseObject != null)
        {
            useMonsterTypeActivations = true;
        }

        // Read activations  from quest data or base type
        activations = qm.activations;
        if (activations.Length == 0 && baseObject != null)
        {
            useMonsterTypeActivations = true;
        }
    }
}

// Class for quest defined activations
public class QuestActivation : ActivationData
{
    public QuestActivation(QuestData.Activation qa) : base()
    {
        // Read data from activation
        sectionName = qa.name;
        ability = EventManager.SymbolReplace(qa.ability);
        masterActions = EventManager.SymbolReplace(qa.masterActions);
        minionActions = EventManager.SymbolReplace(qa.minionActions);
        minionFirst = qa.minionFirst;
        masterFirst = qa.masterFirst;
    }
}