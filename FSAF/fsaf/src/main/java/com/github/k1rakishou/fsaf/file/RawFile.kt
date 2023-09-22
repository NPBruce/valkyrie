package com.github.k1rakishou.fsaf.file

import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import com.github.k1rakishou.fsaf.extensions.appendMany
import java.io.File

class RawFile(
  root: Root<File>,
  badPathSymbolResolutionStrategy: BadPathSymbolResolutionStrategy,
  segments: List<Segment> = listOf()
) : AbstractFile(badPathSymbolResolutionStrategy, root, segments) {

  override fun cloneInternal(newSegments: List<Segment>): RawFile = RawFile(
    root.clone() as Root<File>,
    badSymbolResolutionStrategy,
    segments.toMutableList().apply { addAll(newSegments) }
  )

  override fun getFullPath(): String {
    val oldRootFilePath = (root.holder as File).absolutePath

    return File(oldRootFilePath)
      .appendMany(segments.map { segment -> segment.name })
      .absolutePath
  }

  override fun getFileManagerId(): FileManagerId = FILE_MANAGER_ID

  companion object {
    private const val TAG = "RawFile"
    val FILE_MANAGER_ID = FileManagerId(TAG)
  }
}