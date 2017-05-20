using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class EditorComponentCustomMonster : EditorComponent
{

    private readonly StringKey BASE = new StringKey("val", "BASE");
    private readonly StringKey NAME = new StringKey("val", "NAME");
    private readonly StringKey ACTIVATIONS = new StringKey("val","ACTIVATIONS");
    private readonly StringKey INFO = new StringKey("val", "INFO");
    private readonly StringKey HEALTH = new StringKey("val", "HEALTH");
    private readonly StringKey HEALTH_HERO = new StringKey("val", "HEALTH_HERO");
    private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");
    private readonly StringKey PLACE_IMG = new StringKey("val", "PLACE_IMG");
    private readonly StringKey IMAGE = new StringKey("val", "IMAGE");

    QuestData.CustomMonster monsterComponent;
    
    DialogBoxEditable nameDBE;
    PaneledDialogBoxEditable infoDBE;
    DialogBoxEditable healthDBE;
    DialogBoxEditable healthHeroDBE;
    EditorSelectionList baseESL;
    EditorSelectionList activationsESL;
    EditorSelectionList traitsESL;
    EditorSelectionList imageESL;
    EditorSelectionList placeESL;

    // TODO: Translate expansion traits, translate base monster names.

    public EditorComponentCustomMonster(string nameIn) : base()

    {
        Game game = Game.Get();
        monsterComponent = game.quest.qd.components[nameIn] as QuestData.CustomMonster;
        component = monsterComponent;
        name = component.sectionName;
        Update();
    }
    
    override public float AddSubComponents(float offset)
    {
        Game game = Game.Get();

        UIElement ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", BASE));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(3, offset, 16.5f, 1);
        ui.SetText(monsterComponent.baseMonster);
        ui.SetButton(delegate { SetBase(); });
        new UIElementBorder(ui);
        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", NAME));

        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.monsterName.KeyExists())
        {
            nameDBE = new DialogBoxEditable(
                new Vector2(3, offset), new Vector2(13.5f, 1), 
                monsterComponent.monsterName.Translate(), false,
                delegate { UpdateName(); });
            nameDBE.background.transform.SetParent(scrollArea.transform);
            nameDBE.ApplyTag(Game.EDITOR);
            nameDBE.AddBorder();
            if (monsterComponent.baseMonster.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.RESET);
                ui.SetButton(delegate { ClearName(); });
                new UIElementBorder(ui);
            }
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.SET);
            ui.SetButton(delegate { SetName(); });
            new UIElementBorder(ui);
        }
        offset += 2;

        if (game.gameType is D2EGameType)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0.5f, offset, 16, 1);
            ui.SetText(new StringKey("val", "X_COLON", INFO));

            if (monsterComponent.baseMonster.Length == 0 || monsterComponent.info.KeyExists())
            {
                infoDBE = new PaneledDialogBoxEditable(
                    new Vector2(0.5f, offset + 1), new Vector2(19, 8), 
                    monsterComponent.info.Translate(),
                    delegate { UpdateInfo(); });
                infoDBE.background.transform.SetParent(scrollArea.transform);
                infoDBE.ApplyTag(Game.EDITOR);
                infoDBE.AddBorder();
                if (monsterComponent.baseMonster.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.transform);
                    ui.SetLocation(16.5f, offset, 3, 1);
                    ui.SetText(CommonStringKeys.RESET);
                    ui.SetButton(delegate { ClearInfo(); });
                    new UIElementBorder(ui);
                }
                offset += 10;
            }
            else
            {
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.SET);
                ui.SetButton(delegate { SetInfo(); });
                new UIElementBorder(ui);
                offset += 2;
            }
        }

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 11.5f, 1);
        ui.SetText(new StringKey("val", "X_COLON", ACTIVATIONS));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(11.5f, offset, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddActivation(); });
        new UIElementBorder(ui, Color.green);

        float traitOffset = offset;
        offset += 1;
        int index;
        for (index = 0; index < monsterComponent.activations.Length; index++)
        {
            int i = index;

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0, offset, 11.5f, 1);
            ui.SetText(monsterComponent.activations[index]);
            ui.SetButton(delegate { QuestEditorData.SelectComponent(monsterComponent.activations[i]); });
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(11.5f, offset, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveActivation(i); });
            new UIElementBorder(ui, Color.red);
            offset += 1;
        }

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(12.5f, traitOffset, 6, 1);
        ui.SetText(new StringKey("val", "X_COLON", CommonStringKeys.TRAITS));

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(18.5f, traitOffset++, 1, 1);
        ui.SetText(CommonStringKeys.PLUS, Color.green);
        ui.SetButton(delegate { AddTrait(); });
        new UIElementBorder(ui, Color.green);

        for (index = 0; index < monsterComponent.traits.Length; index++)
        {
            int i = index;
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(12.5f, traitOffset, 6, 1);
            ui.SetText(new StringKey("val", monsterComponent.traits[index]));
            new UIElementBorder(ui);

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(18.5f, traitOffset++, 1, 1);
            ui.SetText(CommonStringKeys.MINUS, Color.red);
            ui.SetButton(delegate { RemoveTrait(i); });
            new UIElementBorder(ui, Color.red);
        }

        if (traitOffset > offset) offset = traitOffset;
        offset += 1;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 5, 1);
        ui.SetText(new StringKey("val", "X_COLON", HEALTH));

        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.healthDefined)
        {
            healthDBE = new DialogBoxEditable(new Vector2(5, offset), new Vector2(3, 1), 
                monsterComponent.healthBase.ToString(), false, delegate { UpdateHealth(); });
            healthDBE.background.transform.SetParent(scrollArea.transform);
            healthDBE.ApplyTag(Game.EDITOR);
            healthDBE.AddBorder();

            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(8, offset, 6, 1);
            ui.SetText(new StringKey("val", "X_COLON", HEALTH_HERO));

            healthHeroDBE = new DialogBoxEditable(new Vector2(14, offset), new Vector2(2.5f, 1), 
                monsterComponent.healthPerHero.ToString(), false, delegate { UpdateHealthHero(); });
            healthHeroDBE.background.transform.SetParent(scrollArea.transform);
            healthHeroDBE.ApplyTag(Game.EDITOR);
            healthHeroDBE.AddBorder();
            if (monsterComponent.baseMonster.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.RESET);
                ui.SetButton(delegate { ClearHealth(); });
                new UIElementBorder(ui);
            }
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.SET);
            ui.SetButton(delegate { SetHealth(); });
            new UIElementBorder(ui);
        }

        offset += 2;

        ui = new UIElement(Game.EDITOR, scrollArea.transform);
        ui.SetLocation(0, offset, 3, 1);
        ui.SetText(new StringKey("val", "X_COLON", IMAGE));
        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.imagePath.Length > 0)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(3, offset, 13.5f, 1);
            ui.SetText(monsterComponent.imagePath);
            ui.SetButton(delegate { SetImage(); });
            new UIElementBorder(ui);
            if (monsterComponent.baseMonster.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.RESET);
                ui.SetButton(delegate { ClearImage(); });
                new UIElementBorder(ui);
            }
        }
        else
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(16.5f, offset, 3, 1);
            ui.SetText(CommonStringKeys.SET);
            ui.SetButton(delegate { SetImage(); });
            new UIElementBorder(ui);
        }
        offset += 2;

        if (game.gameType is D2EGameType)
        {
            ui = new UIElement(Game.EDITOR, scrollArea.transform);
            ui.SetLocation(0, offset, 4, 1);
            ui.SetText(new StringKey("val", "X_COLON", PLACE_IMG));
            if (monsterComponent.baseMonster.Length == 0 || monsterComponent.imagePlace.Length > 0)
            {
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(4, offset, 12.5f, 1);
                ui.SetText(monsterComponent.imagePlace);
                ui.SetButton(delegate { SetImagePlace(); });
                new UIElementBorder(ui);
                if (monsterComponent.baseMonster.Length > 0)
                {
                    ui = new UIElement(Game.EDITOR, scrollArea.transform);
                    ui.SetLocation(16.5f, offset, 3, 1);
                    ui.SetText(CommonStringKeys.RESET);
                    ui.SetButton(delegate { ClearImagePlace(); });
                    new UIElementBorder(ui);
                }
            }
            else
            {
                ui = new UIElement(Game.EDITOR, scrollArea.transform);
                ui.SetLocation(16.5f, offset, 3, 1);
                ui.SetText(CommonStringKeys.SET);
                ui.SetButton(delegate { SetImagePlace(); });
                new UIElementBorder(ui);
            }
        }
        offset += 2;

        return offset;
    }

    public void SetBase()
    {
        List<EditorSelectionList.SelectionListEntry> baseMonster = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        baseMonster.Add(EditorSelectionList.SelectionListEntry.BuildNameKeyItem(CommonStringKeys.NONE.Translate(),"{NONE}"));
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
            baseMonster.Add(
                EditorSelectionList.SelectionListEntry.BuildNameKeyTraitsItem(
                    localizedDisplay.ToString(),display.ToString(), sets));
        }

        baseESL = new EditorSelectionList(
            new StringKey("val", "SELECT", CommonStringKeys.EVENT),
            baseMonster, delegate { SelectSetBase(); });
        baseESL.SelectItem();
    }

    public void SelectSetBase()
    {
        if (baseESL.selection.Equals("{NONE}"))
        {
            monsterComponent.baseMonster = "";
            if (!monsterComponent.monsterName.KeyExists())
            {
                SetName();
            }
            if (!monsterComponent.info.KeyExists())
            {
                SetInfo();
            }
            if (!monsterComponent.healthDefined)
            {
                SetHealth();
            }
        }
        else
        {
            monsterComponent.baseMonster = baseESL.selection.Split(" ".ToCharArray())[0];
        }
        Update();
    }

    public void UpdateName()
    {
        if (nameDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(monsterComponent.monstername_key, nameDBE.Text);
        }
    }

    public void ClearName()
    {
        LocalizationRead.scenarioDict.Remove(monsterComponent.monstername_key);
        Update();
    }

    public void SetName()
    {
        LocalizationRead.updateScenarioText(monsterComponent.monstername_key, NAME.Translate());
        Update();
    }

    public void UpdateInfo()
    {
        if (infoDBE.CheckTextChangedAndNotEmpty())
        {
            LocalizationRead.updateScenarioText(monsterComponent.info_key, infoDBE.Text);
        }
    }

    public void ClearInfo()
    {
        LocalizationRead.scenarioDict.Remove(monsterComponent.info_key);
        Update();
    }

    public void SetInfo()
    {
        LocalizationRead.updateScenarioText(monsterComponent.info_key, INFO.Translate());
        Update();
    }

    public void AddActivation()
    {
        List<EditorSelectionList.SelectionListEntry> activations = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        foreach (KeyValuePair<string, QuestData.QuestComponent> kv in game.quest.qd.components)
        {
            if (kv.Value is QuestData.Activation)
            {
                activations.Add(new EditorSelectionList.SelectionListEntry(kv.Key.Substring("Activation".Length)));
            }
        }

        activationsESL = new EditorSelectionList(
            new StringKey("val", "SELECT", CommonStringKeys.ACTIVATION),
            activations, delegate { SelectAddActivation(); });
        activationsESL.SelectItem();
    }

    public void SelectAddActivation()
    {
        string[] newA = new string[monsterComponent.activations.Length + 1];
        int i;
        for (i = 0; i < monsterComponent.activations.Length; i++)
        {
            newA[i] = monsterComponent.activations[i];
        }

        newA[i] = activationsESL.selection;
        monsterComponent.activations = newA;
        Update();
    }

    public void RemoveActivation(int index)
    {
        string[] newA = new string[monsterComponent.activations.Length - 1];

        int j = 0;
        for (int i = 0; i < monsterComponent.activations.Length; i++)
        {
            if (i != index)
            {
                newA[j++] = monsterComponent.activations[i];
            }
        }

        monsterComponent.activations = newA;
        Update();
    }

    public void AddTrait()
    {
        HashSet<string> traits = new HashSet<string>();

        Game game = Game.Get();
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
        traitsESL = new EditorSelectionList(
            new StringKey("val","SELECT",CommonStringKeys.ACTIVATION), 
            list, delegate { SelectAddTraits(); });
        traitsESL.SelectItem();
    }

    public void SelectAddTraits()
    {
        string[] newT = new string[monsterComponent.traits.Length + 1];
        int i;
        for (i = 0; i < monsterComponent.traits.Length; i++)
        {
            newT[i] = monsterComponent.traits[i];
        }

        newT[i] = traitsESL.selection;
        monsterComponent.traits = newT;
        Update();
    }

    public void RemoveTrait(int index)
    {
        string[] newT = new string[monsterComponent.traits.Length - 1];

        int j = 0;
        for (int i = 0; i < monsterComponent.traits.Length; i++)
        {
            if (i != index)
            {
                newT[j++] = monsterComponent.traits[i];
            }
        }

        monsterComponent.traits = newT;
        Update();
    }

    public void UpdateHealth()
    {
        float.TryParse(healthDBE.Text, out monsterComponent.healthBase);
    }

    public void UpdateHealthHero()
    {
        float.TryParse(healthHeroDBE.Text, out monsterComponent.healthPerHero);
    }

    public void ClearHealth()
    {
        monsterComponent.healthBase = 0;
        monsterComponent.healthPerHero = 0;
        monsterComponent.healthDefined = false;
        Update();
    }

    public void SetHealth()
    {
        monsterComponent.healthBase = 0;
        monsterComponent.healthPerHero = 0;
        monsterComponent.healthDefined = true;
        Update();
    }

    public void SetImage()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }
        imageESL = new EditorSelectionList(SELECT_IMAGE, list, delegate { SelectImage(); });
        imageESL.SelectItem();
    }

    public void SelectImage()
    {
        monsterComponent.imagePath = imageESL.selection;
        Update();
    }

    public void ClearImage()
    {
        monsterComponent.imagePath = "";
        Update();
    }

    public void SetImagePlace()
    {
        string relativePath = new FileInfo(Path.GetDirectoryName(Game.Get().quest.qd.questPath)).FullName;
        List<EditorSelectionList.SelectionListEntry> list = new List<EditorSelectionList.SelectionListEntry>();
        foreach (string s in Directory.GetFiles(relativePath, "*.png", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }
        foreach (string s in Directory.GetFiles(relativePath, "*.jpg", SearchOption.AllDirectories))
        {
            list.Add(new EditorSelectionList.SelectionListEntry(s.Substring(relativePath.Length + 1)));
        }
        placeESL = new EditorSelectionList(SELECT_IMAGE, list, delegate { SelectImagePlace(); });
        placeESL.SelectItem();
    }

    public void SelectImagePlace()
    {
        monsterComponent.imagePlace = placeESL.selection;
        Update();
    }

    public void ClearImagePlace()
    {
        monsterComponent.imagePlace = "";
        Update();
    }
}
