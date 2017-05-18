using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentSpawn : EditorComponentEvent
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
    PaneledDialogBoxEditable uniqueTextDBE;
    DialogBoxEditable healthDBE;
    DialogBoxEditable healthHeroDBE;

    EditorSelectionList monsterTypeESL;
    EditorSelectionList monsterTraitESL;
    EditorSelectionList monsterPlaceESL;

    public EditorComponentSpawn(string nameIn) : base(nameIn)
    {
    }

    override public void AddLocationType(float offset)
    {
        TextButton tb = null;
        if (!component.locationSpecified)
        {
            tb = new TextButton(new Vector2(14, offset), new Vector2(4, 1), POSITION_TYPE_UNUSED, delegate { PositionTypeCycle(); });
        }
        else
        {
            tb = new TextButton(new Vector2(14, offset), new Vector2(4, 1), POSITION_TYPE_HIGHLIGHT, delegate { PositionTypeCycle(); });
        }
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
    }
    
    override public float AddSubEventComponents(float offset)
    {
        spawnComponent = component as QuestData.Spawn;

        DialogBox db = null;
        TextButton tb = null;
        if (game.gameType is D2EGameType)
        {
            db = new DialogBox(new Vector2(0, offset), new Vector2(6, 1), new StringKey("val", "X_COLON", MONSTER_UNIQUE));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            if (!spawnComponent.unique)
            {
                tb = new TextButton(new Vector2(6, offset), new Vector2(3, 1), new StringKey("val", "FALSE"), delegate { UniqueToggle(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
                offset += 2;
            }
            else
            {
                tb = new TextButton(new Vector2(6, offset), new Vector2(3, 1), new StringKey("val", "TRUE"), delegate { UniqueToggle(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
                offset += 2;

                db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "X_COLON", UNIQUE_TITLE));
                db.background.transform.parent = scrollArea.transform;
                db.ApplyTag(Game.EDITOR);

                uniqueTitleDBE = new DialogBoxEditable(
                    new Vector2(5, offset), new Vector2(14.5f, 1),
                    spawnComponent.uniqueTitle.Translate(), false, 
                    delegate { UpdateUniqueTitle(); });
                uniqueTitleDBE.background.transform.parent = scrollArea.transform;
                uniqueTitleDBE.ApplyTag(Game.EDITOR);
                uniqueTitleDBE.AddBorder();
                offset += 2;

                db = new DialogBox(new Vector2(0, offset++), new Vector2(20, 1), new StringKey("val", "X_COLON", UNIQUE_INFO));
                db.background.transform.parent = scrollArea.transform;
                db.ApplyTag(Game.EDITOR);

                uniqueTextDBE = new PaneledDialogBoxEditable(
                    new Vector2(0.5f, offset), new Vector2(19, 8), 
                spawnComponent.uniqueText.Translate(),
                delegate { UpdateUniqueText(); });
                uniqueTextDBE.background.transform.parent = scrollArea.transform;
                uniqueTextDBE.ApplyTag(Game.EDITOR);
                uniqueTextDBE.AddBorder();
                offset += 9;
            }
        }

        db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "X_COLON", HEALTH));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        // Dumbers dont need translation
        healthDBE = new DialogBoxEditable(new Vector2(5, offset), new Vector2(3, 1),
            spawnComponent.uniqueHealthBase.ToString(), false, delegate { UpdateHealth(); });
        healthDBE.background.transform.parent = scrollArea.transform;
        healthDBE.ApplyTag(Game.EDITOR);
        healthDBE.AddBorder();

        db = new DialogBox(new Vector2(8, offset), new Vector2(7, 1), new StringKey("val", "X_COLON", HEALTH_HERO));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        // Numbers dont need translation
        healthHeroDBE = new DialogBoxEditable(new Vector2(15, offset), new Vector2(3, 1),
            spawnComponent.uniqueHealthHero.ToString(), false, delegate { UpdateHealthHero(); });
        healthHeroDBE.background.transform.parent = scrollArea.transform;
        healthHeroDBE.ApplyTag(Game.EDITOR);
        healthHeroDBE.AddBorder();
        offset += 2;

        db = new DialogBox(new Vector2(1.5f, offset), new Vector2(15, 1), new StringKey("val", "X_COLON", TYPES));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(16.5f, offset++), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTypeAdd(0); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        int i = 0;
        for (i = 0; i < spawnComponent.mTypes.Length; i++)
        {
            int mSlot = i;
            string mName = spawnComponent.mTypes[i];
            if (mName.IndexOf("Monster") == 0)
            {
                mName = mName.Substring("Monster".Length);
            }

            tb = new TextButton(new Vector2(0.5f, offset), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterTypeRemove(mSlot); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(1.5f, offset), new Vector2(15, 1), new StringKey(null,mName,false), delegate { MonsterTypeReplace(mSlot); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(16.5f, offset), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTypeAdd(mSlot + 1); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
            offset++;
        }
        offset++;

        float traitOffset = offset;
        db = new DialogBox(new Vector2(0.5f, offset), new Vector2(8, 1), REQ_TRAITS);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(8.5f, offset++), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTraitsAdd(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        for (i = 0; i < spawnComponent.mTraitsRequired.Length; i++)
        {
            int mSlot = i;
            string mName = spawnComponent.mTraitsRequired[i];

            tb = new TextButton(new Vector2(0.5f, offset), new Vector2(1, 1), 
                CommonStringKeys.MINUS, delegate { MonsterTraitsRemove(mSlot); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(1.5f, offset), new Vector2(8, 1), 
                new StringKey("val", mName), delegate { MonsterTraitReplace(mSlot); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
            offset++;
        }

        db = new DialogBox(new Vector2(10.5f, traitOffset), new Vector2(8, 1), POOL_TRAITS);
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(18.5f, traitOffset++), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterTraitsAdd(true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        for (int j = 0; j < spawnComponent.mTraitsPool.Length; j++)
        {
            int mSlot = j;
            string mName = spawnComponent.mTraitsPool[j];

            tb = new TextButton(new Vector2(10.5f, traitOffset), 
                new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterTraitsRemove(mSlot, true); }, Color.red);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(11.5f, traitOffset), 
                new Vector2(8, 1), new StringKey("val", mName), delegate { MonsterTraitReplace(mSlot, true); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
            traitOffset++;
        }

        if (traitOffset > offset) offset = traitOffset;

        if (game.gameType is D2EGameType)
        {
            offset = AddPlacementComponenets(offset);
        }

        return offset + 1;
    }

    public float AddPlacementComponenets(float offset)
    {
        DialogBox db = null;
        TextButton tb = null;
        for (int heroes = 2; heroes < 5; heroes++)
        {
            int h = heroes;
            db = new DialogBox(new Vector2(0, offset), new Vector2(5, 1), new StringKey("val", "NUMBER_HEROS", heroes));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { MonsterPlaceAdd(h); }, Color.green);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);

            for (int i = 0; i < spawnComponent.placement[heroes].Length; i++)
            {
                int mSlot = i;
                string place = spawnComponent.placement[heroes][i];

                tb = new TextButton(new Vector2(0, offset), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { MonsterPlaceRemove(h, mSlot); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);

                tb = new TextButton(new Vector2(1, offset), new Vector2(19, 1), 
                    new StringKey(null, place,false), delegate { QuestEditorData.SelectComponent(place); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
                offset++;
            }
            offset++;
        }

        return offset + 1;
    }

    override public void PositionTypeCycle()
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
        if (uniqueTitleDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(spawnComponent.uniquetitle_key, uniqueTitleDBE.Text);
        }
    }

    public void UpdateUniqueText()
    {
        if (uniqueTextDBE.CheckTextChangedAndNotEmpty())
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

    public void MonsterPlaceAdd(int heroes)
    {
        Game game = Game.Get();

        List<EditorSelectionList.SelectionListEntry> mplaces = new List<EditorSelectionList.SelectionListEntry>();
        mplaces.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(
            new StringKey("val","NEW_X",CommonStringKeys.MPLACE).Translate(),"{NEW:MPlace}"));
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.MPlace)
            {
                mplaces.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }

        if (mplaces.Count == 0)
        {
            return;
        }
        monsterPlaceESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, mplaces, delegate { MonsterPlaceAddSelection(heroes); });
        monsterPlaceESL.SelectItem();
    }

    public void MonsterPlaceAddSelection(int heroes)
    {
        if (monsterPlaceESL.selection.Equals("{NEW:MPlace}"))
        {
            Game game = Game.Get();
            int index = 0;

            while (game.quest.qd.components.ContainsKey("MPlace" + index))
            {
                index++;
            }
            game.quest.qd.components.Add("MPlace" + index, new QuestData.MPlace("MPlace" + index));
            monsterPlaceESL.selection = "MPlace" + index;
        }

        string[] newM = new string[spawnComponent.placement[heroes].Length + 1];
        int i;
        for (i = 0; i < spawnComponent.placement[heroes].Length; i++)
        {
            newM[i] = spawnComponent.placement[heroes][i];
        }

        newM[i] = monsterPlaceESL.selection;
        spawnComponent.placement[heroes] = newM;
        Update();
    }

    public void MonsterPlaceRemove(int heroes, int pos)
    {
        string[] newM = new string[spawnComponent.placement[heroes].Length - 1];

        int j = 0;
        for (int i = 0; i < spawnComponent.placement[heroes].Length; i++)
        {
            if (i != pos || i != j)
            {
                newM[j] = spawnComponent.placement[heroes][i];
                j++;
            }
        }
        spawnComponent.placement[heroes] = newM;
        Update();
    }
}
