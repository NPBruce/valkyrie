using Assets.Scripts.Content;
using System.Collections.Generic;

// A monster quest class that is defined by the quest
public class QuestMonster : MonsterData
{
    public bool useMonsterTypeActivations = false;
    public string derivedType = "";

    // Construct with quest data
    public QuestMonster(QuestData.CustomMonster qm) : base()
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

        // If name not set use base type
        if (name.fullKey.Length == 0 && baseObject != null)
        {
            name = baseObject.name;
        } else
        {
            name = qm.monsterName;
        }

        // Initialise sets
        sets = new List<string>();

        // define data
        sectionName = qm.sectionName;
        priority = 0;

        // Read traits from quest data or base type
        traits = qm.traits;
        if (traits.Length == 0 && baseObject != null)
        {
            traits = baseObject.traits;
        }

        // Read info from quest data or base type
        info = new StringKey(null, EventManager.SymbolReplace(qm.info.fullKey), false);
        if (info.fullKey.Length == 0 && baseObject != null)
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

        healthBase = qm.healthBase;
        healthPerHero = qm.healthPerHero;
        if (!qm.healthDefined && baseObject != null)
        {
            healthBase = baseObject.healthBase;
            healthPerHero = baseObject.healthPerHero;
        }
    }
}

// Class for quest defined activations
public class QuestActivation : ActivationData
{
    public QuestActivation(QuestData.Activation qa) : base()
    {
        // Read data from activation
        ability = qa.ability;
        masterActions = qa.masterActions;
        minionActions = qa.minionActions;
        minionFirst = qa.minionFirst;
        masterFirst = qa.masterFirst;
        move = qa.move;
        moveButton = qa.moveButton;
        sectionName = qa.sectionName;
    }
}