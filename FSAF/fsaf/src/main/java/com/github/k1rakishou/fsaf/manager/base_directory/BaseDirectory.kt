package com.github.k1rakishou.fsaf.manager.base_directory

import android.net.Uri
import com.github.k1rakishou.fsaf.document_file.CachingDocumentFile
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.ExternalFile
import com.github.k1rakishou.fsaf.file.RawFile
import java.io.File

/**
 * Base directory is useful when you want to have a file dump directory for your app (like a
 * directory where you will be storing downloaded files or some user selected directory to be used
 * by your app). It may have a SAF directory (like on sd-card) and a regular java-file backed
 * directory (like a directory in the internal app storage or external like the Downloads directory)
 * or even both at the same time. When having both you may set up a mechanism that will
 * automatically switch from the SAF directory to the java backed directory if SAF directory is
 * deleted by the user of the permissions are not granted for that directory anymore.
 *
 * If you want to have a base directory you need to inherit from this class and then add register it
 * in the [DirectoryManager] via the [FileManager.registerBaseDir] method
 * */
abstract class BaseDirectory {

  fun isBaseDir(dirPath: Uri): Boolean {
    check(!(getDirUri() == null && getDirFile() == null)) { "Both dirUri and dirFile are nulls!" }

    if (getDirUri() == null) {
      return false
    }

    return getDirUri() == dirPath
  }

  fun isBaseDir(dirPath: File): Boolean {
    check(!(getDirUri() == null && getDirFile() == null)) { "Both dirUri and dirFile are nulls!" }

    if (getDirFile() == null) {
      return false
    }

    return getDirFile() == dirPath
  }

  fun isBaseDir(dirPath: String): Boolean {
    check(!(getDirUri() == null && getDirFile() == null)) { "Both dirUri and dirFile are nulls!" }

    if (getDirFile() == null) {
      return false
    }

    return getDirFile()!!.absolutePath == dirPath
  }

  fun isBaseDir(dir: AbstractFile): Boolean {
    check(!(getDirUri() == null && getDirFile() == null)) { "Both dirUri and dirFile are nulls!" }

    if (dir is ExternalFile) {
      if (getDirUri() == null) {
        return false
      }

      return dir.getFileRoot<CachingDocumentFile>().holder.uri() == getDirUri()
    } else if (dir is RawFile) {
      if (getDirFile() == null) {
        return false
      }

      return dir.getFileRoot<File>().holder.absolutePath == getDirFile()?.absolutePath
    }

    throw IllegalStateException("${dir.javaClass.name} is not supported!")
  }

  fun dirPath(): String {
    check(!(getDirUri() == null && getDirFile() == null)) { "Both dirUri and dirFile are nulls!" }

    if (getDirUri() != null) {
      return getDirUri().toString()
    }

    val dirFile = checkNotNull(getDirFile()) {
      "dirPath() both dirUri and dirFile are not set!"
    }

    return dirFile.absolutePath
  }

  /**
   * This should return an Uri to the SAF directory.
   *
   * If both [getDirUri] and [getDirFile] return null then methods like
   * [FileManager.newBaseDirectoryFile] will throw an exception!
   * */
  abstract fun getDirUri(): Uri?

  /**
   * This one should return a fallback java file backed directory.
   * */
  abstract fun getDirFile(): File?

  /**
   * In some cases we want both dirUri and dirFile be non null and we also want to somehow
   * tell apart which one is currently in use by the user.
   *
   * You may provide to the user support for both the SAF base directories and java file
   * api ones and the user may change between them so to figure out which one they currently
   * use this flag exists.
   *
   * Also, when both are not null and you provide an ability to copy files from one base dir to
   * another after the user changes them you may want to copy the files from an old base dir to the
   * new one and the user may actually accidentally select the same directory for both base dir
   * types and to avoid copying to the same directory where they already exist we need to somehow
   * figure out whether the two [AbstractFile]s point to the same directory/file. This flag is also
   * useful for such cases.
   * */
  abstract fun currentActiveBaseDirType(): ActiveBaseDirType

  enum class ActiveBaseDirType {
    SafBaseDir,
    JavaFileBaseDir
  }

  companion object {
    const val TAG = "BaseDirectory"
  }
}
