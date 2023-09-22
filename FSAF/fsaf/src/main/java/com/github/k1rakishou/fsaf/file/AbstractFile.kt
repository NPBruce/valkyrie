package com.github.k1rakishou.fsaf.file

import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import com.github.k1rakishou.fsaf.extensions.extension
import com.github.k1rakishou.fsaf.extensions.splitIntoSegments
import com.github.k1rakishou.fsaf.util.FSAFUtils

/**
 * An abstraction class over both the Java File and the Storage Access Framework DocumentFile.
 * */
abstract class AbstractFile(
  protected val badSymbolResolutionStrategy: BadPathSymbolResolutionStrategy,
  protected val root: Root<*>,
  protected val segments: List<Segment>
) : HasFileManagerId {

  /**
   * Returns not-yet-applied to the Root segments
   * */
  fun getFileSegments(): List<Segment> {
    return segments
  }

  /**
   * Returns the current root of this file
   * */
  fun <T> getFileRoot(): Root<T> {
    return root as Root<T>
  }

  /**
   * Returns full file path whether it's a DocumentFile (in this case the uri string is returned)
   * or a regular File
   * */
  abstract fun getFullPath(): String

  /**
   * Takes a path string that may look like this: "123/456/" or like this "/123/456/" or like this
   * "123/456" or like this "123/456/test.txt" and splits it into a list of [Segment]s
   * If the last segment of [path] has an extension assumes it as a [FileSegment], if it doesn't then
   * assumes it as a [DirectorySegment]. This function is unsafe. Only use it if you are certain in
   * your input. Otherwise use other versions of [clone]. This method exists only because
   * sometimes it's really tedious to create segments by hand.
   * */
  fun cloneUnsafe(path: String): AbstractFile {
    val segmentStrings = path.splitIntoSegments()

    val segments = segmentStrings.mapIndexed { index, segmentString ->
      if (index == segmentStrings.lastIndex && segmentString.extension() != null) {
        return@mapIndexed FileSegment(segmentString)
      }

      return@mapIndexed DirectorySegment(segmentString)
    }

    return clone(segments)
  }

  /**
   * Clones the file and appends new segment
   * */
  fun clone(newSegment: Segment): AbstractFile {
    return clone(listOf(newSegment))
  }

  /**
   * Clones the file and appends new segments (newSegments may be empty)
   * */
  fun clone(vararg newSegments: Segment): AbstractFile {
    return clone(newSegments.toList())
  }

  /**
   * Clones the file and appends new segments (newSegments may be empty)
   * */
  fun clone(newSegments: List<Segment>): AbstractFile {
    if (newSegments.isNotEmpty()) {
      newSegments.forEach { segment ->
        require(segment.name.isNotBlank()) { "Bad name: ${segment.name}" }
      }

      check(!isFilenameAppended()) { "Cannot append anything after file name has been appended" }
    }

    return cloneInternal(
      FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(badSymbolResolutionStrategy, newSegments)
    )
  }

  /**
   * Clones the current abstract file and appends new segments to it (if there are any).
   * Basically you should use this method when you want to point to some file or
   * directory somewhere inside the directory structure. This method does not create the file on the
   * disk it only points to it (it may or may not already exist on the disk). Work exactly the same
   * as the regular java File where you first set the file path and then do some operation on it
   * (like exists() or createNew() or mkdir() etc.)
   *
   * [newSegments] parameter is a list of path segments that will be appended to the file after
   * cloning. May be empty.
   * */
  protected abstract fun cloneInternal(newSegments: List<Segment>): AbstractFile

  private fun isFilenameAppended(): Boolean =
    segments.lastOrNull()?.isFileName ?: false

  override fun equals(other: Any?): Boolean {
    if (other == null) {
      return false
    }

    if (other === this) {
      return true
    }

    if (other !is AbstractFile) {
      return false
    }

    // Two AbstractFiles can be the same only if they are of the same child class.
    // This also doesn't check where the two AbstractFiles point to the same file or directory
    return when {
      this is RawFile && other is RawFile -> {
        other.getFullPath() == this.getFullPath()
      }
      this is ExternalFile && other is ExternalFile -> {
        other.getFullPath() == this.getFullPath()
      }
      else -> false
    }
  }

  override fun hashCode(): Int {
    return getFullPath().hashCode()
  }

  override fun toString(): String {
    return getFullPath().splitIntoSegments().joinToString(separator = "/")
  }
}