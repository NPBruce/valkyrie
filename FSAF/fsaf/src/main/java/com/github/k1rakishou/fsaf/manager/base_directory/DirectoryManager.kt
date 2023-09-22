package com.github.k1rakishou.fsaf.manager.base_directory

import android.content.Context
import android.net.Uri
import android.provider.DocumentsContract
import android.util.Log
import android.annotation.SuppressLint
import com.github.k1rakishou.fsaf.document_file.CachingDocumentFile
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.ExternalFile
import com.github.k1rakishou.fsaf.file.RawFile

/**
 * A class that is responsible for base directories registering/unregistering etc.
 * */
open class DirectoryManager(
  private val appContext: Context
) {
  private val baseDirList = mutableMapOf<Class<BaseDirectory>, BaseDirectory>()

  open fun registerBaseDir(clazz: Class<*>, baseDirectory: BaseDirectory) {
    checkConflictingPaths(baseDirectory)

    // User may revoke permissions at any time so we need to check whether we still have them, and
    // if not then, for now, log it.
    baseDirectory.getDirUri()?.let { dirUri ->
      if (dirUri.toString().isNotEmpty()) {
        checkBaseDirSafStillHasPermissions(dirUri)
      }
    }

    baseDirList[clazz as Class<BaseDirectory>] = baseDirectory
  }
  
  @SuppressLint("NewApi")
  fun checkBaseDirSafStillHasPermissions(dirUri: Uri): Boolean {
    val treeUri = DocumentsContract.buildTreeDocumentUri(
      dirUri.authority,
      DocumentsContract.getTreeDocumentId(dirUri)
    )

    var hasReadPermission = false
    var hasWritePermission = false

    val foundTreeUriInPermitted = appContext.contentResolver.persistedUriPermissions.any { permission ->
      if (permission.uri.toString() == treeUri.toString()) {
        if (!permission.isReadPermission) {
          Log.e(TAG, "SAF base directory ${treeUri} does not have read permission anymore!")
        }

        if (!permission.isWritePermission) {
          Log.e(TAG, "SAF base directory ${treeUri} does not have write permission anymore!")
        }

        hasReadPermission = permission.isReadPermission
        hasWritePermission = permission.isWritePermission

        return@any true
      }

      return@any false
    }

    if (!foundTreeUriInPermitted) {
      Log.e(TAG, "SAF base directory ${treeUri} has no permissions whatsoever anymore!")
    }

    return hasReadPermission && hasWritePermission
  }

  private fun checkConflictingPaths(baseDirectory: BaseDirectory) {
    val conflictingBaseDir = baseDirList.values.firstOrNull { baseDir ->
      if (baseDirectory.getDirFile() != null) {
        val path1 = baseDir.getDirFile()?.absolutePath ?: ""
        val path2 = baseDirectory.getDirFile()?.absolutePath ?: ""

        if (path1.isEmpty() && path2.isEmpty()) {
          return@firstOrNull false
        }

        if (path1 == path2) {
          return@firstOrNull true
        }
      }

      if (baseDirectory.getDirUri() != null) {
        val path1 = baseDir.getDirUri()?.toString() ?: ""
        val path2 = baseDirectory.getDirUri()?.toString() ?: ""

        if (path1.isEmpty() && path2.isEmpty()) {
          return@firstOrNull false
        }

        if (path1 == path2) {
          return@firstOrNull true
        }
      }

      return@firstOrNull false
    }

    if (conflictingBaseDir != null) {
      throw IllegalArgumentException(
        "A base dir with the same " +
          "dirFile (${conflictingBaseDir.getDirFile()}) or " +
          "dirUri (${conflictingBaseDir.getDirUri()}) " +
          "is already registered! Change the paths!"
      )
    }
  }

  fun isAlreadyRegistered(clazz: Class<*>, file: AbstractFile): Boolean {
    return baseDirList.entries.any { (dirClass, baseDirectory) ->
      if (dirClass == clazz) {
        return@any false
      }

      baseDirectory.isBaseDir(file)
    }
  }

  fun isAlreadyRegistered(clazz: Class<*>, filePath: String): Boolean {
    return baseDirList.entries.any { (dirClass, baseDirectory) ->
      if (dirClass == clazz) {
        return@any false
      }

      baseDirectory.isBaseDir(filePath)
    }
  }

  fun isAlreadyRegistered(clazz: Class<*>, uri: Uri): Boolean {
    return baseDirList.entries.any { (dirClass, baseDirectory) ->
      if (dirClass == clazz) {
        return@any false
      }

      baseDirectory.isBaseDir(uri)
    }
  }

  open fun unregisterBaseDir(clazz: Class<*>) {
    baseDirList.remove(clazz)
  }

  open fun isBaseDir(dir: AbstractFile): Boolean {
    return baseDirList.values.any { baseDir -> baseDir.isBaseDir(dir) }
  }

  open fun isBaseDir(dir: Uri): Boolean {
    return baseDirList.values.any { baseDir ->
      baseDir.isBaseDir(dir)
    }
  }

  open fun isBaseDir(dir: CachingDocumentFile): Boolean {
    if (!dir.isDirectory()) {
      Log.e(TAG, "dir ${dir.uri()} is not a directory")
      return false
    }

    return baseDirList.values.any { baseDir ->
      baseDir.isBaseDir(dir.uri())
    }
  }

  inline fun <reified T : BaseDirectory> getBaseDirByClass(): BaseDirectory? {
    return getBaseDirByClass(T::class.java)
  }

  open fun getBaseDirByClass(clazz: Class<*>): BaseDirectory? {
    return baseDirList[clazz as Class<BaseDirectory>]
  }

  open fun getBaseDir(dir: Uri): BaseDirectory? {
    return baseDirList.values.firstOrNull { baseDir ->
      baseDir.isBaseDir(dir)
    }
  }

  open fun getBaseDirThisFileBelongsTo(file: AbstractFile): BaseDirectory? {
    return baseDirList.values.firstOrNull { baseDir ->
      when (file) {
        is RawFile -> {
          val baseDirPath = baseDir.getDirFile()
            ?: return@firstOrNull false

          if (baseDirPath.absolutePath.isEmpty()) {
            Log.e(TAG, "getBaseDirThisFileBelongsTo() baseDirPath.absolutePath() is empty!")
            return@firstOrNull false
          }

          return@firstOrNull file.getFullPath().startsWith(baseDirPath.absolutePath)
        }
        is ExternalFile -> {
          val baseDirPathUri = baseDir.getDirUri()
            ?: return@firstOrNull false

          if (baseDirPathUri.toString().isEmpty()) {
            Log.e(TAG, "getBaseDirThisFileBelongsTo() baseDirPathUri is empty!")
            return@firstOrNull false
          }

          return@firstOrNull file.getFullPath().startsWith(baseDirPathUri.toString())
        }
        else -> throw NotImplementedError("Not implemented for ${file::class.java}")
      }
    }
  }

  companion object {
    private const val TAG = "DirectoryManager"
  }
}