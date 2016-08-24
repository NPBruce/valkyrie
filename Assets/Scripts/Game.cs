using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
hero section
monster events (point)
event triggers
event conditions
event flags
monster UI
moster activations
hero activations
mouse scroll
comments
error handling
packaging
camera jump bug
text background
bigtokens
dialog interlock

tokens/doors in the same space
specific place monsters
unique monsters
extra event types
threat
content selection

    */

public class Game : MonoBehaviour {

    public ContentData cd;
    public QuestData qd;
    public List<Hero> heros;
    public List<Monster> monsters;

    // Use this for initialization
    void Awake () {
        cd = new ContentData(Application.dataPath + "/../../valkyrie-contentpacks/");
        foreach(string pack in cd.GetPacks())
        {
            cd.LoadContent(pack);
        }

        qd = new QuestData(Application.dataPath + "/../../valkyrie-quests/roag-intro/quest.ini");

        heros = new List<Hero>();
        heros.Add(new Hero(cd.heros["HeroSyndrael"]));
        heros.Add(new Hero(cd.heros["HeroJainFairwood"]));
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 100, 100), d2e);
        //ih.drawGUI();
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("escape"))
           Application.Quit();
    }

    public void triggerEvent(string name)
    {
        QuestData.Event e = (QuestData.Event)qd.components[name];
        DialogWindow dw = new DialogWindow(e);
        foreach (string s in e.addComponents)
        {
            qd.components[s].setVisible(true);
        }
        foreach (string s in e.removeComponents)
        {
            qd.components[s].setVisible(false);
        }

        if (e.location != null)
        {
            Camera cam = FindObjectOfType<Camera>();
            cam.transform.position = new Vector3(e.location.x * 105, e.location.y * 105, -800);
        }
    }

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

    public class Monster
    {

    }
}
