using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using Assets.Scripts.UI;

public class ChangePhaseWindow
{
    private static readonly string IMG_BG_INVESTIGATORS_PHASE = "ImageGreenBG";
    private static readonly string IMG_BG_MYTHOS_PHASE = "ImageMythosBackground";

    private static readonly StringKey PHASE_INVESTIGATOR = new StringKey("val", "PHASE_INVESTIGATOR");
    private static readonly StringKey PHASE_MYTHOS = new StringKey("val", "PHASE_MYTHOS");

    private static GameObject gameobject_timer = null;
    private static SimpleTimer timer=null;

    // Object default configuration
    public static float heroSize = 5f;
    public static float offset_size = 6f;
    public static float transition_duration = 3;

    public static void DisplayTransitionWindow(Quest.MoMPhase phase)
    {
        // do NOT delete dialog, they are hidden behing the mythos phase
        //Destroyer.Dialog();

        Game game = Game.Get();
        UIElement text;

        // dot NOT display transition screen while we are in Editor mode
        if (game.testMode)
            return;

        // Background picture in full screen
        UIElement bg = new UIElement(Game.TRANSITION);
        Texture2D bgTex;
        if (phase == Quest.MoMPhase.investigator)
            bgTex = ContentData.FileToTexture(game.cd.images[IMG_BG_INVESTIGATORS_PHASE].image);
        else
            bgTex = ContentData.FileToTexture(game.cd.images[IMG_BG_MYTHOS_PHASE].image);
        bg.SetImage(bgTex);
        bg.SetLocation(0, 0, UIScaler.GetWidthUnits(), UIScaler.GetHeightUnits());

        // Text of phase
        text = new UIElement(Game.TRANSITION, bg.GetTransform());
        if (phase == Quest.MoMPhase.investigator)
        {
            // Silver 	#C0C0C0 	(192,192,192)
            text.SetText(PHASE_INVESTIGATOR, new Color32(192, 192, 192, 255));
            text.SetLocation(5, 3, 30, 3);
        }
        else
        {
            // Dark red #8B0000 (139,0,0)
            text.SetText(PHASE_MYTHOS, new Color32(139, 0, 0, 255));
            text.SetLocation(7, 3, 30, 3);
        }
        text.SetFont(game.gameType.GetHeaderFont());
        text.SetFontSize(UIScaler.GetLargeFont());
        text.SetFontStyle(FontStyle.Italic);
        text.SetTextAlignment(TextAnchor.MiddleLeft);
        text.SetBGColor(Color.clear);


        if (phase == Quest.MoMPhase.investigator)
        {
            // Draw pictures of investigators for investigator phase
            float offset = (game.quest.GetHeroCount() - 1) * (-1 * offset_size / 2) - (offset_size / 2);
            UIElement ui = null;
            foreach (Quest.Hero h in game.quest.heroes)
            {
                if (h.heroData != null)
                {
                    // Draw pictures
                    Texture2D newTex = ContentData.FileToTexture(h.heroData.image);

                    ui = new UIElement(Game.TRANSITION, bg.GetTransform());
                    ui.SetLocation(UIScaler.GetHCenter(offset), UIScaler.GetVCenter(), heroSize, heroSize);
                    ui.SetImage(newTex);

                    offset += offset_size;
                }
            }
        }
        else
        {
            // Don't draw anything for Mythos phase
        }

        // Launch timer to remove this window in 'transition_duration' seconds
        if (gameobject_timer==null)
        {
            gameobject_timer = new GameObject("TIMER");
            timer = gameobject_timer.AddComponent<SimpleTimer>();
        }

        timer.Init(transition_duration, Close);
    }

    public static void Close()
    {
        Destroyer.Transition();
        UnityEngine.Object.Destroy(gameobject_timer);
    }

}
