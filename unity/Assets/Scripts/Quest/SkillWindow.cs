using Assets.Scripts.Content;
using UnityEngine;
using System.Collections.Generic;

// Next stage button is used by MoM to move between investigators and monsters
public class SkillWindow
{
    public Dictionary<string, DialogBoxEditable> valueDBE;

    public bool developerToggle = false;

    // Construct and display
    public SkillWindow()
    {
        Update();
    }

    public void Update(int hero = 0;)
    {
        Destroyer.Dialog();
        Game game = Game.Get();

        // Add a title to the page
        DialogBox db = new DialogBox(
            new Vector2(8, 1),
            new Vector2(UIScaler.GetWidthUnits() - 16, 3),
            new StringKey("val", "SELECT_SKILLS")
            );
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(game.gameType.GetHeaderFont());

        // Get all heros
        int heroCount = 0;
        // Count number of selected heroes
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null) heroCount++;
        }

        float xOffset = UIScaler.GetHCenter(-18);
        if (heroCount < 4) xOffset += 4.5f;
        if (heroCount < 3) xOffset += 4.5f;

        TextButton tb = null;

        for (int i = 0; i < heroCount; i++)
        {
            int tmp = i;
            Texture2D heroTex = ContentData.FileToTexture(game.quest.heroes[i].heroData.image);
            Sprite heroSprite = Sprite.Create(heroTex, new Rect(0, 0, heroTex.width, heroTex.height), Vector2.zero, 1);
            tb = new TextButton(new Vector2(xOffset + 2.5f, 3.5f), new Vector2(4f, 4f), StringKey.NULL, delegate { Update(tmp); }, Color.clear, Color.white);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = heroSprite;
            tb.border.Destroy();

            xOffset += 9f;
        }

        float[] xOffsetArray = new float[4];
        xOffsetArray[1] = UIScaler.GetHCenter(-18);
        xOffsetArray[2] = UIScaler.GetHCenter(-18);
        xOffsetArray[3] = UIScaler.GetHCenter(-18);
        float yOffset = 10;

        foreach (SkillData s in game.cd.skills)
        {
            string skill = s.sectionName;
            if (s.sectionName.IndexOf("Skill" + game.quest.heroes[hero].class.Substring("Class".Length)) != 0) continue;
            tb = new TextButton(new Vector2(xOffset, yOffset + (s.xp * 5)), new Vector2(4f, 4f), s.name, delegate { SelectSkill(hero, skill); }, Color.green);
        }

        // Add a finished button to start the quest
        tb = new TextButton(
            new Vector2(UIScaler.GetRight(-8.5f),
            UIScaler.GetBottom(-2.5f)),
            new Vector2(8, 2),
            CommonStringKeys.CLOSE,
            delegate { Destroyer.Dialog(); });
    }

    public void SelectSkill(int hero, string skill)
    {

    }
}
