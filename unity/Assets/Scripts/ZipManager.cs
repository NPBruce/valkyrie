using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using ValkyrieTools;
using System.Threading;

public class ZipManager : MonoBehaviour
{
    static private Thread _jobHandle;
    static private bool _job_started = false;

    static private string local_tempPath;
    static private string local_quest_path;
    static private string local_archive_path;
    static private bool local_update;

    static private void _Execute()
    {
        try
        {
            if (!local_update)
            {
                // New savefile with quest
                ZipFile zip = new ZipFile();
                zip.AddFile(Path.Combine(local_tempPath, "save.ini"), "");
                zip.AddFile(Path.Combine(local_tempPath, "image.png"), "");
                zip.AddDirectory(local_quest_path, "quest");
                zip.Save(local_archive_path);
                zip.Dispose();
            }
            else
            {
                // Update savefile without quest
                ZipFile zip = ZipFile.Read(local_archive_path);
                zip.UpdateFile(Path.Combine(local_tempPath, "save.ini") , "");
                zip.UpdateFile(Path.Combine(local_tempPath, "image.png") , "");
                zip.Save();
                zip.Dispose();
            }
        }
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Warning: Unable to write file: " + local_archive_path + "\nException: "+ e.ToString()) ;
        }
    }

    static public void WriteZipAsync(string tempPath, string quest_path, string archive_path, bool update)
    {
        if (_job_started)
        {
            _jobHandle.Join();
        }

        local_tempPath = tempPath;
        local_quest_path = quest_path;
        local_archive_path = archive_path;
        local_update = update;

        _jobHandle = new Thread(_Execute);
        _jobHandle.Start();

        _job_started = true;
    }

    static public void Wait4PreviousSave()
    {
        if (_job_started)
        {
            _jobHandle.Join();
            _job_started = false;
        }
    }

    public enum Extract_mode
    {
        ZIPMANAGER_EXTRACT_FULL=0,
        ZIPMANAGER_EXTRACT_INI_TXT,
        ZIPMANAGER_EXTRACT_INI_TXT_PIC,
        ZIPMANAGER_EXTRACT_SAVE_INI_PIC
    };


    static public void Extract(string target_path, string archive_name, Extract_mode mode)
    {
        // make sure save is done, to not manipulate file being currently written
        if (_job_started)
        {
            _jobHandle.Join();
            _job_started = false;
        }

        try
        {
            if (!Directory.Exists(target_path))
            {
                Directory.CreateDirectory(target_path);
            }

            ZipFile zip = ZipFile.Read(archive_name);

            if (mode == Extract_mode.ZIPMANAGER_EXTRACT_FULL)
            {
                zip.ExtractAll(target_path, ExtractExistingFileAction.OverwriteSilently);
            }

            if (mode == Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT || mode == Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT_PIC)
            {
                zip.ExtractSelectedEntries("name = quest.ini", null, target_path, ExtractExistingFileAction.OverwriteSilently);
                zip.ExtractSelectedEntries("name = Localization.*.txt", null, target_path, ExtractExistingFileAction.OverwriteSilently);

                if(mode == Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT_PIC)
                {
                    Dictionary<string, string> iniData = IniRead.ReadFromIni(target_path + "/quest.ini", "Quest");
                    if (iniData.ContainsKey("image"))
                        zip.ExtractSelectedEntries("name = " + iniData["image"], null, target_path, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            if (mode == Extract_mode.ZIPMANAGER_EXTRACT_SAVE_INI_PIC)
            {
                zip.ExtractSelectedEntries("name = save.ini", null, target_path, ExtractExistingFileAction.OverwriteSilently);
                zip.ExtractSelectedEntries("name = image.png", null, target_path, ExtractExistingFileAction.OverwriteSilently);

                // search in subfolder (* before filename is required for Android)
                zip.ExtractSelectedEntries("name = *quest.ini", null, target_path, ExtractExistingFileAction.OverwriteSilently);
                zip.ExtractSelectedEntries("name = *Localization.*.txt", null, target_path, ExtractExistingFileAction.OverwriteSilently);
            }

            zip.Dispose();
        }
        catch (System.Exception e)
        {
            ValkyrieDebug.Log("Warning: Unable to read file: " + archive_name + "\nException" + e.ToString());
        }
    }

}