package com.github.k1rakishou.fsaf

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.provider.DocumentsContract
import android.util.Log
import android.webkit.MimeTypeMap
import android.annotation.SuppressLint
import androidx.documentfile.provider.DocumentFile
import com.github.k1rakishou.fsaf.callback.*
import com.github.k1rakishou.fsaf.callback.directory.DirectoryChooserCallback
import com.github.k1rakishou.fsaf.callback.directory.PermanentDirectoryChooserCallback
import com.github.k1rakishou.fsaf.callback.directory.TemporaryDirectoryCallback
import com.github.k1rakishou.fsaf.extensions.getMimeFromFilename

class FileChooser(
  private val appContext: Context
) {
  private val callbacksMap = hashMapOf<Int, ChooserCallback>()
  private val mimeTypeMap = MimeTypeMap.getSingleton()

  private var requestCode = 10000
  private var fsafActivityCallbacks: FSAFActivityCallbacks? = null

  fun setCallbacks(FSAFActivityCallbacks: FSAFActivityCallbacks) {
    this.fsafActivityCallbacks = FSAFActivityCallbacks
  }

  fun removeCallbacks() {
    this.fsafActivityCallbacks = null
  }

  /**
   * Use this method to get a user-selected directory for as your app's file dump directory.
   * Automatically requests all the necessary permissions including the persist permissions so that
   * the directory will stay visible for the app even after the phone reboots.
   * */
  fun openChooseDirectoryDialog(directoryChooserCallback: DirectoryChooserCallback) {
    fsafActivityCallbacks?.let { callbacks ->
      val intent = Intent(Intent.ACTION_OPEN_DOCUMENT_TREE)
      intent.putExtra("android.content.extra.SHOW_ADVANCED", true)

      val flags = if (directoryChooserCallback is PermanentDirectoryChooserCallback) {
        Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION or
          Intent.FLAG_GRANT_READ_URI_PERMISSION or
          Intent.FLAG_GRANT_WRITE_URI_PERMISSION
      } else {
        Intent.FLAG_GRANT_READ_URI_PERMISSION or
          Intent.FLAG_GRANT_WRITE_URI_PERMISSION
      }

      intent.addFlags(flags)

      // Do not use any remote providers (dropbox/google drive/etc since they may not work correctly
      // with ACTION_OPEN_DOCUMENT_TREE and persistable permissions)
      intent.putExtra(Intent.EXTRA_LOCAL_ONLY, true)

      val nextRequestCode = ++requestCode
      callbacksMap[nextRequestCode] = directoryChooserCallback as ChooserCallback

      try {
        callbacks.fsafStartActivityForResult(intent, nextRequestCode)
      } catch (e: Exception) {
        callbacksMap.remove(nextRequestCode)
        directoryChooserCallback.onCancel(e.message ?: "openChooseDirectoryDialog() Unknown error")
      }
    }
  }

  /**
   * Use this method to get a single user-selected file with all of the necessary permissions.
   * */
  fun openChooseFileDialog(fileChooserCallback: FileChooserCallback) {
    fsafActivityCallbacks?.let { callbacks ->
      val intent = Intent(Intent.ACTION_OPEN_DOCUMENT)
      intent.addFlags(
        Intent.FLAG_GRANT_READ_URI_PERMISSION or
          Intent.FLAG_GRANT_WRITE_URI_PERMISSION
      )

      intent.addCategory(Intent.CATEGORY_OPENABLE)
      intent.type = "*/*"

      val nextRequestCode = ++requestCode
      callbacksMap[nextRequestCode] = fileChooserCallback as ChooserCallback

      try {
        callbacks.fsafStartActivityForResult(intent, nextRequestCode)
      } catch (e: Exception) {
        callbacksMap.remove(nextRequestCode)
        fileChooserCallback.onCancel(e.message ?: "openChooseFileDialog() Unknown error")
      }
    }
  }

  /**
   * Use this method to get multiple user-selected files with all of the necessary permissions.
   * */
  fun openChooseMultiSelectFileDialog(
    fileMultiSelectChooserCallback: FileMultiSelectChooserCallback
  ) {
    fsafActivityCallbacks?.let { callbacks ->
      val intent = Intent(Intent.ACTION_OPEN_DOCUMENT)
      intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION or Intent.FLAG_GRANT_WRITE_URI_PERMISSION)

      intent.addCategory(Intent.CATEGORY_OPENABLE)
      intent.type = "*/*"
      intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true) // !

      val nextRequestCode = ++requestCode
      callbacksMap[nextRequestCode] = fileMultiSelectChooserCallback

      try {
        callbacks.fsafStartActivityForResult(intent, nextRequestCode)
      } catch (e: Exception) {
        callbacksMap.remove(nextRequestCode)
        fileMultiSelectChooserCallback.onCancel(e.message ?: "openChooseMultiSelectFileDialog() Unknown error")
      }
    }
  }

  /**
   * Use this method to get a user-selected path where a new file will be located with all of the
   * necessary permissions.
   * */
  fun openCreateFileDialog(
    fileName: String,
    fileCreateCallback: FileCreateCallback
  ) {
    fsafActivityCallbacks?.let { callbacks ->
      val intent = Intent(Intent.ACTION_CREATE_DOCUMENT)
      intent.addFlags(
        Intent.FLAG_GRANT_READ_URI_PERMISSION or
          Intent.FLAG_GRANT_WRITE_URI_PERMISSION
      )

      intent.addCategory(Intent.CATEGORY_OPENABLE)
      intent.type = mimeTypeMap.getMimeFromFilename(fileName)
      intent.putExtra(Intent.EXTRA_TITLE, fileName)

      val nextRequestCode = ++requestCode
      callbacksMap[nextRequestCode] = fileCreateCallback as ChooserCallback

      try {
        callbacks.fsafStartActivityForResult(intent, nextRequestCode)
      } catch (e: Exception) {
        callbacksMap.remove(nextRequestCode)
        fileCreateCallback.onCancel(e.message ?: "openCreateFileDialog() Unknown error")
      }
    }
  }

  /**
   * Override the onActivityResult in your base activity and call this method inside so that the
   * library can handle the SAF response from the system.
   * */
  fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?): Boolean {
    val callback = callbacksMap[requestCode]
    if (callback == null) {
      Log.d(TAG, "Callback is already removed from the map, resultCode = $requestCode")
      return false
    }

    try {
      if (fsafActivityCallbacks == null) {
        // Skip all requests when the callback is not set
        Log.d(TAG, "Callback is not attached")
        return false
      }

      when (callback) {
        is DirectoryChooserCallback -> {
          handleDirectoryChooserCallback(callback, resultCode, data)
        }
        is FileChooserCallback -> {
          handleFileChooserCallback(callback, resultCode, data)
        }
        is FileCreateCallback -> {
          handleFileCreateCallback(callback, resultCode, data)
        }
        is FileMultiSelectChooserCallback -> {
          handleFileChooserMultiSelectCallback(callback, resultCode, data)
        }
        else -> throw IllegalArgumentException("Not implemented for ${callback.javaClass.name}")
      }

      return true
    } finally {
      callbacksMap.remove(requestCode)
    }
  }

  private fun handleFileCreateCallback(
    callback: FileCreateCallback,
    resultCode: Int,
    intent: Intent?
  ) {
    if (resultCode != Activity.RESULT_OK) {
      val msg = "handleFileCreateCallback() Non OK result ($resultCode)"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    if (intent == null) {
      val msg = "handleFileCreateCallback() Intent is null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val read = (intent.flags and Intent.FLAG_GRANT_READ_URI_PERMISSION) != 0
    if (!read) {
      val msg = "handleFileCreateCallback() No grant read uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val write = (intent.flags and Intent.FLAG_GRANT_WRITE_URI_PERMISSION) != 0
    if (!write) {
      val msg = "handleFileCreateCallback() No grant write uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val uri = intent.data
    if (uri == null) {
      val msg = "handleFileCreateCallback() intent.getData() == null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    callback.onResult(uri)
  }

  private fun handleFileChooserCallback(
    callback: FileChooserCallback,
    resultCode: Int,
    intent: Intent?
  ) {
    if (resultCode != Activity.RESULT_OK) {
      val msg = "handleFileChooserCallback() Non OK result ($resultCode)"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    if (intent == null) {
      val msg = "handleFileChooserCallback() Intent is null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val read = (intent.flags and Intent.FLAG_GRANT_READ_URI_PERMISSION) != 0
    if (!read) {
      val msg = "handleFileChooserCallback() No grant read uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val write = (intent.flags and Intent.FLAG_GRANT_WRITE_URI_PERMISSION) != 0
    if (!write) {
      val msg = "handleFileChooserCallback() No grant write uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val uri = intent.data
    if (uri == null) {
      val msg = "handleFileChooserCallback() intent.getData() == null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    callback.onResult(uri)
  }

  private fun handleFileChooserMultiSelectCallback(
    callback: FileMultiSelectChooserCallback,
    resultCode: Int,
    intent: Intent?
  ) {
    if (resultCode != Activity.RESULT_OK) {
      val msg = "handleFileChooserMultiSelectCallback() Non OK result ($resultCode)"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    if (intent == null) {
      val msg = "handleFileChooserMultiSelectCallback() Intent is null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val read = (intent.flags and Intent.FLAG_GRANT_READ_URI_PERMISSION) != 0
    if (!read) {
      val msg = "handleFileChooserMultiSelectCallback() No grant read uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val write = (intent.flags and Intent.FLAG_GRANT_WRITE_URI_PERMISSION) != 0
    if (!write) {
      val msg = "handleFileChooserMultiSelectCallback() No grant write uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    if (intent.data == null && intent.clipData == null) {
      callback.onResult(emptyList())
      return
    }

    val uris: ArrayList<Uri>?

    val clipData = intent.clipData
    if (null != clipData) { // checking multiple selection or not
      uris = arrayListOf<Uri>()
      for (i in 0 until clipData.itemCount) {
        uris.add(clipData.getItemAt(i).uri)
      }
    } else {
      val data = intent.data
      if (data != null) {
        uris = arrayListOf<Uri>()
        uris.add(data)
      } else {
        uris = null
      }
    }

    if (uris == null) {
      val msg =
        "handleFileChooserMultiSelectCallback() intent.getData() == null ${intent.data == null} or intent.getClipData() == null ${intent.clipData == null}"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    callback.onResult(uris)
  }
  @SuppressLint("NewApi")
  private fun handleDirectoryChooserCallback(
    callback: DirectoryChooserCallback,
    resultCode: Int,
    intent: Intent?
  ) {
    if (resultCode != Activity.RESULT_OK) {
      val msg = "handleDirectoryChooserCallback() Non OK result ($resultCode)"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    if (intent == null) {
      val msg = "handleDirectoryChooserCallback() Intent is null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val read = (intent.flags and Intent.FLAG_GRANT_READ_URI_PERMISSION) != 0
    if (!read) {
      val msg = "handleDirectoryChooserCallback() No grant read uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    val write = (intent.flags and Intent.FLAG_GRANT_WRITE_URI_PERMISSION) != 0
    if (!write) {
      val msg = "handleDirectoryChooserCallback() No grant write uri permission given"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    if (callback is PermanentDirectoryChooserCallback) {
      val persist = (intent.flags and Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION) != 0
      if (!persist) {
        val msg = "handleDirectoryChooserCallback() No grant persist uri permission given"

        Log.e(TAG, msg)
        callback.onCancel(msg)
        return
      }
    }

    val uri = intent.data
    if (uri == null) {
      val msg = "handleDirectoryChooserCallback() intent.getData() == null"

      Log.e(TAG, msg)
      callback.onCancel(msg)
      return
    }

    Log.d(TAG, "treeUri = ${uri}")

    if (callback is TemporaryDirectoryCallback) {
      callback.onResult(uri)
      return
    }

    val flags = Intent.FLAG_GRANT_READ_URI_PERMISSION or
      Intent.FLAG_GRANT_WRITE_URI_PERMISSION

    val contentResolver = appContext.contentResolver
    contentResolver.takePersistableUriPermission(uri, flags)
    callback.onResult(uri)
  }

  /**
   * Use this method to remove all of the permissions from the base directory by it's uri
   * */
  @SuppressLint("NewApi")
  fun forgetSAFTree(directoryUri: Uri): Boolean {
    val directory = DocumentFile.fromTreeUri(appContext, directoryUri)
    if (directory == null) {
      Log.e(TAG, "forgetSAFTree() DocumentFile.fromTreeUri returned null")
      return false
    }

    if (!directory.exists()) {
      Log.e(
        TAG, "Couldn't revoke permissions from directory because it does not exist, " +
          "path = $directoryUri"
      )
      return false
    }

    if (!directory.isDirectory) {
      Log.e(
        TAG, "Couldn't revoke permissions from directory it is not a directory, " +
          "path = $directoryUri"
      )
      return false
    }

    return try {
      val flags = Intent.FLAG_GRANT_READ_URI_PERMISSION or
        Intent.FLAG_GRANT_WRITE_URI_PERMISSION

      val rootUri = DocumentsContract.buildTreeDocumentUri(
        directoryUri.authority,
        DocumentsContract.getTreeDocumentId(directoryUri)
      )

      appContext.contentResolver.releasePersistableUriPermission(rootUri, flags)
      appContext.revokeUriPermission(rootUri, flags)

      Log.d(TAG, "Revoke old path permissions success on $directoryUri")
      true
    } catch (err: Exception) {
      Log.e(TAG, "Error revoking old path permissions on $directoryUri", err)
      false
    }
  }

  companion object {
    private const val TAG = "FileChooser"
  }
}