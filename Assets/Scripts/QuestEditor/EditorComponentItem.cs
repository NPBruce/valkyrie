using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentItem : EditorComponent
{
    QuestData.Item itemComponent;
    EditorSelectionList itemESL;
    EditorSelectionList traitESL;

    public EditorComponentItem(string nameIn) : base()
    {
        Game game = Game.Get();
        itemComponent = game.quest.qd.components[nameIn] as QuestData.Item;
        component = itemComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), CommonStringKeys.ITEM, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), 
            new StringKey(name.Substring("Item".Length),false), delegate { QuestEditorData.ListItem(); });
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
                new StringKey(itemComponent.itemName[i],false));
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
                new StringKey(itemComponent.traits[i],false));
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
            QuestData.Item i = kv.Value as QuestData.Item;
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
            string display = kv.Key;
            List<string> sets = new List<string>(kv.Value.traits);
            foreach (string s in kv.Value.sets)
            {
                if (s.Length == 0)
                {
                    sets.Add("base");
                }
                else
                {
                    display += " " + s;
                    sets.Add(s);
                }
            }

            if (usedItems.Contains(kv.Key))
            {
                items.Add(new EditorSelectionList.SelectionListEntry(display, sets, Color.grey));
            }
            else
            {
                items.Add(new EditorSelectionList.SelectionListEntry(display, sets, Color.white));
            }
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
            list.Add(new EditorSelectionList.SelectionListEntry(s));
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
