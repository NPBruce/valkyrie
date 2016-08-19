using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {


    // Use this for initialization
    void Start () {
        string imagePath = @"file://" + Application.dataPath + "/../../" + "GamePacks/D2E/img/tiles/001A.png";

        UnityEngine.UI.Image image;
        Sprite testSprite;

        WWW www = new WWW(imagePath);
        Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
        www.LoadImageIntoTexture(newTex);

        GameObject tile = new GameObject("Package");

        Canvas canvas = FindObjectOfType<Canvas>();
        tile.transform.parent = canvas.transform;

        image = tile.AddComponent<UnityEngine.UI.Image>();
        testSprite = Sprite.Create(newTex, new Rect(0, 0, 1024, 1024), Vector2.zero, 105);
        image.sprite = testSprite;
    }

    // Update is called once per frame
    void Update () {
    }
}
