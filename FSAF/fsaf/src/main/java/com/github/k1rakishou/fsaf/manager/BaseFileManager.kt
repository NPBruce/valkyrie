package com.github.k1rakishou.fsaf.manager

import android.os.ParcelFileDescriptor
import androidx.annotation.CheckResult
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.FileDescriptorMode
import com.github.k1rakishou.fsaf.file.Segment
import java.io.FileDescriptor
import java.io.InputStream
import java.io.OutputStream

interface BaseFileManager {

  fun create(baseDir: AbstractFile): AbstractFile?

  /**
   * Creates a new file that consists of the root directory and segments (sub dirs or the file name)
   * Behaves similarly to Java's mkdirs() method but work not only with directories but files as well.
   * */
  fun create(baseDir: AbstractFile, segments: List<Segment>): AbstractFile?

  /**
   * Check whether a file or a directory exists
   * */
  fun exists(file: AbstractFile): Boolean

  /**
   * Check whether this AbstractFile is actually a file on the disk
   * */
  fun isFile(file: AbstractFile): Boolean

  /**
   * Check whether this AbstractFile is actually a directory
   * */
  fun isDirectory(file: AbstractFile): Boolean

  /**
   * Check whether it is possible to read from this file (or whether a directory has a read
   * permission)
   * */
  fun canRead(file: AbstractFile): Boolean

  /**
   * Check whether it is possible to write into this file (or whether a directory has a write
   * permission)
   * */
  fun canWrite(file: AbstractFile): Boolean

  /**
   * Converts full file/directory path into a list of path segments ("/1/2/3" -> ["1", "2", "3"])
   * */
  fun getSegmentNames(file: AbstractFile): List<String>

  /**
   * Deletes this file or directory
   * */
  fun delete(file: AbstractFile): Boolean

  /**
   * Deletes contents of this directory. Does nothing if [dir] is not actually a directory.
   * */
  fun deleteContent(dir: AbstractFile): Boolean

  /**
   * Returns an input stream created from this file
   * */
  fun getInputStream(file: AbstractFile): InputStream?

  /**
   * Returns an output stream created from this file
   * */
  fun getOutputStream(file: AbstractFile): OutputStream?

  /**
   * Returns a name of this file or directory
   * */
  fun getName(file: AbstractFile): String?

  /**
   * AbstractFile may contain multiple segments internally (e.g. a file that is deep inside the
   * directory structure). When using a file with multiple segments with such operators like
   * [getLength], [canRead], [canWrite], [getName] etc, a [findFile] is applied to every intermediate
   * segment sequentially. This slows the speed down by a lot. To avoid the speed slow downs you should
   * get rid of those segments by using [flattenSegments]. It will attempt to find a file
   * inside the directory structure and return that file back. The returned file will not have any
   * segments so all of the aforementioned operators will be executed way faster because [findFile]
   * won't have to be used for every intermediate segment anymore. If the file does not exist on the
   * disk null will be returned.
   * */
  fun flattenSegments(file: AbstractFile): AbstractFile?

  /**
   * Searches for a file with name [fileName] inside this directory
   * */
  fun findFile(dir: AbstractFile, fileName: String): AbstractFile?

  /**
   * Returns the length of this file
   * */
  fun getLength(file: AbstractFile): Long

  /**
   * Returns a list of all files and directories inside this directory
   * */
  fun listFiles(dir: AbstractFile): List<AbstractFile>

  /**
   * Returns lastModified parameters of this file or directory
   * */
  fun lastModified(file: AbstractFile): Long

  @CheckResult
  fun getParcelFileDescriptor(
    file: AbstractFile,
    fileDescriptorMode: FileDescriptorMode
  ): ParcelFileDescriptor?

  /**
   * Useful method to safely work this this file's fileDescriptor (it is automatically closed upon
   * exiting the callback)
   * */
  fun <T> withFileDescriptor(
    file: AbstractFile,
    fileDescriptorMode: FileDescriptorMode,
    func: (FileDescriptor) -> T?
  ): T?
}
