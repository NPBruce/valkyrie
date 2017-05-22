using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentItem : EditorComponent
{
    QuestData.QItem itemComponent;
    EditorSelectionList traitESL;

    EditorSelectionList itemESL;

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

        UIElement ui = null;
        if (game.gameType is MoMGameType)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 8, 1);
            ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.STARTING_ITEM));

            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(8, offset, 4, 1);
            ui.SetText(itemComponent.starting.ToString());
            ui.SetButton(delegate { ToggleStarting(); });
            new UIElementBorder(ui);
            offset += 2;
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0.5f, offset, 18, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.ITEM));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, offset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddItem(); });
        new UIElementBorder(ui, Color.green);

        for (int i = 0; i < itemComponent.itemName.Length; i++)
        {
            int tmp = i;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0.5f, offset, 18, 1);
            ui.SetText(itemComponent.itemName[i]);

            if (itemComponent.traits.Length > 0 || itemComponent.itemName.Length > 1 || itemComponent.traitpool.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveItem(tmp); });
                new UIElementBorder(ui, Color.red);
            }
            offset++;
        }

        offset++;

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(0, offset, 9, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TRAITS));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(9, offset, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTrait(); });
        new UIElementBorder(ui, Color.green);

        float traitOffset = offset;
        offset++;

        for (int i = 0; i < itemComponent.traits.Length; i++)
        {
            int tmp = i;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(0, offset, 9, 1);
            ui.SetText(new StringKey("val", itemComponent.traits[i]));

            if (itemComponent.traits.Length > 1 || itemComponent.itemName.Length > 0 || itemComponent.traitpool.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(9, offset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveTrait(tmp); });
                new UIElementBorder(ui, Color.red);
            }
            offset++;
        }

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(10, traitOffset, 8.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", new StringKey("val", "POOL_TRAITS")));

        ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
        ui.SetLocation(18.5f, traitOffset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTrait(true); });
        new UIElementBorder(ui, Color.green);

        for (int i = 0; i < itemComponent.traitpool.Length; i++)
        {
            int tmp = i;
            ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
            ui.SetLocation(10, traitOffset, 8.5f, 1);
            ui.SetText(new StringKey("val", itemComponent.traitpool[i]));

            if (itemComponent.traitpool.Length > 1 || itemComponent.itemName.Length > 0 || itemComponent.traits.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.GetScrollTransform());
                ui.SetLocation(18.5f, traitOffset, 1, 1);
                ui.SetText(CommonStringKeys.MINUS, Color.red);
                ui.SetButton(delegate { RemoveTraitPool(tmp); });
                new UIElementBorder(ui, Color.red);
            }
            traitOffset++;
        }
        if (offset < traitOffset) offset = traitOffset;
        offset++;

        return AddInspect(offset);
    }

    public float AddInspect(float offset)
    {
        if (!(game.gameType is MoMGameType)) return offset;

        DialogBox db = new DialogBox(new Vector2(0, offset), new Vector2(6f, 1), new StringKey("val", "X_COLON", new StringKey("val", "INSPECT")));
        db.ApplyTag(Game.EDITOR);

        TextButton tb = new TextButton(new Vector2(6f, offset), new Vector2(12, 1), new StringKey(null, itemComponent.inspect, false), delegate { PickInpsect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.background.transform.SetParent(scrollArea.GetScrollTransform());
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
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        Game game = Game.Get();
        UIWindowSelectionListTraits select = new UIWindowSelectionListTraits(SelectAddItem, CommonStringKeys.SELECT_ITEM);

        Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();
        traits.Add(new StringKey("val", "EXPANSION").Translate(), new string[] { "Quest" });

        HashSet<string> usedItems = new HashSet<string>();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            QuestData.QItem i = kv.Value as QuestData.QItem;
            if (i != null)
            {
                select.AddItem(i.sectionName, traits);
                if (i.traits.Length == 0 && i.traitpool.Length == 0)
                {
                    usedItems.Add(i.itemName[0]);
                }
            }
        }

        foreach (KeyValuePair<string, ItemData> kv in game.cd.items)
        {
            if (usedItems.Contains(kv.Key))
            {
                select.AddItem(kv.Value, Color.grey);
            }
            else
            {
                select.AddItem(kv.Value);
            }
        }
        select.Draw();
    }

    public void SelectAddItem(string item)
    {
        string[] newArray = new string[itemComponent.itemName.Length + 1];

        for (int i = 0; i < itemComponent.itemName.Length; i++)
        {
            newArray[i] = itemComponent.itemName[i];
        }
        newArray[itemComponent.itemName.Length] = item;
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
