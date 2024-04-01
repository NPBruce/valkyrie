package com.android.accessmomdata

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.provider.DocumentsContract
import androidx.annotation.RequiresApi
import android.util.Log

class DocumentVM {
    companion object {

        const val DOC_AUTHORITY = "com.android.externalstorage.documents"

        private const val TAG = "DocumentVM"


        @JvmStatic
        @RequiresApi(Build.VERSION_CODES.O)
        fun requestFolderPermission(activity: Activity, requestCode: Int, id: String?) {
            val i = getUriOpenIntent(getFolderUri(id, false))

            val flags = Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION or
                    Intent.FLAG_GRANT_READ_URI_PERMISSION or
                    Intent.FLAG_GRANT_WRITE_URI_PERMISSION
            i.addFlags(flags)

            activity.startActivityForResult(i, requestCode)
        }

        @JvmStatic
        fun getFolderUri(id: String?, tree: Boolean): Uri {
            return if (tree) DocumentsContract.buildTreeDocumentUri(DOC_AUTHORITY, id) else DocumentsContract.buildDocumentUri(DOC_AUTHORITY, id)
        }

        @JvmStatic
        @RequiresApi(Build.VERSION_CODES.O)
        fun getUriOpenIntent(uri: Uri): Intent {
            return Intent(Intent.ACTION_OPEN_DOCUMENT_TREE)
                .putExtra("android.provider.extra.SHOW_ADVANCED", true)
                .putExtra("android.content.extra.SHOW_ADVANCED", true)
                .putExtra(DocumentsContract.EXTRA_INITIAL_URI, uri)
        }

        @JvmStatic
        fun checkFolderPermission(context: Context, id: String?): Boolean {
            return if (atLeastR()) {
                val treeUri: Uri = getFolderUri(id, true)
                Log.e(TAG, "treeUri:" + treeUri)
                isInPersistedUriPermissions(context, treeUri)
            } else {
                true
            }
        }

        @JvmStatic
        @RequiresApi(Build.VERSION_CODES.KITKAT)
        fun isInPersistedUriPermissions(context: Context, uri: Uri): Boolean {
            val pList = context.contentResolver.persistedUriPermissions
            Log.e(TAG, "pList:" + pList.size)
            for (uriPermission in pList) {
                Log.e(TAG, "uriPermission:$uriPermission")
                if (uriPermission.uri == uri && (uriPermission.isReadPermission || uriPermission.isWritePermission)) {
                    return true
                } else {
                    Log.e(TAG, "up:" + uriPermission.uri)
                }
            }
            return false
        }

        fun atLeastTiramisu(): Boolean {
            return Build.VERSION.SDK_INT >= 33
        }

        fun atLeastR(): Boolean {
            return Build.VERSION.SDK_INT >= 30
        }
    }

}