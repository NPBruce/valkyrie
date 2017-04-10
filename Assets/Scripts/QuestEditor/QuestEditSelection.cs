using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Content;

public class QuestEditSelection
{
    public Dictionary<string, QuestData.Quest> questList;

    // Create a pack with list of quests to edit
    public QuestEditSelection()
    {
        Game game = Game.Get();
        // Get list of unpacked quest in user location (editable)
        // TODO: open/save in packages
        questList = QuestLoader.GetUserUnpackedQuests();

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Heading
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3),
            new StringKey("val","SELECT",game.gameType.QuestName()));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(Game.Get().gameType.GetHeaderFont());

        db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 21 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        // List of quests
        int offset = 5;
        TextButton tb;
        foreach (KeyValuePair<string, QuestData.Quest> q in questList)
        {
            string key = q.Key;
            tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f),
                new StringKey("val","INDENT",new StringKey(q.Value.name,false)), delegate { Selection(key); }, Color.black, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;
            offset += 2;
        }
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 5) * UIScaler.GetPixelsPerUnit());

        // Main menu
        tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { Cancel(); }, Color.red);
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
        // Delete a user quest
        tb = new TextButton(new Vector2((UIScaler.GetRight() * 3 / 8) - 4, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.DELETE, delegate { Delete(); }, Color.red);
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
        // Copy a quest
        tb = new TextButton(new Vector2((UIScaler.GetRight() * 5 / 8) - 4, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.COPY, delegate { Copy(); });
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
        // Create a new quest
        tb = new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.NEW, delegate { NewQuest(); });
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
    }

    public void Cancel()
    {
        Destroyer.MainMenu();
    }

    // Change the dialog to a delete dialog
    public void Delete()
    {
        questList = QuestLoader.GetUserUnpackedQuests();
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Header
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3),
            new StringKey("val","SELECT_TO_DELETE", game.gameType.QuestName()));
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(Game.Get().gameType.GetHeaderFont());

        db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 21 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        // List of quests
        int offset = 5;
        TextButton tb;
        foreach (KeyValuePair<string, QuestData.Quest> q in questList)
        {
            string key = q.Key;
            tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f),
                new StringKey("val","INDENT",new StringKey(q.Value.name,false)), delegate { Delete(key); }, Color.black, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 0f, 0f);
            tb.background.transform.parent = scrollArea.transform;
            offset += 2;
        }
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 5) * UIScaler.GetPixelsPerUnit());

        // Back to edit list
        tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), CommonStringKeys.BACK, delegate { CancelDelete(); }, Color.red);
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
    }

    // Delete quest
    public void Delete(string key)
    {
        try
        {
            Directory.Delete(key, true);
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Failed to delete quest: " + key);
        }
        new QuestEditSelection();
    }

    public void CancelCopy()
    {
        new QuestEditSelection();
    }

    public void CancelDelete()
    {
        new QuestEditSelection();
    }

    // List of quests to copy
    public void Copy()
    {
        // Can copy all quests, not just user
        questList = QuestLoader.GetQuests();
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // Header
        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), 
            new StringKey("val","SELECT_TO_COPY", game.gameType.QuestName())
            );
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();
        db.SetFont(Game.Get().gameType.GetHeaderFont());

        db = new DialogBox(new Vector2(1, 5f), new Vector2(UIScaler.GetWidthUnits()-2f, 21f), StringKey.NULL);
        db.AddBorder();
        db.background.AddComponent<UnityEngine.UI.Mask>();
        UnityEngine.UI.ScrollRect scrollRect = db.background.AddComponent<UnityEngine.UI.ScrollRect>();

        GameObject scrollArea = new GameObject("scroll");
        RectTransform scrollInnerRect = scrollArea.AddComponent<RectTransform>();
        scrollArea.transform.parent = db.background.transform;
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, (UIScaler.GetWidthUnits()-3f) * UIScaler.GetPixelsPerUnit());
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 1);

        GameObject scrollBarObj = new GameObject("scrollbar");
        scrollBarObj.transform.parent = db.background.transform;
        RectTransform scrollBarRect = scrollBarObj.AddComponent<RectTransform>();
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 21 * UIScaler.GetPixelsPerUnit());
        scrollBarRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (UIScaler.GetWidthUnits() - 3f) * UIScaler.GetPixelsPerUnit(), 1 * UIScaler.GetPixelsPerUnit());
        UnityEngine.UI.Scrollbar scrollBar = scrollBarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollBar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollBar;

        GameObject scrollBarHandle = new GameObject("scrollbarhandle");
        scrollBarHandle.transform.parent = scrollBarObj.transform;
        //RectTransform scrollBarHandleRect = scrollBarHandle.AddComponent<RectTransform>();
        scrollBarHandle.AddComponent<UnityEngine.UI.Image>();
        scrollBarHandle.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.7f, 0.7f);
        scrollBar.handleRect = scrollBarHandle.GetComponent<RectTransform>();
        scrollBar.handleRect.offsetMin = Vector2.zero;
        scrollBar.handleRect.offsetMax = Vector2.zero;

        scrollRect.content = scrollInnerRect;
        scrollRect.horizontal = false;

        // List of quests
        int offset = 5;
        TextButton tb;
        foreach (KeyValuePair<string, QuestData.Quest> q in questList)
        {
            string key = q.Key;
            tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 5, 1.2f),
                new StringKey("val","INDENT",new StringKey(q.Value.name,false)), delegate { Copy(key); }, Color.black, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().material = (Material)Resources.Load("Fonts/FontMaterial");
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            tb.background.transform.parent = scrollArea.transform;
            offset += 2;
        }
        scrollInnerRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, (offset - 5) * UIScaler.GetPixelsPerUnit());

        // Back to edit selection
        tb = new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2),CommonStringKeys.BACK, delegate { CancelCopy(); }, Color.red);
        tb.SetFont(Game.Get().gameType.GetHeaderFont());
    }

    // Copy a quest
    public void Copy(string key)
    {
        Game game = Game.Get();
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";

        // Find a new unique directory name
        int i = 1;
        while (Directory.Exists(dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i))
        {
            i++;
        }
        string targetLocation = dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i;

        // Copy files
        try
        {
            DirectoryCopy(key, targetLocation, true);
            // read new quest file
            string[] questData = File.ReadAllLines(targetLocation + "/quest.ini");

            // Search for quest section
            bool questFound = false;
            for (i = 0; i < questData.Length; i++)
            {
                if (questData[i].Equals("[Quest]"))
                {
                    // Inside quest section
                    questFound = true;
                }
                if (questFound && questData[i].IndexOf("name=") == 0)
                {
                    // Add copy to name
                    questFound = false;
                    questData[i] = questData[i] + " (Copy)";
                }
            }
            // Write back to ini file
            File.WriteAllLines(targetLocation + "/quest.ini", questData);
        }
        catch (System.Exception)
        {
            ValkyrieDebug.Log("Error: Failed to copy quest.");
            Application.Quit();
        }
        // Back to selection
        new QuestEditSelection();
    }

    // Copy a directory
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    // Create a new quest
    public void NewQuest()
    {
        Game game = Game.Get();
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";

        // Find an available unique directory name
        int i = 1;
        while (Directory.Exists(dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i))
        {
            i++;
        }
        string targetLocation = dataLocation + "/Editor" + game.gameType.QuestName().Translate() + i;

        try
        {
            Directory.CreateDirectory(targetLocation);

            string[] questData = new string[3];

            // Create basic quest info
            questData[0] = "[Quest]";
            questData[1] = "name=Editor " + game.gameType.QuestName().Translate() + " " + i;
            questData[2] = "type=" + game.gameType.TypeName();

            // Write quest file
            File.WriteAllLines(targetLocation + "/quest.ini", questData);
        }
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Error: Failed to create new quest: " + e.Message);
            Application.Quit();
        }
        // Back to edit selection
        new QuestEditSelection();
    }

    // Select a quest for editing
    public void Selection(string key)
    {
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        game.audioControl.Music(new List<string>());

        // Fetch all of the quest data
        ValkyrieDebug.Log("Selecting Quest: " + key + System.Environment.NewLine);
        game.quest = new Quest(questList[key]);
        ValkyrieDebug.Log("Starting Editor" + System.Environment.NewLine);
        QuestEditor.Begin();
    }
}
