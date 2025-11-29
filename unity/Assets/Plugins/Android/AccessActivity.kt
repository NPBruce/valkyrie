package com.android.accessmomdata

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.os.Environment
import android.util.Log
import android.view.View
import android.widget.TextView
import androidx.fragment.app.FragmentActivity
import androidx.documentfile.provider.DocumentFile
import com.github.k1rakishou.fsaf.FileChooser
import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.callback.FSAFActivityCallbacks
import com.github.k1rakishou.fsaf_app.AccessBaseDirectory
import java.io.File

class AccessActivity : Activity(), FSAFActivityCallbacks {

    private lateinit var fileManager: FileManager
    private lateinit var fileChooser: FileChooser

    private lateinit var sharedPreferences: SharedPreferences

    private val accessBaseDirectory = AccessBaseDirectory({
        getTreeUri()
    }, {
        null
    })

    companion object {
        if (DocumentVM.atLeastR()) {
            if (Build.VERSION.SDK_INT > 31) {
                docId += "/" + getIntent().getExtras().getString("targetPackageName")
            }
            Log.e(TAG, " onSelected: $docId")
        // make a new dir
        //val dir = root?.createDirectory("test")

        //list children
        root?.listFiles()?.let {
            val sb = StringBuilder()
            for (documentFile in it) {
                if(documentFile.isDirectory){
                    sb.append("📁")
                }
                sb.append(documentFile.name).append('\n')
            }
            tv?.text = sb.toString()
        }
    }

                deleteDirectory(file)
            }
        }
        return directoryToBeDeleted.delete()
    }


    private fun copyOfficialData() {
        try {
            val baseSAFDir = fileManager.newBaseDirectoryFile<AccessBaseDirectory>()
            if (baseSAFDir == null) {
                throw NullPointerException("baseSAFDir is null!")
            }

            val baseFileApiDir = fileManager.fromRawFile(
                File(Environment.getExternalStorageDirectory().absolutePath + "/Valkyrie" , getIntent().getExtras().getString("targetPackageName"))
            )
            val directory = File(Environment.getExternalStorageDirectory().absolutePath + "/Valkyrie" , getIntent().getExtras().getString("targetPackageName"))

            if (!directory.exists()) {
                directory.mkdir()
            }
            else {
                deleteDirectory(directory)
                directory.mkdir()
            }
            var packageDirFound = false

            if (baseSAFDir.getFullPath().endsWith(getIntent().getExtras().getString("targetPackageName"))) {
                fileManager.copyDirectoryWithContent(baseSAFDir, baseFileApiDir, true)
                packageDirFound = true
            }
            else {
                val innerFiles = fileManager.listFiles(baseSAFDir)
                
            
                innerFiles.forEach {
                    if (it.getFullPath().endsWith(getIntent().getExtras().getString("targetPackageName"))) {
                        Log.i(TAG, " from: $it to: $baseFileApiDir")

                        fileManager.copyDirectoryWithContent(it, baseFileApiDir, true)
                        packageDirFound = true
                    }
                }
            }

            if(!packageDirFound)
            {
                throw NullPointerException("packageDirFound not found!")
            }


	       val copyCompleteIndicationFile = File(Environment.getExternalStorageDirectory().absolutePath + "/Valkyrie/" + getIntent().getExtras().getString("targetPackageName") + "/done");
           if (!copyCompleteIndicationFile.exists()) {
               copyCompleteIndicationFile.createNewFile()
           }

            val message = "Copy completed"

	        Log.d(TAG, message)
        } catch (error: Throwable) {
	        Log.d(TAG, Log.getStackTraceString(error))
           val copyFailedIndicationFile = File(Environment.getExternalStorageDirectory().absolutePath + "/Valkyrie/" + getIntent().getExtras().getString("targetPackageName") + "/failed");
           if (!copyFailedIndicationFile.exists()) {
               copyFailedIndicationFile.createNewFile()
           }
        }
    }

    private fun storeTreeUri(uri: Uri) {
        val dir = checkNotNull(fileManager.fromUri(uri)) { "fileManager.fromUri(${uri}) failure" }

        check(fileManager.exists(dir)) { "Does not exist" }
        check(fileManager.isDirectory(dir)) { "Not a dir" }
        try {
            fileManager.registerBaseDir<AccessBaseDirectory>(accessBaseDirectory)
        }  catch (error: Throwable) {
	        Log.d(TAG, Log.getStackTraceString(error))
        }
        sharedPreferences.edit().putString(TREE_URI, uri.toString()).apply()
        Log.d(TAG, "storeTreeUri: $uri")
    }

    private fun removeTreeUri() {
        val treeUri = getTreeUri()
        if (treeUri == null) {
            println("Already removed")
            return
        }

        fileChooser.forgetSAFTree(treeUri)
        fileManager.unregisterBaseDir<AccessBaseDirectory>()
        sharedPreferences.edit().remove(TREE_URI).apply()
    }

    private fun getTreeUri(): Uri? {
        return sharedPreferences.getString(TREE_URI, null)
            ?.let { str -> Uri.parse(str) }
    }

    override fun fsafStartActivityForResult(intent: Intent, requestCode: Int) {
        //
        Log.i(TAG, "fsafStartActivityForResult")
    }

}