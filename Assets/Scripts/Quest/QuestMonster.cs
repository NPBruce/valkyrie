using System.Collections.Generic;
using UnityEngine;

public class QuestMonster : MonsterData
{
    public QuestMonster(QuestData.UniqueMonster qm) : base()
    {
        Game game = Game.Get();

        // Get base derived monster type
        MonsterData baseObject = null;
        if (game.cd.monsters.ContainsKey(qm.baseMonster))
        {
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

        image = qm.imagePath;
        if (image.Length == 0 && baseObject != null)
        {
            image = baseObject.image;
        }

        imagePlace = qm.imagePlace;
        if (imagePlace.Length == 0 && baseObject != null)
        {
            imagePlace = baseObject.image;
        }
        if (imagePlace.Length == 0) imagePlace = image;


        activations = qm.activations;
        //asdf dsl;kfj ! unfinished!!
    }
    /*
    public QuestMonster(string nameIn, Dictionary<string, string> data, string path) : base()
    {
        Game game = Game.Get();

        // Get base derived monster type
        if (data.ContainsKey("base"))
        {
            baseMonster = data["base"];
        }
        MonsterData baseObject = null;
        if (game.cd.monsters.ContainsKey(baseMonster))
        {
            baseObject = game.cd.monsters[baseMonster];
        }

        sectionName = nameIn;
        sets = new List<string>();

        name = "";
        if (data.ContainsKey("name"))
        {
            name = data["name"];
        }
        if (baseObject != null && name.Length == 0)
        {
            name = baseObject.name;
        }
        if (name.Length == 0)
        {
            name = nameIn;
        }

        priority = 0;

        if (data.ContainsKey("traits"))
        {
            traits = data["traits"].Split(" ".ToCharArray());
        }
        else if (baseObject != null)
        {
            traits = baseObject.traits;
        }
        else
        {
            traits = new string[0];
        }

        image = "";
        if (data.ContainsKey("image"))
        {
            image = path + "/" + data["image"];
        }
        else if (baseObject != null)
        {
            image = baseObject.image;
        }

        if (data.ContainsKey("info"))
        {
            info = data["info"];
        }
        else if (baseObject != null)
        {
            info = baseObject.info;
        }

        imagePlace = image;
        if (data.ContainsKey("imageplace"))
        {
            info = data["imageplace"];
        }
        else if (baseObject != null)
        {
            imagePlace = baseObject.imagePlace;
        }

        activations = new string[0];
        if (data.ContainsKey("activation"))
        {
            activations = data["activation"].Split(' ');
        }
        // Note - we don't copy activations from base, if it is empty we will fall back to standard activation system
    }*/
}