using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Class for creation of a dialog window with buttons and handling button press
public class DialogWindow {
    // The even that raises this dialog
    public QuestData.Event eventData;

    public DialogWindow(QuestData.Event e)
    {
        eventData = e;
        CreateWindow();
    }

    public void CreateWindow()
    {
        // holding object
        GameObject dialog = new GameObject("dialog");
        // All things are tagged dialog so we can clean up
        dialog.tag = "dialog";

        // Find the UI canvas
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

        // Position the dialog text
        RectTransform trans = dialog.AddComponent<RectTransform>();
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 30, 200);
        trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 400, 500);
        dialog.AddComponent<CanvasRenderer>();

        // Add the text to the component
        UnityEngine.UI.Text text = dialog.AddComponent<UnityEngine.UI.Text>();
        text.color = Color.white;
        text.text = eventData.text.Replace("\\n", "\n");
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Do we have a cancel button?
        if (eventData.cancelable)
        {
            new TextButton(new Vector2(400, 150), new Vector2(50, 20), "Cancel", delegate { onCancel(); });
        }
        // If there isn't a fail event we have a confirm button
        if(eventData.failEvent.Equals(""))
        {
            new TextButton(new Vector2(600, 150), new Vector2(50, 20), "Confirm", delegate { onConfirm(); });
        }
        // Otherwise we have pass and fail buttons
        else
        {
            new TextButton(new Vector2(500, 150), new Vector2(50, 20), "Fail", delegate { onFail(); }, Color.red);
            new TextButton(new Vector2(600, 150), new Vector2(50, 20), "Pass", delegate { onPass(); }, Color.green);
        }
    }

    // Pass and confirm are the same
    public void onPass()
    {
        onConfirm();
    }

    // Cancel cleans up
    public void onCancel()
    {
        destroy();
    }

    public void onFail()
    {
        // Destroy this dialog to close
        destroy();
        // Trigger failure event
        if (!eventData.failEvent.Equals(""))
        {
            Game game = GameObject.FindObjectOfType<Game>();
            game.triggerEvent(eventData.failEvent);
        }
    }

    public void onConfirm()
    {
        // Destroy this dialog to close
        destroy();
        // Trigger next event
        if (!eventData.nextEvent.Equals(""))
        {
            Game game = GameObject.FindObjectOfType<Game>();
            game.triggerEvent(eventData.nextEvent);
        }
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);
    }
}
