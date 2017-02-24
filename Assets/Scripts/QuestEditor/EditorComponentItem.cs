using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        name = component.name;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(3, 1), "Item", delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 0), new Vector2(16, 1), name.Substring("Item".Length), delegate { QuestEditorData.ListItem(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), "E", delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1), "Item:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 2), new Vector2(17, 1), itemComponent.itemName, delegate { SetItem(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 4), new Vector2(16, 1), "Traits:");
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(17, 4), new Vector2(1, 1), "+", delegate { AddTrait(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        for (int i = 0; i < itemComponent.traits.Length; i++)
        {
            int tmp = i;
            db = new DialogBox(new Vector2(0, 5 + i), new Vector2(16, 1), itemComponent.traits[i]);
            db.ApplyTag("editor");

            if (itemComponent.traits.Length > 1 || itemComponent.itemName.Length > 0)
            {
                tb = new TextButton(new Vector2(17, 5 + i), new Vector2(1, 1), "-", delegate { RemoveTrait(tmp); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
    }


    public void SetItem()
    {
        Game game = Game.Get();
        Dictionary<string, Color> items = new Dictionary<string, Color>();

        if (itemComponent.traits.Length > 0)
        {
            items.Add("", Color.white);
        }

        HashSet<string> usedItems = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.Item i = kv.Value as QuestData.Item;
            if (i != null)
            {
                usedItems.Add(i.itemName);
            }
        }

        foreach (KeyValuePair<string, ItemData> kv in game.cd.items)
        {
            string display = kv.Key;
            foreach (string s in kv.Value.sets)
            {
                display += " " + s;
            }

            if (usedItems.Contains(kv.Key))
            {
                items.Add(display, Color.grey);
            }
            else
            {
                items.Add(display, Color.white);
            }
        }
        itemESL = new EditorSelectionList("Select Item", items, delegate { SelectSetItem(); });
        itemESL.SelectItem();
    }

    public void SelectSetItem()
    {
        itemComponent.itemName = itemESL.selection;
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

        List<string> list = new List<string>(traits);
        traitESL = new EditorSelectionList("Select Trait", list, delegate { SelectAddTrait(); });
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
