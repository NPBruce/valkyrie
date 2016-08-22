using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
    // Use this for initialization
    void Start () {

        Game game = FindObjectOfType<Game>();

        QuestData qd = new QuestData(Application.dataPath + "/../../valkyrie-quests/roag-intro/quest.ini", game);

        foreach(QuestData.Tile t in qd.tiles)
        {
            string imagePath = @"file://" +  t.tileType.image;

            UnityEngine.UI.Image image;
            Sprite testSprite;

            WWW www = new WWW(imagePath);
            Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
            www.LoadImageIntoTexture(newTex);

            GameObject tile = new GameObject(t.name);

            Canvas canvas = FindObjectOfType<Canvas>();
            tile.transform.parent = canvas.transform;

            image = tile.AddComponent<UnityEngine.UI.Image>();
            testSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            image.sprite = testSprite;
            /*image.rectTransform.sizeDelta = new Vector2(newTex.width / 105, newTex.height / 105);
            tile.transform.Translate(Vector3.right * ((newTex.width / 2) - t.tileType.left)/105);
            tile.transform.Translate(Vector3.down * ((newTex.height/ 2) - t.tileType.top) / 105);
            tile.transform.Translate(new Vector3(t.x - (float)0.5, t.y - (float)0.5, 0));*/
            image.rectTransform.sizeDelta = new Vector2(newTex.width, newTex.height);
            tile.transform.Translate(Vector3.right * ((newTex.width / 2) - t.tileType.left));
            tile.transform.Translate(Vector3.down * ((newTex.height / 2) - t.tileType.top));
            tile.transform.Translate(new Vector3(t.x - (float)0.5, t.y - (float)0.5, 0) * 105);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}

