using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Dump list of things to to:

hero section
monster events (point)
event triggers
event conditions
event flags
monster UI
moster activations
mouse pan
packaging
text background
big tokens

tokens/doors in the same space
specific place monsters
unique monsters
extra event types
threat
content selection

    */


// General controller for the game
public class Game : MonoBehaviour {

    public ContentData cd;
    public QuestData qd;
    public List<Hero> heros;
    public List<Monster> monsters;
    public int round = 0;

    // Use this for initialization
    void Awake () {
        // This will load content, need to work out where this should be stored, and how it should be packed
        cd = new ContentData(Application.dataPath + "/../../valkyrie-contentpacks/");

        // In the future this is where you select which packs to load
        foreach(string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }

        // In the future this is where you select a quest, need to work out where this should be stored, and how it should be delivered
        qd = new QuestData(Application.dataPath + "/../../valkyrie-quests/roag-intro/quest.ini");

        // Working on Hero selection here (currently hard coded)
        heros = new List<Hero>();
        heros.Add(new Hero(cd.heros["HeroSyndrael"]));
        heros.Add(new Hero(cd.heros["HeroJainFairwood"]));

        monsters = new List<Monster>();
    }

	// Update is called once per frame
	void Update () {
        // Escape will quit because we don't have a proper UI yet
        if (Input.GetKey("escape"))
           Application.Quit();
    }

    public void triggerEvent(string name)
    {
        // Check if the event doesn't exists - quest fault
        if(!qd.components.ContainsKey(name))
        {
            Debug.Log("Warning: Missing event called: " + name);
            return;
        }

        QuestData.Event e = (QuestData.Event)qd.components[name];

        // If this is a monster event then add the monster group
        if (e is QuestData.Monster)
        {
            QuestData.Monster qm = (QuestData.Monster)e;

            // Is this type new?
            bool newMonster = true;
            foreach(Monster m in monsters)
            {
                if (m.monsterData.name.Equals(qm.mData.name))
                    newMonster = false;
            }

            // Add the new type
            if (newMonster)
            {
                monsters.Add(new Monster(qm.mData));
                MonsterCanvas mc = FindObjectOfType<MonsterCanvas>();
                mc.UpdateList();
            }
        }

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        new DialogWindow(e);
        foreach (string s in e.addComponents)
        {
            qd.components[s].setVisible(true);
        }
        foreach (string s in e.removeComponents)
        {
            qd.components[s].setVisible(false);
        }

        if (e.locationSpecified)
        {
            Camera cam = FindObjectOfType<Camera>();
            cam.transform.position = new Vector3(e.location.x * 105, e.location.y * 105, -800);
        }
    }

    // A hero has finished their turn
    public void heroActivated()
    {
        // Check if all heros have finished
        bool herosActivated = true;
        foreach (Hero h in heros)
        {
            if (!h.activated)
                herosActivated = false;
        }

        // activate a monster group
        bool monstersActivated = activateMonster();

        // If all heros have finished activate all other monster groups
        if(herosActivated)
        {
            while(!monstersActivated)
                monstersActivated = activateMonster();
        }

        // If everyone has finished move to next round
        if (monstersActivated && herosActivated)
        {
            foreach (Hero h in heros)
            {
                h.activated = false;
            }
            foreach (Monster m in monsters)
            {
                m.activated = false;
            }
            round++;
            MonsterCanvas mc = FindObjectOfType<MonsterCanvas>();
            mc.UpdateStatus();
        }
    }

    // Activate a monster (if any left) and return true if all monsters activated
    public bool activateMonster()
    {
        List<int> notActivated = new List<int>();
        Debug.Log("A");
        // Get the index of all monsters that haven't activated
        for(int i = 0; i < monsters.Count; i++)
        {
            if (!monsters[i].activated)
                notActivated.Add(i);
        }

        Debug.Log(notActivated.Count);
        // If no monsters are found return true
        if (notActivated.Count == 0)
            return true;

        Debug.Log("C");
        monsters[notActivated[Random.Range(0, notActivated.Count)]].activated = true;
        MonsterCanvas mc = FindObjectOfType<MonsterCanvas>();
        mc.UpdateStatus();

        // If there was one group left return true
        if (notActivated.Count == 1)
            return true;

        Debug.Log("D");
        // More groups unactivated
        return false;
    }

    // Class for holding current hero status
    public class Hero
    {
        public HeroData heroData;
        public bool activated = false;
        public bool defeated = false;

        public Hero(HeroData h)
        {
            heroData = h;
        }
    }

    // Class for holding current monster status
    public class Monster
    {
        public MonsterData monsterData;
        public bool activated = false;

        public Monster(MonsterData m)
        {
            monsterData = m;
        }
    }
}
