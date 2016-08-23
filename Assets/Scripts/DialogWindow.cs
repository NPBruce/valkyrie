using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DialogWindow {

    public bool cancelable = false;
    public QuestData.Event eventData;
    public bool active = true;

    public DialogWindow(QuestData.Event e)
    {
        eventData = e;
        CreateWindow();
    }

    public void CreateWindow()
    {
        Game game = GameObject.FindObjectOfType<Game>();

        GameObject dialog = new GameObject("dialog");
        dialog.tag = "dialog";

        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }


        dialog.transform.parent = canvas.transform;

        RectTransform trans = dialog.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 30, 50);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 400, 500);

        CanvasRenderer cr = dialog.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Text text = dialog.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.white;
        text.text = eventData.text.Replace("\\n", "\n");
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        /*GameObject dialogEventHolder = new GameObject("dialogEventHolder");
        dialogEventHolder.tag = "dialog";
        dialogEventHolder.transform.SetParent(canvas.transform);
        Game game = GameObject.FindObjectOfType<Game>();
        EventSystem events = dialogEventHolder.AddComponent<EventSystem>();
        dialogEventHolder.AddComponent<StandaloneInputModule>();*/

        if (cancelable)
        {
            createCancel();
        }
        if(eventData.failEvent.Equals(""))
        {
            createConfirm();
        }
        else
        {
            createPass();
            createFail();
        }
    }

    public void createCancel()
    {

    }

    public void createConfirm()
    {
        GameObject confirm = new GameObject("confirm");
        confirm.tag = "dialog";

        
        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }


        confirm.transform.parent = canvas.transform;

        RectTransform trans = confirm.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 90, 20);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 500, 50);

        CanvasRenderer cr = confirm.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Button button = confirm.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { onConfirm(); });

        UnityEngine.UI.Text text = confirm.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.white;
        text.text = "Confirm";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

    }

    public void createPass()
    {

    }

    public void createFail()
    {

    }

    public void onPass()
    {
        onConfirm();
    }

    public void onCancel()
    {
        destroy();
    }

    public void onFail()
    {
        destroy();
    }

    public void onConfirm()
    {
        if(!eventData.nextEvent.Equals(""))
        {
            Game game = GameObject.FindObjectOfType<Game>();
            game.triggerEvent(eventData.nextEvent);
        }
        destroy();
    }

    public void destroy()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
        active = false;
    }
}
