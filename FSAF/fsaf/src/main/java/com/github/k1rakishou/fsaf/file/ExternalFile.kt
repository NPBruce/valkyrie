package com.github.k1rakishou.fsaf.file

import android.content.Context
import android.net.Uri
import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import com.github.k1rakishou.fsaf.document_file.CachingDocumentFile
import com.github.k1rakishou.fsaf.util.FSAFUtils

class ExternalFile(
  private val appContext: Context,
  badPathSymbolResolutionStrategy: BadPathSymbolResolutionStrategy,
  root: Root<CachingDocumentFile>,
  segments: List<Segment> = listOf()
) : AbstractFile(badPathSymbolResolutionStrategy, root, segments) {

  @Suppress("UNCHECKED_CAST")
  override fun getFullPath(): String {
    return getUri().toString()
  }

  @Suppress("UNCHECKED_CAST")
  fun getUri(): Uri {
    val oldRootUri = (root as Root<CachingDocumentFile>).holder.uri()

    if (segments.isEmpty()) {
      return oldRootUri
    }

    val oldRootUriString = oldRootUri.toString()

    val resultPath = buildString {
      append(oldRootUriString)

      // For some reason, when using Uri.Builder().appendPath() or Uri.Builder().appendEncodedPath()
      // (doesn't matter), the builder uses '/' symbols to separate segments instead of '%2F'. This
      // is bad because sometimes such path cannot be used in DocumentFile.fromSingleUri() because
      // all operation on that document file will return null or false. But when using a uri with
      // '%2F' segment separator everything works! So here we have to manually append segments using
      // '%2F' separator so that it can be used further by other functions such as FileManager.fromUri()
      segments.forEach { segment ->
        append(FSAFUtils.ENCODED_SEPARATOR)
        append(segment.name)
      }
    }

    return Uri.parse(resultPath)
  }

  @Suppress("UNCHECKED_CAST")
  override fun cloneInternal(newSegments: List<Segment>): ExternalFile = ExternalFile(
    appContext,
    badSymbolResolutionStrategy,
    (root as Root<CachingDocumentFile>).clone(),
    segments.toMutableList().apply { addAll(newSegments) }
  )

  override fun getFileManagerId(): FileManagerId = FILE_MANAGER_ID

  companion object {
    private const val TAG = "ExternalFile"
    val FILE_MANAGER_ID = FileManagerId(TAG)
  }
}