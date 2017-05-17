using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentItem : EditorComponent
{
    QuestData.QItem itemComponent;
    EditorSelectionList itemESL;
    EditorSelectionList traitESL;

    public EditorComponentItem(string nameIn) : base()
    {
        Game game = Game.Get();
        itemComponent = game.quest.qd.components[nameIn] as QuestData.QItem;
        component = itemComponent;
        name = component.sectionName;
        Update();
    }
    
    override public float AddSubComponents(float offset)
    {
        Game game = Game.Get();

        TextButton tb = null;
        DialogBox db = null;
        if (game.gameType is MoMGameType)
        {
            db = new DialogBox(new Vector2(0, offset), new Vector2(8, 1), new StringKey("val","X_COLON",CommonStringKeys.STARTING_ITEM));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            tb = new TextButton(new Vector2(8, offset), new Vector2(4, 1), new StringKey(null, itemComponent.starting.ToString(), false), delegate { ToggleStarting(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.background.transform.parent = scrollArea.transform;
            tb.ApplyTag(Game.EDITOR);
            offset += 2;
        }

        db = new DialogBox(new Vector2(0.5f, offset), new Vector2(18, 1), new StringKey("val","X_COLON",CommonStringKeys.ITEM));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(18.5f, offset), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddItem(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        offset += 1;

        for (int i = 0; i < itemComponent.itemName.Length; i++)
        {
            int tmp = i;
            db = new DialogBox(new Vector2(0.5f, offset), new Vector2(18, 1), 
                new StringKey(null,itemComponent.itemName[i],false));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            if (itemComponent.traits.Length > 0 || itemComponent.itemName.Length > 1 || itemComponent.traitpool.Length > 0)
            {
                tb = new TextButton(new Vector2(18.5f, offset), new Vector2(1, 1), CommonStringKeys.MINUS , delegate { RemoveItem(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
            }
            offset++;
        }

        offset++;

        db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), new StringKey("val", "X_COLON", CommonStringKeys.TRAITS));
        db.background.transform.parent = scrollArea.transform;
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(9, offset), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddTrait(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        float traitOffset = offset;
        offset++;

        for (int i = 0; i < itemComponent.traits.Length; i++)
        {
            int tmp = i;
            db = new DialogBox(new Vector2(0, offset), new Vector2(9, 1), 
                new StringKey("val",itemComponent.traits[i]));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            if (itemComponent.traits.Length > 1 || itemComponent.itemName.Length > 0 || itemComponent.traitpool.Length > 0)
            {
                tb = new TextButton(new Vector2(9, offset), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { RemoveTrait(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
            }
            offset++;
        }

        db = new DialogBox(new Vector2(10, traitOffset), new Vector2(8.5f, 1), new StringKey("val", "X_COLON", new StringKey("val", "POOL_TRAITS")));
        db.ApplyTag(Game.EDITOR);

        tb = new TextButton(new Vector2(18.5f, traitOffset), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddTrait(true); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);
        traitOffset++;

        for (int i = 0; i < itemComponent.traitpool.Length; i++)
        {
            int tmp = i;
            db = new DialogBox(new Vector2(10, traitOffset), new Vector2(8.5f, 1),
                new StringKey("val", itemComponent.traitpool[i]));
            db.background.transform.parent = scrollArea.transform;
            db.ApplyTag(Game.EDITOR);

            if (itemComponent.traitpool.Length > 1 || itemComponent.itemName.Length > 0 || itemComponent.traits.Length > 0)
            {
                tb = new TextButton(new Vector2(18.5f, traitOffset), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { RemoveTraitPool(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.background.transform.parent = scrollArea.transform;
                tb.ApplyTag(Game.EDITOR);
            }
            traitOffset++;
        }
        if (offset < traitOffset) offset = traitOffset;
        offset++;

        return AddInspect(offset);
    }

    public float AddInspect(float offset)
    {
        if (!game.gameType is MoMGameType) return offset;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(6f, 1), new StringKey("val", "X_COLON", new StringKey("val", "INSPECT")));
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(6f, offset), new Vector2(12, 1), new StringKey(null, itemComponent.inspect, false), delegate { PickInpsect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.parent = scrollArea.transform;
        tb.ApplyTag(Game.EDITOR);

        return offset + 2;
    }

    public void ToggleStarting()
    {
        itemComponent.starting = !itemComponent.starting;
        Update();
    }

    public void AddItem()
    {
        Game game = Game.Get();
        List<EditorSelectionList.SelectionListEntry> items = new List<EditorSelectionList.SelectionListEntry>();

        if (itemComponent.traits.Length > 0)
        {
            items.Add(new EditorSelectionList.SelectionListEntry("", Color.white));
        }

        HashSet<string> usedItems = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.QItem i = kv.Value as QuestData.QItem;
            if (i != null)
            {
                items.Add(new EditorSelectionList.SelectionListEntry(i.sectionName, "Quest"));
                if (i.traits.Length == 0 && i.traitpool.Length == 0)
                {
                    usedItems.Add(i.itemName[0]);
                }
            }
        }

        foreach (KeyValuePair<string, ItemData> kv in game.cd.items)
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

            Color buttonColor = Color.white;
            if (usedItems.Contains(kv.Key))
            {
                buttonColor = Color.grey;

            }

            items.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyTraitsColorItem(
                localizedDisplay.ToString(), display.ToString(), sets, buttonColor));
        }
        itemESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, items, delegate { SelectAddItem(); });
        itemESL.SelectItem();
    }

    public void SelectAddItem()
    {
        string[] newArray = new string[itemComponent.itemName.Length + 1];

        for (int i = 0; i < itemComponent.itemName.Length; i++)
        {
            newArray[i] = itemComponent.itemName[i];
        }
        newArray[itemComponent.itemName.Length] = itemESL.selection;
        itemComponent.itemName = newArray;
        Update();
    }

    public void RemoveItem(int index)
    {
        string[] newArray = new string[itemComponent.itemName.Length - 1];

        int j = 0;
        for (int i = 0; i < itemComponent.itemName.Length; i++)
        {
            if (i != index)
            {
                newArray[j++] = itemComponent.itemName[i];
            }
        }
        itemComponent.itemName = newArray;
        Update();
    }

    public void AddTrait(bool pool = false)
    {
        Game game = Game.Get();
        HashSet<string> traits = new HashSet<string>();

        foreach (KeyValuePair<string, ItemData> kv in game.cd.items)
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
        traitESL = new EditorSelectionList(new StringKey("val","SELECT",CommonStringKeys.TRAITS), list, delegate { SelectAddTrait(pool); });
        traitESL.SelectItem();
    }

    public void SelectAddTrait(bool pool)
    {
        if (pool)
        {
            string[] newArray = new string[itemComponent.traitpool.Length + 1];

            for (int i = 0; i < itemComponent.traitpool.Length; i++)
            {
                newArray[i] = itemComponent.traitpool[i];
            }
            newArray[itemComponent.traitpool.Length] = traitESL.selection;
            itemComponent.traitpool = newArray;
        }
        else
        {
            string[] newArray = new string[itemComponent.traits.Length + 1];

            for (int i = 0; i < itemComponent.traits.Length; i++)
            {
                newArray[i] = itemComponent.traits[i];
            }
            newArray[itemComponent.traits.Length] = traitESL.selection;
            itemComponent.traits = newArray;
        }
        Update();
    }

    public void RemoveTrait(int index)
    {
        string[] newArray = new string[itemComponent.traits.Length - 1];

        int j = 0;
        for (int i = 0; i < itemComponent.traits.Length; i++)
        {
            if (i != index)
            {
                newArray[j++] = itemComponent.traits[i];
            }
        }
        itemComponent.traits = newArray;
        Update();
    }

    public void RemoveTraitPool(int index)
    {
        string[] newArray = new string[itemComponent.traitpool.Length - 1];

        int j = 0;
        for (int i = 0; i < itemComponent.traitpool.Length; i++)
        {
            if (i != index)
            {
                newArray[j++] = itemComponent.traitpool[i];
            }
        }
        itemComponent.traitpool = newArray;
        Update();
    }

    public void PickInpsect()
    {
        List<EditorSelectionList.SelectionListEntry> items = new List<EditorSelectionList.SelectionListEntry>();

        items.Add(new EditorSelectionList.SelectionListEntry("", Color.white));

        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if(kv.Value.typeDynamic.Equals("Event"))
            {
                items.Add(new EditorSelectionList.SelectionListEntry(kv.Key));
            }
        }

        itemESL = new EditorSelectionList(CommonStringKeys.SELECT_ITEM, items, delegate { SelectInspectEvent(); });
        itemESL.SelectItem();
    }

    public void SelectInspectEvent()
    {
        itemComponent.inspect = itemESL.selection;
        Update();
    }
}
