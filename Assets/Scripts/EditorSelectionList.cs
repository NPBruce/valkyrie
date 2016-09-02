using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorSelectionList
{

    public string selection = "";
    public string[] items;
    public string title;
    public UnityEngine.Events.UnityAction returnCall;
    public UnityEngine.Events.UnityAction cancelCall;
    public int indexOffset = 0;

    public EditorSelectionList(string t, List<string> list, UnityEngine.Events.UnityAction call)
    {
        items = list.ToArray();
        title = t;
        returnCall = call;
    }

    public void SelectItem()
    {
        SelectItem(delegate { Destroyer.Dialog(); });
    }


    public void SelectItem(UnityEngine.Events.UnityAction call)
    {
        Destroyer.Dialog();
        cancelCall = call;

        DialogBox db = new DialogBox(new Vector2(21, 0), new Vector2(20, 26), "");
        db.AddBorder();

        db = new DialogBox(new Vector2(21, 0), new Vector2(20, 1), title);

        float offset = 2;
        TextButton tb = null;
        if (items.Length <= 20)
        {
            foreach (string s in items)
            {
                string key = s;
                tb = new TextButton(new Vector2(21, offset), new Vector2(20, 1), key, delegate { SelectComponent(key); });
                tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                offset += 1;
            }
            offset += 1;
            tb = new TextButton(new Vector2(26.5f, offset), new Vector2(9, 1), "Cancel", call);
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        }
        else
        {
            for (int i = indexOffset; i < (20 + indexOffset); i++)
            {
                if (items.Length > i)
                {
                    string key = items[i];
                    tb = new TextButton(new Vector2(21, offset), new Vector2(20, 1), key, delegate { SelectComponent(key); });
                    tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
                }
                offset += 1;
            }
            offset += 1;
            tb = new TextButton(new Vector2(22f, offset), new Vector2(1, 1), "<", delegate { PreviousPage(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb = new TextButton(new Vector2(39f, offset), new Vector2(1, 1), ">", delegate { NextPage(); });
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            offset += 1;
            tb = new TextButton(new Vector2(26.5f, offset), new Vector2(9, 1), "Cancel", cancelCall);
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.0f, 0f);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
        }
    }
    
    public void NextPage()
    {
        indexOffset += 20;
        if (indexOffset > items.Length)
        {
            indexOffset -= 20;
        }
        SelectItem(cancelCall);
    }

    public void PreviousPage()
    {
        indexOffset -= 20;
        if (indexOffset < 0)
        {
            indexOffset = 0;
        }
        SelectItem(cancelCall);
    }

    public void SelectComponent(string s)
    {
        selection = s;
        returnCall();
    }
}