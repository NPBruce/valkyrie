using UnityEngine;

public class LoadingScreen
{
    public bool finished = false;
    bool closed = false;
    UnityEngine.Events.UnityAction function;

    public void Start(UnityEngine.Events.UnityAction call)
    {
        function = call;
        finished = false;
        closed = false;
        Destroyer.Dialog();
        // Display message
        DialogBox db = new DialogBox(new Vector2(2, 10), new Vector2(UIScaler.GetWidthUnits() - 4, 2), "Loading...");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetMediumFont();
    }

    // This is called by the non unity thread
    // Must not have any unity things
    public void End()
    {
        finished = true;
    }

    public void Update()
    {
        if (finished && !closed)
        {
            Destroyer.Dialog();
            closed = true;
            if (function != null)
            {
                function();
            }
        }
    }
}

