package com.github.k1rakishou.fsaf.file

/**
 * Root is a real path to the file (raw or external). It may be incomplete (in this case
 * [AbstractFile] will have non empty segments). When building a file path we first append new path
 * segments and when doing some file operation (like checking it's existence or creating it)
 * we append the segments to the Root.
 *
 * Root may be a directory or a file.
 * If it's a directory, that means that we can append sub directories to it.
 * If it's a file we can't do that so usually when attempting to append something to the FileRoot
 * an exception will be thrown
 *
 * [holder] either SAF DocumentFile or Java File.
 * */
sealed class Root<T>(val holder: T) {

  fun name(): String? {
    if (this is FileRoot) {
      return this.fileName
    }

    return null
  }

  fun clone(): Root<T> {
    return when (this) {
      is DirRoot<*> -> DirRoot(holder)
      is FileRoot<*> -> FileRoot(holder, fileName)
    }
  }

  /**
   * /test/123/test2
   * or
   * /test/123/test2/5/6/7/8/112233
   * */
  class DirRoot<T>(holder: T) : Root<T>(holder)

  /**
   * /test/123/test2/filename.txt
   * where holder = /test/123/test2/filename.txt,
   * fileName = filename.txt (may have no extension)
   * */
  class FileRoot<T>(holder: T, val fileName: String) : Root<T>(holder)
}