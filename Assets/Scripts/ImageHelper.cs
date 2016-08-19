using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageHelper{
    private List<GUITextureState> drawList = new List<GUITextureState>();

    public static Texture2D getImage(string path)
    {
        string imagePath = @"file://" + Application.dataPath + "/../../" + path;

        WWW www = new WWW(imagePath);
        Texture2D newTex = new Texture2D(256, 256, TextureFormat.DXT5, false);
        www.LoadImageIntoTexture(newTex);
        Debug.Log("Read image from " + imagePath);

        return newTex;
    }

    //Screen.width

    class GUITextureState{
        public Texture2D tex;
        public int x;
        public int y;
        public int width;
        public int height;
        public bool visible;

        public Rect rect()
        {
            return new Rect(x, y, width, height);
        }
    }

    public void drawGUI()
    {
        //foreach(GUITextureState t in drawList)
          //  if(t.visible)
            //    GUI.DrawTexture(t.rect(), t.tex);
    }

    public void drawImage(string path, int x, int y)
    {
        Texture2D tex = getImage(path);
        GUITextureState tState = new GUITextureState();
        tState.tex = tex;
        tState.x = x;
        tState.y = y;
        tState.width = tex.width;
        tState.height = tex.height;
        tState.visible = true;

        drawList.Add(tState);
    }
}
