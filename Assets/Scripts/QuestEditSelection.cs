using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class QuestEditSelection
{
    public Dictionary<string, QuestLoader.Quest> questList;

    public QuestEditSelection()
    {
        // For now only edit unpacked quests
        questList = QuestLoader.GetUserUnpackedQuests();
        //questList = QuestLoader.GetUserQuests();

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Select Quest");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        int offset = 5;
        foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
        {
            string key = q.Key;
            TextButton tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + q.Value.name, delegate { Selection(key); }, Color.white, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
            offset += 2;
        }

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { Cancel(); }, Color.red);
        new TextButton(new Vector2((UIScaler.GetRight() * 3 / 8) - 4, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Delete", delegate { Delete(); }, Color.red);
        new TextButton(new Vector2((UIScaler.GetRight() * 5 / 8) - 4, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Copy", delegate { Copy(); });
        new TextButton(new Vector2(UIScaler.GetRight(-9), UIScaler.GetBottom(-3)), new Vector2(8, 2), "New", delegate { NewQuest(); });
    }

    public void Cancel()
    {
        Destroyer.MainMenu();
    }

    public void Delete()
    {
        questList = QuestLoader.GetUserUnpackedQuests();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Select Quest To Delete");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        int offset = 5;
        foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
        {
            string key = q.Key;
            TextButton tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + q.Value.name, delegate { Delete(key); }, Color.red, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
            offset += 2;
        }

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { CancelDelete(); }, Color.red);

    }

    public void Delete(string key)
    {
        //Fixme
    }

    public void CancelCopy()
    {
        new QuestEditSelection();
    }

    public void CancelDelete()
    {
        new QuestEditSelection();
    }

    public void Copy()
    {
        questList = QuestLoader.GetQuests();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        DialogBox db = new DialogBox(new Vector2(2, 1), new Vector2(UIScaler.GetWidthUnits() - 4, 3), "Select Quest To Copy");
        db.textObj.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetLargeFont();

        int offset = 5;
        foreach (KeyValuePair<string, QuestLoader.Quest> q in questList)
        {
            string key = q.Key;
            TextButton tb = new TextButton(new Vector2(2, offset), new Vector2(UIScaler.GetWidthUnits() - 4, 1.2f), "  " + q.Value.name, delegate { Copy(key); }, Color.white, offset);
            tb.button.GetComponent<UnityEngine.UI.Text>().fontSize = UIScaler.GetSmallFont();
            tb.button.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleLeft;
            tb.background.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.1f);
            offset += 2;
        }

        new TextButton(new Vector2(1, UIScaler.GetBottom(-3)), new Vector2(8, 2), "Back", delegate { CancelCopy(); }, Color.red);
    }

    public void Copy(string key)
    {
        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";

        int i = 1;
        while (Directory.Exists(dataLocation + "/EditorQuest" + i))
        {
            i++;
        }
        string targetLocation = dataLocation + "/EditorQuest" + i;

        try
        {
            DirectoryCopy(key, targetLocation, true);
            string[] questData = File.ReadAllLines(targetLocation + "/quest.ini");

            bool questFound = false;
            for (i = 0; i < questData.Length; i++)
            {
                if (questData[i].Equals("[Quest]"))
                {
                    questFound = true;
                }
                if (questFound && questData[i].IndexOf("name=") == 0)
                {
                    questFound = false;
                    questData[i] = questData[i] + " (Copy)";
                }
            }

            File.WriteAllLines(targetLocation + "/quest.ini", questData);
        }
        catch (System.Exception)
        {
            Debug.Log("Error: Failed to copy quest.");
            Application.Quit();
        }
        new QuestEditSelection();
    }

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

    public void NewQuest()
    {

        string dataLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/Valkyrie";

        int i = 1;
        while (Directory.Exists(dataLocation + "/EditorQuest" + i))
        {
            i++;
        }
        string targetLocation = dataLocation + "/EditorQuest" + i;

        try
        {
            Directory.CreateDirectory(targetLocation);

            string[] questData = new string[2];

            questData[0] = "[Quest]";
            questData[1] = "name=EditorQuest" + i;

            File.WriteAllLines(targetLocation + "/quest.ini", questData);
        }
        catch (System.Exception)
        {
            Debug.Log("Error: Failed to create new quest.");
            Application.Quit();
        }
        new QuestEditSelection();
    }

    public void Selection(string key)
    {
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("dialog"))
            Object.Destroy(go);

        // In the build the content packs need to go into the build data dir, this is currently manual
        string contentLocation = Application.dataPath + "/valkyrie-contentpacks/";
        if (Application.isEditor)
        {
            // If running through unity then we assume you are using the git content, with the project at the same level
            contentLocation = Application.dataPath + "/../../valkyrie-contentpacks/";
        }

        // Fetch content (in future this will need to be selectable
        // Find any content packs at the location
        game.cd = new ContentData(contentLocation);
        // Check if we found anything
        if (game.cd.GetPacks().Count == 0)
        {
            Debug.Log("Error: Failed to find any content packs, please check that you have them present in: " + contentLocation);
        }

        // In the future this is where you select which packs to load, for now we load everything.
        foreach (string pack in game.cd.GetPacks())
        {
            game.cd.LoadContent(pack);
        }

        // Fetch all of the quest data
        game.qd = new QuestData(questList[key]);

        if (game.qd == null)
        {
            Debug.Log("Error: Unable to load quest: " + key);
            Destroyer.MainMenu();
        }
        else
        {
            QuestEditor.Begin();
        }
    }
}
