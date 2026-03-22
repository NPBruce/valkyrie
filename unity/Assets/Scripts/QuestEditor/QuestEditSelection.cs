using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using ValkyrieTools;
using Object = UnityEngine.Object;

public class QuestEditSelection
{
    public Dictionary<string, QuestData.Quest> questList;

    private static string editSearchFilter = "";
    private UIElementEditable editSearchInput = null;
    private string deleteSearchFilter = "";
    private UIElementEditable deleteSearchInput = null;
    private string copySearchFilter = "";
    private UIElementEditable copySearchInput = null;
    private static readonly StringKey SEARCH_BY_NAME = new StringKey("val", "SEARCH_BY_NAME");
    private static readonly StringKey CREATE = new StringKey("val", "CREATE");
    private static readonly StringKey FOLDER_NAME = new StringKey("val", "SCENARIO_FOLDER_NAME");
    private static readonly StringKey DISPLAY_NAME = new StringKey("val", "SCENARIO_DISPLAY_NAME");
    private static readonly StringKey NEW_QUEST_FOLDER_EXISTS = new StringKey("val", "NEW_QUEST_FOLDER_EXISTS");
    private static readonly StringKey NEW_QUEST_INVALID_NAME = new StringKey("val", "NEW_QUEST_INVALID_NAME");

    private UIElementEditable newQuestFolderInput = null;
    private UIElementEditable newQuestNameInput = null;

    // Create a pack with list of quests to edit
    public QuestEditSelection()
    {
        Game game = Game.Get();

        // clear list of local quests to make sure we take the latest changes
        game.questsList.UnloadLocalQuests();

        // Get list of unpacked quest in user location (editable)
        // TODO: open/save in packages
        questList = QuestLoader.GetUserUnpackedQuests();

        // If a dialog window is open we force it closed (this shouldn't happen)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Heading
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val","SELECT",game.gameType.QuestName()));
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        editSearchInput = new UIElementSearchBox();
        editSearchInput.SetLocation(UIScaler.GetHCenter(-9f), 4.2f, 18f, 1.5f);
        editSearchInput.SetText(editSearchFilter);
        editSearchInput.SetSingleLine();
        editSearchInput.SetPlaceholder(SEARCH_BY_NAME);
        editSearchInput.SetButton(delegate { PerformEditSearch(); });
        new UIElementBorder(editSearchInput, Color.grey);

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 6.5f, UIScaler.GetWidthUnits() - 2f, 19.5f);
        new UIElementBorder(scrollArea);

        // List of quests
        int offset = 0;
        foreach (KeyValuePair<string, QuestData.Quest> q in questList)
        {
            string key = q.Key;
            LocalizationRead.RemoveDictionary("qst");
            LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
            string translation = q.Value.name.Translate();

            if (!editSearchFilter.Equals("") && !translation.ToLower().Contains(editSearchFilter.ToLower()))
                continue;

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
            ui.SetText(new StringKey("val", "INDENT", translation), Color.black);
            ui.SetButton(delegate { Selection(key); });
            ui.SetBGColor(Color.white);
            offset += 2;
        }
        scrollArea.SetScrollSize(offset);

        // Main menu
        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Cancel);
        new UIElementBorder(ui, Color.red);
        // Delete a user quest
        ui = new UIElement();
        ui.SetLocation((UIScaler.GetRight() * 3 / 8) - 4, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.DELETE, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Delete);
        new UIElementBorder(ui, Color.red);
        // Copy a quest
        ui = new UIElement();
        ui.SetLocation((UIScaler.GetRight() * 5 / 8) - 4, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.COPY);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(Copy);
        new UIElementBorder(ui);
        // Create a new quest
        ui = new UIElement();
        ui.SetLocation(UIScaler.GetRight(-9), UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.NEW);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(delegate { NewQuestDialog(); });
        new UIElementBorder(ui);
    }

    public void Cancel()
    {
        editSearchFilter = "";
        GameStateManager.MainMenu();
    }

    // Change the dialog to a delete dialog
    public void Delete()
    {
        questList = QuestLoader.GetUserUnpackedQuests();
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Header
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val","SELECT_TO_DELETE",game.gameType.QuestName()));
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        deleteSearchInput = new UIElementSearchBox();
        deleteSearchInput.SetLocation(UIScaler.GetHCenter(-9f), 4.2f, 18f, 1.5f);
        deleteSearchInput.SetText(deleteSearchFilter);
        deleteSearchInput.SetSingleLine();
        deleteSearchInput.SetPlaceholder(SEARCH_BY_NAME);
        deleteSearchInput.SetButton(delegate { PerformDeleteSearch(); });
        new UIElementBorder(deleteSearchInput, Color.grey);

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 6.5f, UIScaler.GetWidthUnits() - 2f, 19.5f);
        new UIElementBorder(scrollArea);

        // List of quests
        int offset = 0;
        foreach (KeyValuePair<string, QuestData.Quest> q in questList)
        {
            string key = q.Key;
            LocalizationRead.RemoveDictionary("qst");
            LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
            string translation = q.Value.name.Translate();

            if (!deleteSearchFilter.Equals("") && !translation.ToLower().Contains(deleteSearchFilter.ToLower()))
                continue;

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
            ui.SetText(new StringKey("val", "INDENT", translation), Color.black);
            ui.SetButton(delegate { Delete(key); });
            ui.SetBGColor(Color.red);
            offset += 2;
        }
        scrollArea.SetScrollSize(offset);

        // Back to edit list
        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(CancelDelete);
        new UIElementBorder(ui, Color.red);
    }

    // Delete quest
    public void Delete(string key)
    {
        try
        {
            Directory.Delete(key, true);
        }
        catch (Exception)
        {
            ValkyrieDebug.Log("Failed to delete quest: " + key);
        }
        new QuestEditSelection();
    }

    private void PerformEditSearch()
    {
        if (editSearchInput == null) return;
        string newFilter = editSearchInput.GetText().Trim();
        if (newFilter.Equals(editSearchFilter)) return;
        editSearchFilter = newFilter;
        new QuestEditSelection();
    }

    private void PerformDeleteSearch()
    {
        if (deleteSearchInput == null) return;
        string newFilter = deleteSearchInput.GetText().Trim();
        if (newFilter.Equals(deleteSearchFilter)) return;
        deleteSearchFilter = newFilter;
        Delete();
    }

    public void CancelCopy()
    {
        copySearchFilter = "";
        new QuestEditSelection();
    }

    public void CancelDelete()
    {
        deleteSearchFilter = "";
        new QuestEditSelection();
    }

    // List of quests to copy
    public void Copy()
    {
        // Can copy all quests, not just user
        questList = QuestLoader.GetQuests(true);
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        // Header
        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(new StringKey("val","SELECT_TO_COPY",game.gameType.QuestName()));
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        // Search bar
        copySearchInput = new UIElementSearchBox();
        copySearchInput.SetLocation(UIScaler.GetHCenter(-9f), 4.2f, 18f, 1.5f);
        copySearchInput.SetText(copySearchFilter);
        copySearchInput.SetSingleLine();
        copySearchInput.SetPlaceholder(SEARCH_BY_NAME);
        copySearchInput.SetButton(delegate { PerformCopySearch(); });
        new UIElementBorder(copySearchInput, Color.grey);

        UIElementScrollVertical scrollArea = new UIElementScrollVertical();
        scrollArea.SetLocation(1, 6.5f, UIScaler.GetWidthUnits() - 2f, 19.5f);
        new UIElementBorder(scrollArea);

        // List of quests
        int offset = 0;
        foreach (KeyValuePair<string, QuestData.Quest> q in questList)
        {
            string key = q.Key;
            LocalizationRead.RemoveDictionary("qst");
            LocalizationRead.AddDictionary("qst", q.Value.localizationDict);
            string translation = q.Value.name.Translate();

            // Apply search filter
            if (!copySearchFilter.Equals("") && !translation.ToLower().Contains(copySearchFilter.ToLower()))
                continue;

            ui = new UIElement(scrollArea.GetScrollTransform());
            ui.SetLocation(1, offset, UIScaler.GetWidthUnits() - 5, 1.2f);
            ui.SetText(new StringKey("val", "INDENT", translation), Color.black);
            ui.SetButton(delegate { Copy(key); });
            ui.SetBGColor(Color.white);
            offset += 2;
        }
        scrollArea.SetScrollSize(offset);

        // Back to edit selection
        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(Game.Get().gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(CancelCopy);
        new UIElementBorder(ui, Color.red);
    }

    private void PerformCopySearch()
    {
        if (copySearchInput == null) return;
        string newFilter = copySearchInput.GetText().Trim();
        if (newFilter.Equals(copySearchFilter)) return;
        copySearchFilter = newFilter;
        Copy();
    }

    // Copy a quest
    public void Copy(string key)
    {
        Game game = Game.Get();
        string dataLocation = Game.AppData() + Path.DirectorySeparatorChar + Game.Get().gameType.TypeName() + "/Editor";
        if (!Directory.Exists(dataLocation))
        {
            Directory.CreateDirectory(dataLocation);
        }

        // Find a new unique directory name
        int uniqueDirectoryValue = 1;
        while (Directory.Exists(dataLocation + "/Editor" + game.gameType.QuestName().Translate() + uniqueDirectoryValue))
        {
            uniqueDirectoryValue++;
        }
        string targetLocation = dataLocation + "/Editor" + game.gameType.QuestName().Translate() + uniqueDirectoryValue;

        // Fully extract this scenario before copy if this is a package
        if (Path.GetExtension(Path.GetFileName(key)) == ValkyrieConstants.ScenarioDownloadContainerExtension)
        {
            // extract the full package
            ExtractManager.ExtractSinglePackageFull(ContentData.DownloadPath() + Path.DirectorySeparatorChar + Path.GetFileName(key));
        }

        // Copy files
        CopyFiles(key, targetLocation, uniqueDirectoryValue);

        // Back to selection
        new QuestEditSelection();
    }

    private void CopyFiles(string key, string targetLocation, int uniqueDirectoryValue)
    {
        try
        {
            DirectoryCopy(key, targetLocation, true);
            // read new quest file
            string[] questData = File.ReadAllLines(targetLocation + ValkyrieConstants.QuestIniFilePath);

            // Search for quest section
            bool questFound = false;
            for (uniqueDirectoryValue = 0; uniqueDirectoryValue < questData.Length; uniqueDirectoryValue++)
            {
                if (questData[uniqueDirectoryValue].Equals("[Quest]"))
                {
                    // Inside quest section
                    questFound = true;
                }
                if (questFound && questData[uniqueDirectoryValue].IndexOf("name=") == 0)
                {
                    // Add copy to name
                    questFound = false;
                    questData[uniqueDirectoryValue] = questData[uniqueDirectoryValue] + " (Copy)";
                }
            }
            // Write back to ini file
            File.WriteAllLines(targetLocation + ValkyrieConstants.QuestIniFilePath, questData);
        }
        catch (Exception)
        {
            ValkyrieDebug.Log("Error: Failed to copy quest.");
            Application.Quit();
        }
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

    public void NewQuestDialog(string errorMessage = "", string folderValue = "", string nameValue = "")
    {
        Game game = Game.Get();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        UIElement ui = new UIElement();
        ui.SetLocation(2, 1, UIScaler.GetWidthUnits() - 4, 3);
        ui.SetText(CommonStringKeys.NEW.Translate() + " " + game.gameType.QuestName().Translate());
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetLargeFont());

        float center = UIScaler.GetHCenter(-9f);

        ui = new UIElement();
        ui.SetLocation(center, 5.5f, 18f, 1.2f);
        ui.SetText(FOLDER_NAME, Color.white);

        newQuestFolderInput = new UIElementEditable();
        newQuestFolderInput.SetLocation(center, 7f, 18f, 1.5f);
        newQuestFolderInput.SetText(folderValue);
        newQuestFolderInput.SetSingleLine();
        newQuestFolderInput.SetAlphanumericOnly();
        new UIElementBorder(newQuestFolderInput, Color.grey);

        ui = new UIElement();
        ui.SetLocation(center, 9.5f, 18f, 1.2f);
        ui.SetText(DISPLAY_NAME, Color.white);

        newQuestNameInput = new UIElementEditable();
        newQuestNameInput.SetLocation(center, 11f, 18f, 1.5f);
        newQuestNameInput.SetText(nameValue);
        newQuestNameInput.SetSingleLine();
        new UIElementBorder(newQuestNameInput, Color.grey);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            ui = new UIElement();
            ui.SetLocation(center, 13.5f, 18f, 1.5f);
            ui.SetText(errorMessage, Color.red);
        }

        ui = new UIElement();
        ui.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CommonStringKeys.BACK, Color.red);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(delegate { new QuestEditSelection(); });
        new UIElementBorder(ui, Color.red);

        ui = new UIElement();
        ui.SetLocation(UIScaler.GetRight(-9), UIScaler.GetBottom(-3), 8, 2);
        ui.SetText(CREATE);
        ui.SetFont(game.gameType.GetHeaderFont());
        ui.SetFontSize(UIScaler.GetMediumFont());
        ui.SetButton(delegate { TryCreateQuest(); });
        new UIElementBorder(ui);
    }

    private void TryCreateQuest()
    {
        if (newQuestFolderInput == null || newQuestNameInput == null) return;

        string folderName = newQuestFolderInput.GetText().Trim();
        string displayName = newQuestNameInput.GetText().Trim();

        if (string.IsNullOrEmpty(folderName) || folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            NewQuestDialog(NEW_QUEST_INVALID_NAME.Translate(), folderName, displayName);
            return;
        }

        string dataLocation = Game.AppData() + Path.DirectorySeparatorChar + Game.Get().gameType.TypeName() + "/Editor";
        if (Directory.Exists(dataLocation + "/" + folderName))
        {
            NewQuestDialog(NEW_QUEST_FOLDER_EXISTS.Translate(), folderName, displayName);
            return;
        }

        if (string.IsNullOrEmpty(displayName))
            displayName = folderName;

        NewQuest(folderName, displayName);
    }

    private void NewQuest(string folderName, string displayName)
    {
        editSearchFilter = "";
        Game game = Game.Get();
        string dataLocation = Game.AppData() + Path.DirectorySeparatorChar + Game.Get().gameType.TypeName() + "/Editor";
        if (!Directory.Exists(dataLocation))
        {
            Directory.CreateDirectory(dataLocation);
        }

        string targetLocation = dataLocation + "/" + folderName;

        try
        {
            Directory.CreateDirectory(targetLocation);

            List<string> questData = new List<string>();
            questData.Add("[Quest]");
            questData.Add("type=" + game.gameType.TypeName());
            questData.Add("format=" + QuestData.Quest.currentFormat);
            questData.Add("defaultlanguage=" + game.currentLang);
            questData.Add("");
            questData.Add("[QuestText]");
            questData.Add("Localization." + game.currentLang + ".txt");

            File.WriteAllLines(targetLocation + ValkyrieConstants.QuestIniFilePath, questData.ToArray());

            DictionaryI18n newScenarioDict = new DictionaryI18n(new string[1] { ".," + game.currentLang }, game.currentLang);
            newScenarioDict.AddEntry("quest.name", displayName);
            newScenarioDict.AddEntry("quest.description", displayName + "...");

            Dictionary<string, List<string>> localization_files = newScenarioDict.SerializeMultiple();
            foreach (string oneLang in localization_files.Keys)
            {
                File.WriteAllText(
                    targetLocation + "/Localization." + oneLang + ".txt",
                    string.Join(Environment.NewLine, localization_files[oneLang].ToArray()));
            }
        }
        catch (Exception e)
        {
            ValkyrieDebug.Log("Error: Failed to create new quest: " + e.Message);
            Application.Quit();
        }
        new QuestEditSelection();
    }

    // Select a quest for editing
    public void Selection(string key)
    {
        ValkyrieDebug.Log("INFO: Select quest " + key + " for editing");

        Game game = Game.Get();

        // Remove all current components
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(Game.DIALOG))
            Object.Destroy(go);

        if (game.CurrentQuest!=null) game.CurrentQuest.RemoveAll();

        // Fetch all of the quest data
        ValkyrieDebug.Log("Selecting Quest: " + key + Environment.NewLine);
        GameStateManager.Editor.EditQuest(questList[key].path);
    }
}
