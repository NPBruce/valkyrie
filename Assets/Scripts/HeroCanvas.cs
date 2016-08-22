using UnityEngine;
using System.Collections;

public class HeroCanvas : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Game game = FindObjectOfType<Game>();
        string imagePath = @"file://" + game.cd.heros["HeroSyndrael"].image;

        Sprite heroSprite;

        WWW www = new WWW(imagePath);
        Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
        www.LoadImageIntoTexture(newTex);

        GameObject heroImg = new GameObject("heroImg");

        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("HeroCanvas"))
            {
                canvas = c;
            }
        }


        heroImg.transform.parent = canvas.transform;

        UnityEngine.UI.Image image = heroImg.AddComponent<UnityEngine.UI.Image>();
        heroSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
        image.sprite = heroSprite;
        image.rectTransform.sizeDelta = new Vector2(100000, 100000);

    }

    // Update is called once per frame
    void Update () {
	
	}
}
