using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentStartingItem : EditorComponent
{
    QuestData.StartingItem itemComponent;
    EditorSelectionList itemESL;
    EditorSelectionList traitESL;

    public EditorComponentStartingItem(string nameIn) : base()
    {
        Game game = Game.Get();
        itemComponent = game.quest.qd.components[nameIn] as QuestData.StartingItem;
        component = itemComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        //Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(5, 1), CommonStringKeys.STARTING_ITEM, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(5, 0), new Vector2(14, 1), 
            new StringKey(null,name.Substring("StartingItem".Length),false), delegate { QuestEditorData.ListStartingItem(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(19, 1), new StringKey("val","X_COLON",CommonStringKeys.ITEM));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 2), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddItem(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        float offset = 3;
        for (int i = 0; i < itemComponent.itemName.Length; i++)
        {
            int tmp = i;
            db = new DialogBox(new Vector2(0, offset), new Vector2(19, 1), 
                new StringKey(null,itemComponent.itemName[i],false));
            db.ApplyTag("editor");

            if (itemComponent.traits.Length > 0 || itemComponent.itemName.Length > 1)
            {
                tb = new TextButton(new Vector2(19, offset), new Vector2(1, 1), CommonStringKeys.MINUS , delegate { RemoveItem(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
            offset++;
        }

        offset++;

        db = new DialogBox(new Vector2(0, offset), new Vector2(16, 1), new StringKey("val", "X_COLON", CommonStringKeys.TRAITS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(17, offset++), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddTrait(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int i = 0; i < itemComponent.traits.Length; i++)
        {
            int tmp = i;
            db = new DialogBox(new Vector2(0, offset), new Vector2(16, 1), 
                new StringKey("val",itemComponent.traits[i]));
            db.ApplyTag("editor");

            if (itemComponent.traits.Length > 1 || itemComponent.itemName.Length > 0)
            {
                tb = new TextButton(new Vector2(17, offset), new Vector2(1, 1), CommonStringKeys.MINUS, delegate { RemoveTrait(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
            offset++;
        }
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
            QuestData.StartingItem i = kv.Value as QuestData.StartingItem;
            if (i != null)
            {
                if (i.traits.Length == 0)
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

    public void AddTrait()
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
        traitESL = new EditorSelectionList(new StringKey("val","SELECT",CommonStringKeys.TRAITS), list, delegate { SelectAddTrait(); });
        traitESL.SelectItem();
    }

    public void SelectAddTrait()
    {
        string[] newArray = new string[itemComponent.traits.Length + 1];

        for (int i = 0; i < itemComponent.traits.Length; i++)
        {
            newArray[i] = itemComponent.traits[i];
        }
        newArray[itemComponent.traits.Length] = traitESL.selection;
        itemComponent.traits = newArray;
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
}
