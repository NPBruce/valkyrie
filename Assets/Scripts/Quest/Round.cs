using UnityEngine;
using System.Collections.Generic;

public class Round
{
    public List<Hero> heroes;
    public List<Monster> monsters;
    public int round = 1;
    public bool heroesSelected = false;
    public int morale;

    public Round()
    {
        // Populate null hero list, these can then be selected as hero types
        heroes = new List<Hero>();
        for (int i = 1; i < 5; i++)
        {
            heroes.Add(new Hero(null, i));
        }

        // Create the monster list so we are ready to start
        monsters = new List<Monster>();
    }


    // This function adjusts morale.  We don't write directly so that NoMorale can be triggered
    public void AdjustMorale(int m)
    {
        Game game = Game.Get();
        morale += m;
        if (morale < 0)
        {
            morale = 0;
            game.moraleDisplay.Update();
            game.quest.eManager.EventTriggerType("NoMorale");
        }
        game.moraleDisplay.Update();
    }

    // Class for holding current hero status
    public class Hero
    {
        // This can be null if not selected
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;
        //  Heros are in a list so they need ID... maybe at some point this can move to an array
        public int id = 0;
        // Used for events that can select or highlight heros
        public bool selected;

        public Hero(HeroData h, int i)
        {
            heroData = h;
            id = i;
        }
    }

    // Class for holding current monster status
    public class Monster
    {
        public MonsterData monsterData;
        public bool activated = false;
        public bool minionStarted = false;
        public bool masterStarted = false;
        public bool unique = false;
        public string uniqueText = "";
        public string uniqueTitle = "";
        // Activation is reset each round so that master/minion are the same and forcing doesn't re roll
        public ActivationData currentActivation;

        // Initialise from monster event
        public Monster(EventManager.MonsterEvent monsterEvent)
        {
            monsterData = monsterEvent.cMonster;
            unique = monsterEvent.qMonster.unique;
            uniqueTitle = monsterEvent.GetUniqueTitle();
            uniqueText = monsterEvent.qMonster.uniqueText;
        }
    }
}
