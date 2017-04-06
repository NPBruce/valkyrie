using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class EditorComponentCustomMonster : EditorComponent
{

    private readonly StringKey BASE = new StringKey("val", "BASE");
    private readonly StringKey NAME = new StringKey("val", "NAME");
    private readonly StringKey ACTIVATIONS = new StringKey("val","ACTIVATIONS");
    private readonly StringKey INFO = new StringKey("val", "INFO");
    private readonly StringKey HEALTH = new StringKey("val", "HEALTH");
    private readonly StringKey SELECT_IMAGE = new StringKey("val", "SELECT_IMAGE");
    private readonly StringKey PLACE_IMG = new StringKey("val", "PLACE_IMG");
    private readonly StringKey IMAGE = new StringKey("val", "IMAGE");

    QuestData.CustomMonster monsterComponent;
    
    DialogBoxEditable nameDBE;
    DialogBoxEditable infoDBE;
    DialogBoxEditable healthDBE;
    EditorSelectionList baseESL;
    EditorSelectionList activationsESL;
    EditorSelectionList traitsESL;
    EditorSelectionList imageESL;
    EditorSelectionList placeESL;


    public EditorComponentCustomMonster(string nameIn) : base()

    {
        Game game = Game.Get();
        monsterComponent = game.quest.qd.components[nameIn] as QuestData.CustomMonster;
        component = monsterComponent;
        name = component.sectionName;
        Update();
    }
    
    override public void Update()
    {
        base.Update();
        Game game = Game.Get();

        TextButton tb = new TextButton(new Vector2(0, 0), new Vector2(6, 1), CommonStringKeys.CUSTOM_MONSTER, delegate { QuestEditorData.TypeSelect(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleRight;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(6, 0), new Vector2(13, 1), new StringKey(name.Substring("CustomMonster".Length),false), delegate { QuestEditorData.ListCustomMonster(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
        tb.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 0), new Vector2(1, 1), CommonStringKeys.E, delegate { Rename(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");


        DialogBox db = new DialogBox(new Vector2(0, 2), new Vector2(3, 1),
            new StringKey("val", "X_COLON", BASE));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(3, 2), new Vector2(17, 1), new StringKey(monsterComponent.baseMonster,false),  delegate { SetBase(); });
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        db = new DialogBox(new Vector2(0, 4), new Vector2(3, 1),
            new StringKey("val", "X_COLON", NAME));
        db.ApplyTag("editor");
        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.monsterName.Length > 0)
        {
            nameDBE = new DialogBoxEditable(new Vector2(3, 4), new Vector2(14, 1), monsterComponent.monsterName, delegate { UpdateName(); });
            nameDBE.ApplyTag("editor");
            nameDBE.AddBorder();
            if (monsterComponent.baseMonster.Length > 0)
            {
                tb = new TextButton(new Vector2(17, 4), new Vector2(3, 1), 
                    CommonStringKeys.RESET, delegate { ClearName(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
        else
        {
            tb = new TextButton(new Vector2(17, 4), new Vector2(3, 1),
                CommonStringKeys.SET, delegate { SetName(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        if (game.gameType is D2EGameType)
        {
            db = new DialogBox(new Vector2(0, 6), new Vector2(17, 1), new StringKey("val","X_COLON",INFO));
            db.ApplyTag("editor");
            if (monsterComponent.baseMonster.Length == 0 || monsterComponent.info.key.Length > 0)
            {
                infoDBE = new DialogBoxEditable(new Vector2(0, 7), new Vector2(20, 8), 
                    monsterComponent.info.Translate(), delegate { UpdateInfo(); });
                infoDBE.ApplyTag("editor");
                infoDBE.AddBorder();
                if (monsterComponent.baseMonster.Length > 0)
                {
                    tb = new TextButton(new Vector2(17, 6), new Vector2(3, 1), CommonStringKeys.RESET, delegate { ClearInfo(); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");
                }
            }
            else
            {
                tb = new TextButton(new Vector2(17, 6), new Vector2(3, 1), CommonStringKeys.SET, delegate { SetInfo(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(0, 15), new Vector2(12, 1), new StringKey("val","X_COLON",ACTIVATIONS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(12, 15), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddActivation(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        int offset = 16;
        int index;
        for (index = 0; index < 6; index++)
        {
            if (monsterComponent.activations.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(0, offset), new Vector2(12, 1), 
                    new StringKey(monsterComponent.activations[index],false));
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(12, offset++), new Vector2(1, 1), 
                    CommonStringKeys.MINUS, delegate { RemoveActivation(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(13, 15), new Vector2(6, 1), new StringKey("val","X_COLON", CommonStringKeys.TRAITS));
        db.ApplyTag("editor");

        tb = new TextButton(new Vector2(19, 15), new Vector2(1, 1), CommonStringKeys.PLUS, delegate { AddTrait(); }, Color.green);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        tb.ApplyTag("editor");

        offset = 16;
        for (index = 0; index < 6; index++)
        {
            if (monsterComponent.traits.Length > index)
            {
                int i = index;
                db = new DialogBox(new Vector2(13, offset), new Vector2(6, 1),
                    new StringKey(monsterComponent.traits[index],false));
                db.AddBorder();
                db.ApplyTag("editor");
                tb = new TextButton(new Vector2(19, offset++), new Vector2(1, 1), CommonStringKeys.MINUS, 
                    delegate { RemoveTrait(i); }, Color.red);
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }


        if (game.gameType is MoMGameType)
        {
            db = new DialogBox(new Vector2(0, 22), new Vector2(3, 1), new StringKey("val","X_COLON",HEALTH));
            db.ApplyTag("editor");
            if (monsterComponent.baseMonster.Length == 0 || monsterComponent.healthDefined)
            {
                healthDBE = new DialogBoxEditable(new Vector2(3, 22), new Vector2(14, 1), 
                monsterComponent.health.ToString(), delegate { UpdateHealth(); });
                healthDBE.ApplyTag("editor");
                healthDBE.AddBorder();
                if (monsterComponent.baseMonster.Length > 0)
                {
                    tb = new TextButton(new Vector2(17, 22), new Vector2(3, 1), CommonStringKeys.RESET, delegate { ClearHealth(); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");
                }
            }
            else
            {
                tb = new TextButton(new Vector2(17, 22), new Vector2(3, 1), CommonStringKeys.SET, delegate { SetHealth(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }

        db = new DialogBox(new Vector2(0, 24), new Vector2(3, 1), new StringKey("val","X_COLON",IMAGE));
        db.ApplyTag("editor");
        if (monsterComponent.baseMonster.Length == 0 || monsterComponent.imagePath.Length > 0)
        {
            tb = new TextButton(new Vector2(3, 24), new Vector2(14, 1), 
                new StringKey(monsterComponent.imagePath,false), delegate { SetImage(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
            if (monsterComponent.baseMonster.Length > 0)
            {
                tb = new TextButton(new Vector2(17, 24), new Vector2(3, 1), CommonStringKeys.RESET, delegate { ClearImage(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
        else
        {
            tb = new TextButton(new Vector2(17, 24), new Vector2(3, 1), CommonStringKeys.SET, delegate { SetImage(); });
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.ApplyTag("editor");
        }

        if (game.gameType is D2EGameType)
        {
            db = new DialogBox(new Vector2(0, 26), new Vector2(4, 1), PLACE_IMG);
            db.ApplyTag("editor");
            if (monsterComponent.baseMonster.Length == 0 || monsterComponent.imagePlace.Length > 0)
            {
                tb = new TextButton(new Vector2(4, 26), new Vector2(13, 1), new StringKey(monsterComponent.imagePath,false), delegate { SetImagePlace(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
                if (monsterComponent.baseMonster.Length > 0)
                {
                    tb = new TextButton(new Vector2(17, 26), new Vector2(3, 1), CommonStringKeys.RESET, delegate { ClearImagePlace(); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                    tb.ApplyTag("editor");
                }
            }
            else
            {
                tb = new TextButton(new Vector2(17, 26), new Vector2(3, 1), CommonStringKeys.SET, delegate { SetImagePlace(); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                tb.ApplyTag("editor");
            }
        }
    }

    public void SetBase()
    {
        List<EditorSelectionList.SelectionListEntry> baseMonster = new List<EditorSelectionList.SelectionListEntry>();

        Game game = Game.Get();
        baseMonster.Add(new EditorSelectionList.SelectionListEntry("{NONE}"));
        foreach (KeyValuePair<string, MonsterData> kv in game.cd.monsters)
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
            baseMonster.Add(new EditorSelectionList.SelectionListEntry(display, sets));
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
            if (monsterComponent.monsterName.Length == 0)
            {
                SetName();
            }
            if (monsterComponent.info.key.Length == 0)
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
        if (!nameDBE.uiInput.text.Equals(""))
        {
            monsterComponent.monsterName = nameDBE.uiInput.text;
        }
    }

    public void ClearName()
    {
        monsterComponent.monsterName = "";
        Update();
    }

    public void SetName()
    {
        monsterComponent.monsterName = "Monster Name";
        Update();
    }

    public void UpdateInfo()
    {
        if (!infoDBE.uiInput.text.Equals(""))
        {
            monsterComponent.info = new StringKey(infoDBE.uiInput.text);
        }
    }

    public void ClearInfo()
    {
        monsterComponent.info = StringKey.NULL;
        Update();
    }

    public void SetInfo()
    {
        monsterComponent.info = new StringKey("Monster Info");
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
            list.Add(new EditorSelectionList.SelectionListEntry(s));
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
        int.TryParse(healthDBE.uiInput.text, out monsterComponent.health);
    }

    public void ClearHealth()
    {
        monsterComponent.health = 0;
        monsterComponent.healthDefined = false;
        Update();
    }

    public void SetHealth()
    {
        monsterComponent.health = 0;
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
