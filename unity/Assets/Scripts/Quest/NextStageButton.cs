using UnityEngine;
using System.Collections;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

// Next stage button is used by MoM to move between investigators and monsters
public class NextStageButton
{
    private readonly StringKey PHASE_INVESTIGATOR = new StringKey("val", "PHASE_INVESTIGATOR");
    private readonly StringKey PHASE_MYTHOS = new StringKey("val", "PHASE_MYTHOS");
    private readonly StringKey MONSTER_STEP = new StringKey("val", "MONSTER_STEP");
    private readonly StringKey HORROR_STEP = new StringKey("val", "HORROR_STEP");

    // Construct and display
    public NextStageButton()
    {
        if (Game.Get().gameType.DisplayHeroes()) return;

        Update();
    }

    public void Update()
    {
        // do not display the button bar when we reach the end of the game screen
        if (Game.Get().quest.questHasEnded)
            return;

        // First tile has not been displayed, button bar is not required yet
        if (!Game.Get().quest.firstTileDisplayed) 
            return;

        // Clean up everything marked as 'uiphase'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.UIPHASE))
            Object.Destroy(go);
                
        Color bgColor = new Color(0.05f, 0, 0, 0.9f);
        StringKey phase;
        if (Game.Get().quest.phase == Quest.MoMPhase.horror)
        {
            phase = HORROR_STEP;
        }
        else if (Game.Get().quest.phase == Quest.MoMPhase.mythos)
        {
            phase = PHASE_MYTHOS;
        }
        else if (Game.Get().quest.phase == Quest.MoMPhase.monsters)
        {
            phase = MONSTER_STEP;
        }
        else
        {
            phase = PHASE_INVESTIGATOR;
            bgColor = new Color(0, 0.05f, 0, 0.9f);
        }

        float string_width=0f, offset=0.5f;
        StringKey text;

        // Inventory button 
        UIElement ui = new UIElement(Game.UIPHASE);
        text = new StringKey("val", "ITEMS_SMALL");
        ui.SetText(text);
        string_width = ui.GetStringWidth(text, UIScaler.GetMediumFont(), Game.Get().gameType.GetHeaderFont()) + 0.5f;
        ui.SetLocation(offset, UIScaler.GetBottom(-2.5f), string_width, 2);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Items);
        ui.SetBGColor(bgColor);
        new UIElementBorder(ui);
        offset += string_width;

        // Action button
        ui = new UIElement(Game.UIPHASE);
        text = CommonStringKeys.SET;
        ui.SetText(text);
        string_width = ui.GetStringWidth(text, UIScaler.GetMediumFont(), Game.Get().gameType.GetHeaderFont()) + 0.5f;
        ui.SetLocation(offset, UIScaler.GetBottom(-2.5f), string_width, 2);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Set);
        ui.SetBGColor(bgColor);
        new UIElementBorder(ui);
        offset += string_width;

        // Log button (text from previous event)
        ui = new UIElement(Game.UIPHASE);
        text = CommonStringKeys.LOG;
        ui.SetText(text);
        string_width = ui.GetStringWidth(text, UIScaler.GetMediumFont(), Game.Get().gameType.GetHeaderFont()) + 0.5f;
        ui.SetLocation(offset, UIScaler.GetBottom(-2.5f), string_width, 2);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Log);
        ui.SetBGColor(bgColor);
        new UIElementBorder(ui);
        offset += string_width;

        // Text description for current phase
        ui = new UIElement(Game.UIPHASE);
        Color color;
        if (phase == PHASE_INVESTIGATOR)
            color = Color.white;
        else
            color = Color.red;
        ui.SetText(phase, color);
        string_width = ui.GetStringWidth(phase, UIScaler.GetMediumFont(), Game.Get().gameType.GetHeaderFont()) + 0.5f;
        ui.SetLocation(UIScaler.GetRight(-4.5f - string_width), UIScaler.GetBottom(-2.5f), string_width, 2);
        ui.SetBGColor(bgColor);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetFontStyle(FontStyle.Italic);

        // Next phase button
        // Tweak border size so that arrow is centered, should be changed if using another character or picture
        ui = new UIElement(Game.UIPHASE);
        ui.SetLocation(UIScaler.GetRight(-4f), UIScaler.GetBottom(-2.5f), 3f, 2.25f);
        new UIElementBorder(ui);
        // make button slightly smaller so it does not overlap border
        ui = new UIElement(Game.UIPHASE);
        ui.SetLocation(UIScaler.GetRight(-3.95f), UIScaler.GetBottom(-2.45f), 2.9f, 1.9f);
        ui.SetText(CommonStringKeys.TAB);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());
        ui.SetButton(Next);
        ui.SetBGColor(bgColor);
    }

    // Button pressed
    public void Next()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }

        Game game = Game.Get();

        if (game.quest.UIItemsPresent()) return;

        // Add to undo stack
        game.quest.Save();

        if (game.quest.phase == Quest.MoMPhase.monsters)
        {
            game.audioControl.PlayTrait("horror");
        }

        if (game.quest.phase == Quest.MoMPhase.horror)
        {
            game.roundControl.EndRound();
        }
        else
        {
            game.quest.log.Add(new Quest.LogEntry(new StringKey("val", "PHASE_MYTHOS").Translate()));
            game.roundControl.HeroActivated();
        }
    }

    public void Items()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        new InventoryWindowMoM();
    }

    public void Log()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        new LogWindow();
    }

    public void Set()
    {
        if (GameObject.FindGameObjectWithTag(Game.DIALOG) != null)
        {
            return;
        }
        new SetWindow();
    }
}
