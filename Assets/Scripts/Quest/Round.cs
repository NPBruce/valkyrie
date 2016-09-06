using UnityEngine;
using System.Collections.Generic;

public class Round
{
    public List<Hero> heroes;
    public List<Monster> monsters;
    public int round = 0;
    public bool heroesSelected = false;
    public Stack<QuestData.Event> eventList;
    public int morale;


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
        public Monster(QuestData.Monster m)
        {
            monsterData = m.mData;
            unique = m.unique;
            uniqueTitle = m.uniqueTitle;
            uniqueText = m.uniqueText;
        }
    }
}
