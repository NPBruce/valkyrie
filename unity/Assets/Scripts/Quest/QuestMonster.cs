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

        // TODO: We get only the name inherited from fixed
        // monsters. It can be edited in next Pull Request
        // when Valkyrie is translated
        // Set name
        name = new StringKey(qm.monsterName, false);
        // If name not set use base type
        if (name.key.Length == 0 && baseObject != null)
        {
            name = baseObject.name;
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
        info = new StringKey(EventManager.SymbolReplace(qm.info.key), false);
        if (info.key.Length == 0 && baseObject != null)
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

        health = qm.health;
        if (!qm.healthDefined && baseObject != null)
        {
            health = baseObject.health;
        }
    }
}

// Class for quest defined activations
public class QuestActivation : ActivationData
{
    public QuestActivation(QuestData.Activation qa) : base()
    {
        // Read data from activation
        ability = new StringKey(EventManager.SymbolReplace(qa.ability.key), false);
        masterActions = new StringKey(EventManager.SymbolReplace(qa.masterActions.key), false);
        minionActions = new StringKey(EventManager.SymbolReplace(qa.minionActions.key), false);
        minionFirst = qa.minionFirst;
        masterFirst = qa.masterFirst;
        move = new StringKey(EventManager.SymbolReplace(qa.move.key), false);
        moveButton = new StringKey(EventManager.SymbolReplace(qa.moveButton.key), false);
        sectionName = qa.sectionName;
    }
}