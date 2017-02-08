using UnityEngine;
using System.Collections;

public class GameSelection
{
    FetchContent fcD2E;
    FetchContent fcMoM;

    // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
    public GameSelection()
    {
        // This will destroy all
        Destroyer.Destroy();

        Game game = Game.Get();

        game.gameType = new NoGameType();

        fcD2E = new FetchContent("D2E");
        fcMoM = new FetchContent("MoM");

        // Banner Image
        Sprite bannerSprite;
        Texture2D newTex = Resources.Load("sprites/valkyrie-white") as Texture2D;

        GameObject banner = new GameObject("banner");
        banner.tag = "dialog";

        banner.transform.parent = game.uICanvas.transform;

        RectTransform trans = banner.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 1 * UIScaler.GetPixelsPerUnit(), 4.5f * UIScaler.GetPixelsPerUnit());
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 18.75f) * UIScaler.GetPixelsPerUnit() / 2f, 18.75f * UIScaler.GetPixelsPerUnit());
        banner.AddComponent<CanvasRenderer>();


        UnityEngine.UI.Image image = banner.AddComponent<UnityEngine.UI.Image>();
        bannerSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = bannerSprite;
        image.rectTransform.sizeDelta = new Vector2(18.75f * UIScaler.GetPixelsPerUnit(), 4.5f * UIScaler.GetPixelsPerUnit());

        DialogBox db;

        Color startColor = Color.white;
        if (fcD2E.NeedImport())
        {
            startColor = Color.gray;
        }
        TextButton tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 30) / 2, 10), new Vector2(30, 4f), "Descent: Journeys in the Dark Second Edition", delegate { D2E(); }, startColor);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        if (fcD2E.importAvailable)
        {
            if (fcD2E.NeedImport())
            {
                tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 14.2f), new Vector2(10, 2f), "Import Content", delegate { Import("D2E"); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
            }
            else
            {
                tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 14.2f), new Vector2(10, 2f), "Reimport Content", delegate { Import("D2E"); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
            }
        }
        else
        {
            db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 14.2f), new Vector2(10, 2f), "Import Unavailable", Color.red);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
        }

        startColor = Color.white;
        if (fcMoM.NeedImport())
        {
            startColor = Color.gray;
        }
        tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 30) / 2, 19), new Vector2(30, 4f), "Mansions of Madness Second Edition", delegate { MoM(); }, startColor);
        tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);

        if (fcMoM.importAvailable)
        {
            if (fcMoM.NeedImport())
            {
                tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 23.2f), new Vector2(10, 2f), "Import Content", delegate { Import("MoM"); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
            }
            else
            {
                tb = new TextButton(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 23.2f), new Vector2(10, 2f), "Reimport Content", delegate { Import("MoM"); });
                tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0.03f, 0f);
            }
        }
        else
        {
            db = new DialogBox(new Vector2((UIScaler.GetWidthUnits() - 10) / 2, 23.2f), new Vector2(10, 2f), "Import Unavailable", Color.red);
            db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
            db.AddBorder();
        }
        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Exit", delegate { Exit(); }, Color.red);
    }

    // Start quest
    public void D2E()
    {
        if (!fcD2E.NeedImport())
        {
            Game.Get().gameType = new D2EGameType();
            Destroyer.MainMenu();
        }
    }

    public void Import(string type)
    {
        Destroyer.Destroy();
        DialogBox db = new DialogBox(new Vector2(2, 10), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Importing...");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
        Game.Get().CallAfterFrame(delegate { PerformImport(type); });
    }

    public void MoM()
    {
        if (!fcMoM.NeedImport())
        {
            Game.Get().gameType = new MoMGameType();
            Game.Get().roundControl = new RoundControllerMoM();
            Destroyer.MainMenu();
        }
    }

    private void PerformImport(string type)
    {
        if (type.Equals("D2E"))
        {
            fcD2E.Import();
        }
        if (type.Equals("MoM"))
        {
            fcMoM.Import();
        }
        Destroyer.Dialog();
        new GameSelection();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
