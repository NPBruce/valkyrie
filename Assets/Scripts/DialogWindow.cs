using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DialogWindow {

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

        if (eventData.cancelable)
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
        GameObject cancel = new GameObject("cancel");
        cancel.tag = "dialog";


        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }


        cancel.transform.parent = canvas.transform;

        RectTransform trans = cancel.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 90, 20);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 400, 50);

        CanvasRenderer cr = cancel.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Button button = cancel.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { onCancel(); });

        UnityEngine.UI.Text text = cancel.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.white;
        text.text = "Cancel";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

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
        GameObject pass = new GameObject("pass");
        pass.tag = "dialog";


        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }


        pass.transform.parent = canvas.transform;

        RectTransform trans = pass.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 90, 20);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 600, 50);

        CanvasRenderer cr = pass.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Button button = pass.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { onPass(); });

        UnityEngine.UI.Text text = pass.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.green;
        text.text = "Pass";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

    }

    public void createFail()
    {
        GameObject fail = new GameObject("fail");
        fail.tag = "dialog";


        Canvas[] canvii = GameObject.FindObjectsOfType<Canvas>();
        Canvas canvas = canvii[0];
        foreach (Canvas c in canvii)
        {
            if (c.name.Equals("UICanvas"))
            {
                canvas = c;
            }
        }


        fail.transform.parent = canvas.transform;

        RectTransform trans = fail.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 90, 20);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 500, 50);

        CanvasRenderer cr = fail.AddComponent<CanvasRenderer>();

        UnityEngine.UI.Button button = fail.AddComponent<UnityEngine.UI.Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { onFail(); });

        UnityEngine.UI.Text text = fail.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.red;
        text.text = "Fail";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

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
        if (!eventData.failEvent.Equals(""))
        {
            Game game = GameObject.FindObjectOfType<Game>();
            game.triggerEvent(eventData.failEvent);
        }
    }

    public void onConfirm()
    {
        destroy();
        if (!eventData.nextEvent.Equals(""))
        {
            Game game = GameObject.FindObjectOfType<Game>();
            game.triggerEvent(eventData.nextEvent);
        }
    }

    public void destroy()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
        active = false;
    }
}
