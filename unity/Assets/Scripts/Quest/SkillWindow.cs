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

    public void Update(int hero = 0)
    {
        Destroyer.Dialog();
        Game game = Game.Get();

        DialogBox db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-17), 1),
            new Vector2(34, 6),
            StringKey.NULL);
        db.AddBorder();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-17), 7),
            new Vector2(34, 17),
            StringKey.NULL);
        db.AddBorder();

        // Add a title to the page
        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-6), 1),
            new Vector2(12, 3),
            new StringKey("val", "SELECT_SKILLS"));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(game.gameType.GetHeaderFont());

        // Get all heros
        int heroCount = 0;
        // Count number of selected heroes
        foreach (Quest.Hero h in game.quest.heroes)
        {
            if (h.heroData != null) heroCount++;
        }

        float xOffset = UIScaler.GetHCenter(-11);
        if (heroCount < 4) xOffset += 3f;
        if (heroCount < 3) xOffset += 3f;

        TextButton tb = null;

        int availableXP = 0;
        for (int i = 0; i < heroCount; i++)
        {
            int tmp = i;
            Texture2D heroTex = ContentData.FileToTexture(game.quest.heroes[i].heroData.image);
            Sprite heroSprite = Sprite.Create(heroTex, new Rect(0, 0, heroTex.width, heroTex.height), Vector2.zero, 1);
            tb = new TextButton(new Vector2(xOffset, 3.5f), new Vector2(4f, 4f), StringKey.NULL, delegate { Update(tmp); }, Color.clear);
            tb.background.GetComponent<UnityEngine.UI.Image>().sprite = heroSprite;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.3f, 0.3f);
            if (i == hero)
            {
                tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            }
            tb.border.Destroy();

            availableXP = game.quest.heroes[i].AvailableXP();
            if (availableXP != 0)
            {
                tb = new TextButton(new Vector2(xOffset + 2, 5.5f), new Vector2(2f, 2f), availableXP, delegate { Update(tmp); }, Color.blue);
            }

            xOffset += 6f;
        }

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-16), 8.5f),
            new Vector2(32, 5),
            StringKey.NULL);
        db.AddBorder();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-16), 8.5f),
            new Vector2(2, 5),
            1);
        db.AddBorder();
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-16), 13.5f),
            new Vector2(32, 5),
            StringKey.NULL);
        db.AddBorder();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-16), 13.5f),
            new Vector2(2, 5),
            2);
        db.AddBorder();
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-16), 18.5f),
            new Vector2(32, 5),
            StringKey.NULL);
        db.AddBorder();

        db = new DialogBox(
            new Vector2(UIScaler.GetHCenter(-16), 18.5f),
            new Vector2(2, 5),
            3);
        db.AddBorder();
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        float[] xOffsetArray = new float[4];
        xOffsetArray[1] = UIScaler.GetHCenter(-13);
        xOffsetArray[2] = UIScaler.GetHCenter(-13);
        xOffsetArray[3] = UIScaler.GetHCenter(-8);
        float yOffset = 4;

        availableXP = game.quest.heroes[hero].AvailableXP();
        foreach (SkillData s in game.cd.skills.Values)
        {
            if (s.xp == 0) continue;
            if (game.quest.heroes[hero].className.Length == 0) continue;
            string skill = s.sectionName;
            if (s.sectionName.IndexOf("Skill" + game.quest.heroes[hero].className.Substring("Class".Length)) != 0) continue;


            Color buttonColor = new Color(0.4f, 0.4f, 0.4f);
            if (game.quest.heroes[hero].skills.Contains(s.sectionName))
            {
                buttonColor = Color.green;
            }
            else if (s.xp <= availableXP)
            {
                buttonColor = Color.white;
            }

            tb = new TextButton(new Vector2(xOffsetArray[s.xp], yOffset + (s.xp * 5)), new Vector2(8f, 4f), s.name, delegate { SelectSkill(hero, skill); }, buttonColor);
            xOffsetArray[s.xp] += 10;
        }

        // Add a finished button to start the quest
        tb = new TextButton(
            new Vector2(UIScaler.GetHCenter(-4f), 24.5f),
            new Vector2(8, 2),
            CommonStringKeys.CLOSE,
            delegate { Destroyer.Dialog(); });
    }

    public void SelectSkill(int hero, string skill)
    {
        Game game = Game.Get();
        List<string> skills = game.quest.heroes[hero].skills;

        if (skills.Contains(skill))
        {
            skills.Remove(skill);
        }
        else
        {
            if (game.quest.heroes[hero].AvailableXP() < game.cd.skills[skill].xp) return;
            skills.Add(skill);
        }
        Update(hero);
    }
}
