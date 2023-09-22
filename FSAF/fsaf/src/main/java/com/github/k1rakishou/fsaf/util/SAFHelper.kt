package com.github.k1rakishou.fsaf.util

import android.content.Context
import android.database.Cursor
import android.net.Uri
import android.os.Build
import android.provider.DocumentsContract
import android.util.Log
import android.annotation.SuppressLint
import androidx.documentfile.provider.DocumentFile
import com.github.k1rakishou.fsaf.document_file.CachingDocumentFile
import com.github.k1rakishou.fsaf.document_file.SnapshotDocumentFile
import com.github.k1rakishou.fsaf.extensions.safeCapacity
import com.github.k1rakishou.fsaf.file.Segment
import com.github.k1rakishou.fsaf.manager.base_directory.BaseDirectory
import com.github.k1rakishou.fsaf.manager.base_directory.DirectoryManager
import java.util.*
import kotlin.collections.ArrayList

object SAFHelper {
  private const val TAG = "SAFHelper"
  private const val PATH_TREE = "tree"
  private const val SQLITE_IN_OPERATOR_BATCH_SIZE = 950

  /**
   * We want to sort files with the most segments count being at the beginning of the list, i.e:
   * /123
   * /123/456
   * /123/456/567
   * /123/1.txt
   * /123/2.txt
   * /123/3.txt
   * /123/456/1.txt
   * /123/456/2.txt
   * /123/456/3.txt
   * /123/456/567/1.txt
   * /123/456/567/2.txt
   * /123/456/567/3.txt
   * /123/456/567/4.txt
   *
   * Why? Because if the order is reversed, and lets say only the /123 directory exists, then if we
   * attempt to access the innermost file we will likely get an exception because intermediate
   * directories do not exist.
   * */
  private val ITERATED_FILE_COMPARATOR = Comparator<IteratedFile<*>> { iterFile1, iterFile2 ->
    if (iterFile1.isDirectory) {
      return@Comparator -1
    }

    if (iterFile2.isDirectory) {
      return@Comparator 1
    }

    return@Comparator 0
  }

  private val columns = arrayOf(
    DocumentsContract.Document.COLUMN_DOCUMENT_ID,
    DocumentsContract.Document.COLUMN_MIME_TYPE,
    DocumentsContract.Document.COLUMN_DISPLAY_NAME,
    DocumentsContract.Document.COLUMN_LAST_MODIFIED,
    DocumentsContract.Document.COLUMN_FLAGS,
    DocumentsContract.Document.COLUMN_SIZE
  )

  /**
   * Finds a file that has multiple layers of sub directories
   * */
  fun findDeepFile(
    appContext: Context,
    parentUri: Uri,
    segments: List<Segment>,
    directoryManager: DirectoryManager
  ): SnapshotDocumentFile? {
    check(segments.isNotEmpty()) { "segments must not be empty" }

    var file: SnapshotDocumentFile? = null
    for (segment in segments) {
      val uri = file?.uri() ?: parentUri

      file = findSnapshotFile(
        appContext,
        uri,
        segment.name,
        directoryManager.isBaseDir(uri)
      )

      if (file == null) {
        return null
      }
    }

    return file
  }

  fun findCachingFile(
    appContext: Context,
    parentUri: Uri,
    name: String,
    isTreeUri: Boolean
  ): CachingDocumentFile? {
    return findFile(appContext, parentUri, name, isTreeUri) { documentFile, preloadedInfo ->
      return@findFile SnapshotDocumentFile(
        appContext,
        documentFile,
        preloadedInfo.displayName,
        preloadedInfo.mimeType,
        preloadedInfo.flags,
        preloadedInfo.lastModified,
        preloadedInfo.size
      )
    }
  }

  fun findSnapshotFile(
    appContext: Context,
    parentUri: Uri,
    name: String,
    isTreeUri: Boolean
  ): SnapshotDocumentFile? {
    return findFile(appContext, parentUri, name, isTreeUri) { documentFile, preloadedInfo ->
      return@findFile SnapshotDocumentFile(
        appContext,
        documentFile,
        preloadedInfo.displayName,
        preloadedInfo.mimeType,
        preloadedInfo.flags,
        preloadedInfo.lastModified,
        preloadedInfo.size
      )
    }
  }

  /**
   * Finds a file and preloads all of it's info (like name size mime type etc.) because every one
   * of those operations require and IPC call which slow as fuck. So it's way faster to just preload
   * all that shit at once
   * */
  @SuppressLint("NewApi")
  fun <T> findFile(
    appContext: Context,
    parentUri: Uri,
    name: String,
    isTreeUri: Boolean,
    mapper: (DocumentFile, PreloadedInfo) -> T
  ): T? {
    val selection = DocumentsContract.Document.COLUMN_DISPLAY_NAME + " = ?"
    val childrenUri = buildChildrenUri(isTreeUri, appContext, parentUri)

    val contentResolver = appContext.contentResolver
    val filename = name.toLowerCase(Locale.ROOT)

    return contentResolver.query(
      childrenUri,
      columns,
      selection,
      arrayOf(filename),
      null
    )?.use { cursor ->
      while (cursor.moveToNext()) {
        val displayName = cursor.getString(2)
        if (displayName.toLowerCase(Locale.ROOT) != filename) {
          continue
        }

        val documentId = cursor.getString(0)
        val mimeType = cursor.getString(1)
        val lastModified = cursor.getLong(3)
        val flags = cursor.getInt(4)
        val size = cursor.getLong(5)

        val fileUri = DocumentsContract.buildDocumentUriUsingTree(
          parentUri,
          documentId
        )

        val documentFile = DocumentFile.fromSingleUri(
          appContext,
          fileUri
        )

        if (documentFile == null) {
          return@use null
        }

        return@use mapper(
          documentFile,
          PreloadedInfo(
            documentId,
            mimeType,
            displayName,
            lastModified,
            flags,
            size
          )
        )
      }

      return@use null
    }
  }
  @SuppressLint("NewApi")
  fun <T> findManyFilesInDir(
    appContext: Context,
    dirUri: Uri,
    nameList: List<String>,
    mapper: (DocumentFile, PreloadedInfo) -> T
  ): List<T> {
    if (nameList.isEmpty()) {
      return emptyList()
    }

    val nameListBatched = nameList.chunked(SQLITE_IN_OPERATOR_BATCH_SIZE)
    val resultList = ArrayList<IteratedFile<T>>(safeCapacity(nameList))

    for (batch in nameListBatched) {
      val selection = DocumentsContract.Document.COLUMN_DISPLAY_NAME +
        " IN (" + batch.joinToString(separator = "?,") + ")"

      // FIXME: won't work if dirUri is not the treeDirectory (the root, or the base directory)
      //  but a nested directory
      val childrenUri = DocumentsContract.buildChildDocumentsUriUsingTree(
        dirUri,
        DocumentsContract.getTreeDocumentId(dirUri)
      )

      val contentResolver = appContext.contentResolver
      val lowerCaseNameSet = batch
        .map { name -> name.toLowerCase(Locale.ROOT) }
        .toSet()

      resultList += contentResolver.query(
        childrenUri,
        columns,
        selection,
        batch.toTypedArray(),
        null
      )
        ?.use { cursor -> iterateCursor(appContext, dirUri, cursor, lowerCaseNameSet, mapper) }
        ?: emptyList()
    }

    return resultList.sortedWith(ITERATED_FILE_COMPARATOR)
      .map { iteratedFile -> iteratedFile.file }
  }

  @SuppressLint("NewApi")
  private fun <T> iterateCursor(
    appContext: Context,
    dirUri: Uri,
    cursor: Cursor,
    lowerCaseNameSet: Set<String>,
    mapper: (DocumentFile, PreloadedInfo) -> T
  ): MutableList<IteratedFile<T>> {
    val resultList = mutableListOf<IteratedFile<T>>()

    while (cursor.moveToNext()) {
      val displayName = cursor.getString(2)
      if (displayName.toLowerCase(Locale.ROOT) !in lowerCaseNameSet) {
        continue
      }

      val documentId = cursor.getString(0)
      val mimeType = cursor.getString(1)
      val lastModified = cursor.getLong(3)
      val flags = cursor.getInt(4)
      val size = cursor.getLong(5)

      val fileUri = DocumentsContract.buildDocumentUriUsingTree(
        dirUri,
        documentId
      )

      val documentFile = DocumentFile.fromSingleUri(
        appContext,
        fileUri
      )

      if (documentFile == null) {
        continue
      }

      val mapped = mapper(
        documentFile,
        PreloadedInfo(
          documentId,
          mimeType,
          displayName,
          lastModified,
          flags,
          size
        )
      )

      if (mimeType == DocumentsContract.Document.MIME_TYPE_DIR) {
        resultList.add(0, IteratedFile(mapped, true))
      } else {
        resultList += IteratedFile(mapped, false)
      }
    }
    return resultList
  }

  /**
   * Same as above but preloads the whole directory.
   * */

  @SuppressLint("NewApi")
  fun listFilesFast(
    appContext: Context,
    parentUri: Uri,
    isTreeUri: Boolean
  ): List<SnapshotDocumentFile> {
    val resolver = appContext.contentResolver
    val childrenUri = buildChildrenUri(isTreeUri, appContext, parentUri)

    return resolver.query(childrenUri, columns, null, null, null)?.use { cursor ->
      val result = mutableListOf<SnapshotDocumentFile>()

      while (cursor.moveToNext()) {
        val documentId = cursor.getString(0)
        val mimeType = cursor.getString(1)
        val displayName = cursor.getString(2)
        val lastModified = cursor.getLong(3)
        val flags = cursor.getInt(4)
        val size = cursor.getLong(5)

        val documentUri = DocumentsContract.buildDocumentUriUsingTree(
          parentUri,
          documentId
        )

        check(parentUri != documentUri) {
          "Result documentUri is the same as the parentUri, docUri = $documentUri"
        }

        val documentFile = if (mimeType == DocumentsContract.Document.MIME_TYPE_DIR) {
          DocumentFile.fromTreeUri(appContext, documentUri)
        } else {
          DocumentFile.fromSingleUri(appContext, documentUri)
        }
	
        Log.e(TAG, "listFilesFast() documentFile == $documentFile, mimeType = $mimeType")


        if (documentFile == null) {
          Log.e(TAG, "listFilesFast() documentFile == null, mimeType = $mimeType")
          continue
        }

        val snapshotDocumentFile = SnapshotDocumentFile(
          appContext,
          documentFile,
          displayName,
          mimeType,
          flags,
          lastModified,
          size
        )

        if (mimeType == DocumentsContract.Document.MIME_TYPE_DIR) {
          // Insert directories at the beginning
          result.add(0, snapshotDocumentFile)
        } else {
          // And files at the end
          result += snapshotDocumentFile
        }
      }

      return@use result
    } ?: emptyList()
  }

  @SuppressLint("NewApi")
  private fun buildChildrenUri(
    isTreeUri: Boolean,
    appContext: Context,
    parentUri: Uri
  ): Uri {
    return if (isTreeUri) {
      DocumentsContract.buildChildDocumentsUriUsingTree(
        DocumentFile.fromTreeUri(appContext, parentUri)!!.uri,
        DocumentsContract.getTreeDocumentId(parentUri)
      )
    } else {
      DocumentsContract.buildChildDocumentsUriUsingTree(
        parentUri,
        DocumentsContract.getDocumentId(parentUri)
      )
    }
  }

  fun isTreeUri(baseDir: BaseDirectory): Boolean {
    val dirUri = baseDir.getDirUri()
      ?: return false

    return isTreeUri(dirUri)
  }

  /**
   * Checks whether a [uri] is a tree uri
   * */
  fun isTreeUri(uri: Uri): Boolean {
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
      return DocumentsContract.isTreeUri(uri)
    }

    // HACK because this shit can only be used in Nougat and above
    val paths = uri.pathSegments
    return paths.size >= 2 && PATH_TREE == paths[0]
  }

  private class IteratedFile<T>(val file: T, val isDirectory: Boolean)

  class PreloadedInfo(
    val documentId: String,
    val mimeType: String,
    val displayName: String,
    val lastModified: Long,
    val flags: Int,
    val size: Long
  )

}