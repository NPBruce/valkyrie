using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentSpawn : EditorComponent
{
    private readonly StringKey POSITION_TYPE_UNUSED = new StringKey("val", "POSITION_TYPE_UNUSED");
    private readonly StringKey POSITION_TYPE_HIGHLIGHT = new StringKey("val", "POSITION_TYPE_HIGHLIGHT");
    private readonly StringKey MONSTER_UNIQUE = new StringKey("val", "MONSTER_UNIQUE");
    private readonly StringKey MONSTER_NORMAL = new StringKey("val", "MONSTER_NORMAL");

    private readonly StringKey UNIQUE_TITLE = new StringKey("val", "UNIQUE_TITLE");
    private readonly StringKey UNIQUE_INFO = new StringKey("val", "UNIQUE_INFO");
    private readonly StringKey HEALTH = new StringKey("val", "HEALTH");
    private readonly StringKey HEALTH_HERO = new StringKey("val", "HEALTH_HERO");
    private readonly StringKey TYPES = new StringKey("val", "TYPES");
    
    private readonly StringKey REQ_TRAITS = new StringKey("val", "REQ_TRAITS");
    private readonly StringKey POOL_TRAITS = new StringKey("val", "POOL_TRAITS");
    
    
    QuestData.Spawn spawnComponent;

    DialogBoxEditable uniqueTitleDBE;
    DialogBoxEditable uniqueTextDBE;
    DialogBoxEditable healthDBE;
    DialogBoxEditable healthHeroDBE;

    EditorSelectionList monsterTypeESL;
    EditorSelectionList monsterTraitESL;

    public EditorComponentSpawn(string nameIn) : base()
    {
        Game game = Game.Get();
        spawnComponent = game.quest.qd.components[nameIn] as QuestData.Spawn;
        component = spawnComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        CameraController.SetCamera(spawnComponent.location);
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), CommonStringKeys.SPAWN, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), 
            new StringKey(null,name.Substring("Spawn".Length),false), delegate { QuestEditorData.ListSpawn(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(4, 1), CommonStringKeys.POSITION);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(4, 2), new Vector2(1, 1), CommonStringKeys.POSITION_SNAP, delegate { GetPosition(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 2), new Vector2(1, 1), CommonStringKeys.POSITION_FREE, delegate { GetPosition(false); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        if (!spawnComponent.locationSpecified)
        {
            tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), POSITION_TYPE_UNUSED, delegate { PositionTypeCycle(); });
        }
        else
        {
            tb = new TextButton(new Vector2(7, 2), new Vector2(4, 1), POSITION_TYPE_HIGHLIGHT, delegate { PositionTypeCycle(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(0, 4), new Vector2(8, 1), CommonStringKeys.EVENT, delegate { QuestEditorData.SelectAsEvent(name); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(4, 6), new Vector2(3, 1), new StringKey("val","X_COLON",HEALTH));
        db.ApplyTag("editor");

        // Dumbers dont need translation
        healthDBE = new DialogBoxEditable(new Vector2(7, 6), new Vector2(3, 1), 
        spawnComponent.uniqueHealthBase.ToString(), delegate { UpdateHealth(); });
        healthDBE.ApplyTag("editor");
        healthDBE.AddBorder();

        db = new DialogBox(new Vector2(10, 6), new Vector2(7, 1), new StringKey("val","X_COLON",HEALTH_HERO));
        db.ApplyTag("editor");

        // Numbers dont need translation
        healthHeroDBE = new DialogBoxEditable(new Vector2(17, 6), new Vector2(3, 1), 
        spawnComponent.uniqueHealthHero.ToString(), delegate { UpdateHealthHero(); });
        healthHeroDBE.ApplyTag("editor");
        healthHeroDBE.AddBorder();

        if (game.gameType is D2EGameType)
        {
            tb = new TextButton(
                new Vector2(12, 4), new Vector2(8, 1), 
                CommonStringKeys.PLACEMENT, 
                delegate { QuestEditorData.SelectAsSpawnPlacement(name); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        
            if (spawnComponent.unique)
            {
                tb = new TextButton(new Vector2(0, 6), new Vector2(4, 1), MONSTER_UNIQUE, delegate { UniqueToggle(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                db = new DialogBox(new Vector2(0, 8), new Vector2(5, 1), new StringKey("val", "X_COLON", UNIQUE_TITLE));
                db.ApplyTag("editor");

                uniqueTitleDBE = new DialogBoxEditable(
                    new Vector2(5, 8), new Vector2(15, 1),
                    spawnComponent.uniqueTitle.Translate(),
                delegate { UpdateUniqueTitle(); });
                uniqueTitleDBE.ApplyTag("editor");
                uniqueTitleDBE.AddBorder();

                db = new DialogBox(new Vector2(0, 10), new Vector2(20, 1), new StringKey("val", "X_COLON", UNIQUE_INFO));
                db.ApplyTag("editor");

                uniqueTextDBE = new DialogBoxEditable(
                new Vector2(0, 11), new Vector2(20, 8), 
                spawnComponent.uniqueText.Translate(),
                delegate { UpdateUniqueText(); });
                uniqueTextDBE.ApplyTag("editor");
                uniqueTextDBE.AddBorder();
            }
            else
            {
                tb = new TextButton(new Vector2(0, 6), new Vector2(4, 1), MONSTER_NORMAL, delegate { UniqueToggle(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(0, 20), new Vector2(3, 1), new StringKey("val", "X_COLON", TYPES));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 20), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTypeAdd(0); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int i = 0;
        for (i = 0; i < 8; i++)
        {
            if (spawnComponent.mTypes.Length > i)
            {
                int mSlot = i;
                string mName = spawnComponent.mTypes[i];
                if (mName.IndexOf("Monster") == 0)
                {
                    mName = mName.Substring("Monster".Length);
                }

                tb = new TextButton(new Vector2(0, 21 + i), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterTypeRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(1, 21 + i), new Vector2(11, 1), new StringKey(null,mName,false), delegate { MonsterTypeReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(12, 21 + i), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTypeAdd(mSlot + 1); }, Color.green);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }


        db = new DialogBox(new Vector2(14, 20), new Vector2(3, 1), REQ_TRAITS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 20), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTraitsAdd(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (i = 0; i < 8; i++)
        {
            if (spawnComponent.mTraitsRequired.Length > i)
            {
                int mSlot = i;
                string mName = spawnComponent.mTraitsRequired[i];

                tb = new TextButton(new Vector2(14, 21 + i), new Vector2(1, 1), 
                    CommonStringKeys.MINUS, delegate { MonsterTraitsRemove(mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(15, 21 + i), new Vector2(5, 1), 
                    new StringKey("val", mName), delegate { MonsterTraitReplace(mSlot); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(14, 21 + spawnComponent.mTraitsRequired.Length), new Vector2(3, 1), POOL_TRAITS);
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 21 + spawnComponent.mTraitsRequired.Length), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTraitsAdd(true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int j = 0; j < 8; j++)
        {
            if (spawnComponent.mTraitsPool.Length > j)
            {
                int mSlot = j;
                string mName = spawnComponent.mTraitsPool[j];

                tb = new TextButton(new Vector2(14, 22 + spawnComponent.mTraitsRequired.Length + j), 
                    new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterTraitsRemove(mSlot, true); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");

                tb = new TextButton(new Vector2(15, 22 + spawnComponent.mTraitsRequired.Length + j), 
                    new Vector2(5, 1), new StringKey("val", mName), delegate { MonsterTraitReplace(mSlot, true); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        game.tokenBoard.AddHighlight(spawnComponent.location, "MonsterLoc", "editor");
    }

    public void PositionTypeCycle()
    {
        spawnComponent.locationSpecified = !spawnComponent.locationSpecified;
        Update();
    }

    public void UniqueToggle()
    {
        spawnComponent.unique = !spawnComponent.unique;
        if (!spawnComponent.unique)
        {
            LocalizationRead.scenarioDict.Remove(spawnComponent.uniquetitle_key);
            LocalizationRead.scenarioDict.Remove(spawnComponent.uniquetext_key);
        }
        else
        {
            LocalizationRead.updateScenarioText(spawnComponent.uniquetitle_key, spawnComponent.sectionName);
            LocalizationRead.updateScenarioText(spawnComponent.uniquetext_key, "-");
        }
        Update();
    }

    public void UpdateHealth()
    {
        float.TryParse(healthDBE.Text, out spawnComponent.uniqueHealthBase);
        Update();
    }

    public void UpdateHealthHero()
    {
        float.TryParse(healthHeroDBE.Text, out spawnComponent.uniqueHealthHero);
        Update();
    }

    public void UpdateUniqueTitle()
    {
        if (!uniqueTitleDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(spawnComponent.uniquetitle_key, uniqueTitleDBE.Text);
        }
    }

    public void UpdateUniqueText()
    {
        if (!uniqueTextDBE.Text.Equals(""))
        {
            LocalizationRead.updateScenarioText(spawnComponent.uniquetext_key, uniqueTextDBE.Text);
        }
    }

    public void MonsterTypeAdd(int pos)
    {
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> monsters = new List<EditorSelectionList.SelectionListEntry>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.CustomMonster)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "Custom"));
            }
        }

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Spawn)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "Spawn"));
            }
        }

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            StringBuilder display = new StringBuilder().Append(kv.Key);
            StringBuilder localizedDisplay = new StringBuilder().Append(kv.Value.name.Translate());
            List<string> sets = new List<string>(kv.Value.traits);
            foreach (string s in kv.Value.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add("base");
                }
                else
                {
                    display.Append(" ").Append(s);
                    localizedDisplay.Append(" ").Append(new StringKey("val", s).Translate());
                    sets.Add(s);
                }
            }
            monsters.Add(
                EditorSelectionList.SelectionListEntry.BuildNameKeyTraitsItem(
                    localizedDisplay.ToString(),display.ToString(), sets));
        }
        monsterTypeESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, monsters, delegate { SelectMonsterType(pos); });
        monsterTypeESL.SelectItem();
    }

    public void MonsterTypeReplace(int pos)
    {
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> monsters = new List<EditorSelectionList.SelectionListEntry>();

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.CustomMonster)
            {
                monsters.Add(new EditorSelectionList.SelectionListEntry(kv.Key, "Quest"));
            }
        }

        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            StringBuilder display = new StringBuilder().Append(kv.Key);
            StringBuilder localizedDisplay = new StringBuilder().Append(kv.Value.name.Translate());
            List<string> sets = new List<string>(kv.Value.traits);
            foreach (string s in kv.Value.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add("base");
                }
                else
                {
                    display.Append(" ").Append(s);
                    localizedDisplay.Append(" ").Append(new StringKey("val", s).Translate());
                    sets.Add(s);
                }
            }
            monsters.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitsItem(
                localizedDisplay.ToString(),display.ToString(), sets));
        }
        monsterTypeESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, monsters, delegate { SelectMonsterType(pos, true); });
        monsterTypeESL.SelectItem();
    }

    public void SelectMonsterType(int pos, bool replace = false)
    {
        if (replace)
        {
            spawnComponent.mTypes[pos] = monsterTypeESL.selection.Split(" ".ToCharArray())[0];
        }
        else
        {
            string[] newM = new string[spawnComponent.mTypes.Length + 1];

            int j = 0;
            for (int i = 0; i < newM.Length; i++)
            {
                if (j == pos && i == j)
                {
                    newM[i] = monsterTypeESL.selection.Split(" ".ToCharArray())[0];
                }
                else
                {
                    newM[i] = spawnComponent.mTypes[j];
                    j++;
                }
            }
            spawnComponent.mTypes = newM;
        }
        Update();
    }

    public void MonsterTypeRemove(int pos)
    {
        if ((spawnComponent.mTypes.Length == 1) && (spawnComponent.mTraitsRequired.Length == 0) && (spawnComponent.mTraitsPool.Length == 0))
        {
            return;
        }

        string[] newM = new string[spawnComponent.mTypes.Length - 1];

        int j = 0;
        for (int i = 0; i < spawnComponent.mTypes.Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = spawnComponent.mTypes[i];
                j++;
            }
        }
        spawnComponent.mTypes = newM;
        Update();
    }

    public void MonsterTraitReplace(int pos, bool pool = false)
    {
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            foreach (string s in kv.Value.traits)
            {
                traits.Add(s);
            }
        }
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in traits)
        {
            list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(s));
        }
        monsterTraitESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, list, delegate { SelectMonsterTraitReplace(pos, pool); });
        monsterTraitESL.SelectItem();
    }

    public void SelectMonsterTraitReplace(int pos, bool pool = false)
    {
        if (pool)
        {
            spawnComponent.mTraitsPool[pos] = monsterTraitESL.selection;
        }
        else
        {
            spawnComponent.mTraitsRequired[pos] = monsterTraitESL.selection;
        }
        Update();
    }

    public void MonsterTraitsAdd(bool pool = false)
    {
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
        {
            foreach (string s in kv.Value.traits)
            {
                traits.Add(s);
            }
        }

        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in traits)
        {
            list.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(s));
        }
        monsterTraitESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, list, delegate { SelectMonsterTrait(pool); });
        monsterTraitESL.SelectItem();
    }

    public void SelectMonsterTrait(bool pool = false)
    {
        if (pool)
        {
            string[] newM = new string[spawnComponent.mTraitsPool.Length + 1];

            int i;
            for (i = 0; i < spawnComponent.mTraitsPool.Length; i++)
            {
                newM[i] = spawnComponent.mTraitsPool[i];
            }

            newM[i] = monsterTraitESL.selection;
            spawnComponent.mTraitsPool = newM;
        }
        else
        {
            string[] newM = new string[spawnComponent.mTraitsRequired.Length + 1];

            int i;
            for (i = 0; i < spawnComponent.mTraitsRequired.Length; i++)
            {
                newM[i] = spawnComponent.mTraitsRequired[i];
            }

            newM[i] = monsterTraitESL.selection;
            spawnComponent.mTraitsRequired = newM;
        }
        Update();
    }

    public void MonsterTraitsRemove(int pos, bool pool = false)
    {
        if ((spawnComponent.mTypes.Length + spawnComponent.mTraitsPool.Length + spawnComponent.mTraitsRequired.Length) <= 1)
        {
            return;
        }
        if (pool)
        {
            string[] newM = new string[spawnComponent.mTraitsPool.Length - 1];

            int j = 0;
            for (int i = 0; i < spawnComponent.mTraitsPool.Length; i++)
            {
                if (i != pos || i != j)
                {
                    newM[j] = spawnComponent.mTraitsPool[i];
                    j++;
                }
            }
            spawnComponent.mTraitsPool = newM;
        }
        else
        {
            string[] newM = new string[spawnComponent.mTraitsRequired.Length - 1];

            int j = 0;
            for (int i = 0; i < spawnComponent.mTraitsRequired.Length; i++)
            {
                if (i != pos || i != j)
                {
                    newM[j] = spawnComponent.mTraitsRequired[i];
                    j++;
                }
            }
            spawnComponent.mTraitsRequired = newM;
        }
        Update();
    }
}
