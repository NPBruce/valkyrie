package com.github.k1rakishou.fsaf.manager

import android.content.Context
import android.os.ParcelFileDescriptor
import android.provider.DocumentsContract
import android.util.Log
import android.webkit.MimeTypeMap
import androidx.documentfile.provider.DocumentFile
import android.annotation.SuppressLint
import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import com.github.k1rakishou.fsaf.document_file.CachingDocumentFile
import com.github.k1rakishou.fsaf.extensions.getMimeFromFilename
import com.github.k1rakishou.fsaf.extensions.splitIntoSegments
import com.github.k1rakishou.fsaf.file.*
import com.github.k1rakishou.fsaf.manager.base_directory.DirectoryManager
import com.github.k1rakishou.fsaf.util.SAFHelper
import java.io.FileDescriptor
import java.io.InputStream
import java.io.OutputStream

/**
 * Provide an API to work with SAF files
 * */
open class ExternalFileManager(
  protected val appContext: Context,
  protected val badPathSymbolResolutionStrategy: BadPathSymbolResolutionStrategy,
  private val directoryManager: DirectoryManager
) : BaseFileManager {
  private val mimeTypeMap = MimeTypeMap.getSingleton()

  override fun create(baseDir: AbstractFile): AbstractFile? {
    return create(baseDir, baseDir.getFileSegments())
  }

  @Suppress("UNCHECKED_CAST")
  @SuppressLint("NewApi")
  override fun create(baseDir: AbstractFile, segments: List<Segment>): ExternalFile? {
    val root = baseDir.getFileRoot<CachingDocumentFile>()
    check(root !is Root.FileRoot) {
      "create() root is already FileRoot, cannot append anything anymore, " +
        "root = ${root.holder.uri()}, " +
        "baseDir segments = ${baseDir.getFileSegments().joinToString()}, " +
        "segments = ${segments.joinToString()}"
    }

    if (segments.isEmpty()) {
      if (exists(baseDir)) {
        return baseDir as ExternalFile
      }

      throw IllegalStateException("create() Segments are empty and " +
        "baseDir (${baseDir.getFullPath()}) does not exist")
    }

    var newFile: CachingDocumentFile? = null

    for (segment in segments) {
      val innerFile = newFile ?: root.holder

      val mimeType = if (segment.isFileName) {
        mimeTypeMap.getMimeFromFilename(segment.name)
      } else {
        DocumentsContract.Document.MIME_TYPE_DIR
      }

      // Check whether this segment already exists on the disk
      val foundFile = SAFHelper.findCachingFile(
        appContext,
        innerFile.uri(),
        segment.name,
        directoryManager.isBaseDir(innerFile)
      )

      if (foundFile != null) {
        if (foundFile.isFile()) {
          // Ignore any left segments (which we shouldn't have) after encountering fileName
          // segment
          return ExternalFile(
            appContext,
            badPathSymbolResolutionStrategy,
            Root.FileRoot(foundFile, segment.name)
          )
        } else {
          newFile = foundFile
        }

        continue
      }

      val newUri = DocumentsContract.createDocument(
        appContext.contentResolver,
        innerFile.uri(),
        mimeType,
        segment.name
      )

      if (newUri == null) {
        Log.e(
          TAG, "create() DocumentsContract.createDocument returned null, " +
            "file.uri = ${innerFile.uri()}, segment.name = ${segment.name}"
        )
        return null
      }

      val createdFile = if (segment.isFileName) {
        DocumentFile.fromSingleUri(appContext, newUri)
      } else {
        DocumentFile.fromTreeUri(appContext, newUri)
      }

      if (createdFile == null) {
        Log.e(TAG, "create() Couldn't create DocumentFile out of uri, directoryUri = ${newUri}")
        return null
      }

      if (segment.isFileName) {
        // Ignore any left segments (which we shouldn't have) after encountering fileName
        // segment

        val newRoot = Root.FileRoot(
          CachingDocumentFile(appContext, createdFile),
          segment.name
        )

        return ExternalFile(
          appContext,
          badPathSymbolResolutionStrategy,
          newRoot
        )
      } else {
        newFile = CachingDocumentFile(appContext, createdFile)
      }
    }

    if (newFile == null) {
      Log.e(TAG, "create() result file is null")
      return null
    }

    val lastSegment = segments.last()
    val isLastSegmentFilename = lastSegment.isFileName

    val newRoot = if (isLastSegmentFilename) {
      Root.FileRoot(newFile, lastSegment.name)
    } else {
      Root.DirRoot(newFile)
    }

    return ExternalFile(
      appContext,
      badPathSymbolResolutionStrategy,
      newRoot
    )
  }

  override fun exists(file: AbstractFile): Boolean =
    toDocumentFile(file.clone())?.exists() ?: false

  override fun isFile(file: AbstractFile): Boolean =
    toDocumentFile(file.clone())?.isFile() ?: false

  override fun isDirectory(file: AbstractFile): Boolean =
    toDocumentFile(file.clone())?.isDirectory() ?: false

  override fun canRead(file: AbstractFile): Boolean =
    toDocumentFile(file.clone())?.canRead() ?: false

  override fun canWrite(file: AbstractFile): Boolean =
    toDocumentFile(file.clone())?.canWrite() ?: false

  override fun getSegmentNames(file: AbstractFile): List<String> {
    return file.getFullPath().splitIntoSegments()
  }

  override fun delete(file: AbstractFile): Boolean {
    if (!exists(file)) {
      return true
    }

    val documentFile = toDocumentFile(file.clone())
      ?: return true

    return documentFile.delete()
  }

  override fun deleteContent(dir: AbstractFile): Boolean {
    val documentFile = toDocumentFile(dir.clone())
      ?: return false

    if (!documentFile.isDirectory()) {
      Log.e(TAG, "deleteContent() Only directories are supported (files can't have contents anyway)")
      return false
    }

    val filesInDirectory = SAFHelper.listFilesFast(
      appContext,
      documentFile.uri(),
      directoryManager.isBaseDir(documentFile)
    )

    // This may delete only some files and leave other but at least you will know that something
    // went wrong (just don't forget to check the result)
    return filesInDirectory.all { file -> file.delete() }
  }

  override fun getInputStream(file: AbstractFile): InputStream? {
    val documentFile = toDocumentFile(file.clone())
    if (documentFile == null) {
      Log.e(TAG, "getInputStream() toDocumentFile() returned null")
      return null
    }

    if (!documentFile.exists()) {
      Log.e(TAG, "getInputStream() documentFile does not exist, uri = ${documentFile.uri()}")
      return null
    }

    if (!documentFile.isFile()) {
      Log.e(TAG, "getInputStream() documentFile is not a file, uri = ${documentFile.uri()}")
      return null
    }

    if (!documentFile.canRead()) {
      Log.e(TAG, "getInputStream() cannot read from documentFile, uri = ${documentFile.uri()}")
      return null
    }

    val contentResolver = appContext.contentResolver
    return contentResolver.openInputStream(documentFile.uri())
  }

  override fun getOutputStream(file: AbstractFile): OutputStream? {
    val documentFile = toDocumentFile(file.clone())
    if (documentFile == null) {
      Log.e(TAG, "getOutputStream() toDocumentFile() returned null")
      return null
    }

    if (!documentFile.exists()) {
      Log.e(TAG, "getOutputStream() documentFile does not exist, uri = ${documentFile.uri()}")
      return null
    }

    if (!documentFile.isFile()) {
      Log.e(TAG, "getOutputStream() documentFile is not a file, uri = ${documentFile.uri()}")
      return null
    }

    if (!documentFile.canWrite()) {
      Log.e(TAG, "getOutputStream() cannot write to documentFile, uri = ${documentFile.uri()}")
      return null
    }

    val contentResolver = appContext.contentResolver
    return contentResolver.openOutputStream(documentFile.uri())
  }

  override fun getName(file: AbstractFile): String? {
    val segments = file.getFileSegments()
    if (segments.isNotEmpty() && segments.last().isFileName) {
      return segments.last().name
    }

    val documentFile = toDocumentFile(file.clone())
      ?: throw IllegalStateException("getName() toDocumentFile() returned null")

    return documentFile.name()
  }

  override fun flattenSegments(file: AbstractFile): AbstractFile? {
    val segments = file.getFileSegments()
    if (segments.isEmpty()) {
      // File already has no segments, nothing to do
      return file
    }

    val parentUri = file.getFileRoot<CachingDocumentFile>().holder.uri()

    val snapshotDocumentFile = SAFHelper.findDeepFile(
      appContext,
      parentUri,
      segments,
      directoryManager
    ) as? CachingDocumentFile

    if (snapshotDocumentFile == null) {
      return null
    }

    val innerRoot = if (snapshotDocumentFile.isFile()) {
      Root.FileRoot(snapshotDocumentFile, snapshotDocumentFile.name()!!)
    } else {
      Root.DirRoot(snapshotDocumentFile)
    }

    return ExternalFile(
      appContext,
      badPathSymbolResolutionStrategy,
      innerRoot
    )
  }

  override fun findFile(dir: AbstractFile, fileName: String): ExternalFile? {
    val root = dir.getFileRoot<CachingDocumentFile>()
    val segments = dir.getFileSegments()

    check(root !is Root.FileRoot) { "findFile() Cannot use FileRoot as directory" }
    if (segments.isNotEmpty()) {
      check(!segments.last().isFileName) { "findFile() Cannot do search when last segment is file" }
    }

    val parentDocFile = if (segments.isNotEmpty()) {
      SAFHelper.findDeepFile(
        appContext,
        root.holder.uri(),
        segments,
        directoryManager
      )
    } else {
      val docFile = DocumentFile.fromSingleUri(appContext, root.holder.uri())
      if (docFile != null) {
        CachingDocumentFile(appContext, docFile)
      } else {
        null
      }
    }

    if (parentDocFile == null) {
      return null
    }

    val cachingDocFile = SAFHelper.findCachingFile(
      appContext,
      parentDocFile.uri(),
      fileName,
      directoryManager.isBaseDir(parentDocFile)
    )

    if (cachingDocFile == null || !cachingDocFile.exists()) {
      return null
    }

    val innerRoot = if (cachingDocFile.isFile()) {
      Root.FileRoot(cachingDocFile, cachingDocFile.name()!!)
    } else {
      Root.DirRoot(cachingDocFile)
    }

    return ExternalFile(
      appContext,
      badPathSymbolResolutionStrategy,
      innerRoot
    )
  }

  override fun getLength(file: AbstractFile): Long = toDocumentFile(file.clone())?.length() ?: 0L

  override fun listFiles(dir: AbstractFile): List<ExternalFile> {
    Log.d(TAG, "In EFM listFiles")
    val root = dir.getFileRoot<CachingDocumentFile>()
    check(root !is Root.FileRoot) { "listFiles() Cannot use listFiles with FileRoot" }
    

    val docFile = toDocumentFile(dir.clone())
      ?: return emptyList()

    Log.d(TAG, "EFM listFiles: $appContext ${docFile.uri()} ${directoryManager.isBaseDir(dir)}")
    
    return SAFHelper.listFilesFast(appContext, docFile.uri(), directoryManager.isBaseDir(dir))
      .map { snapshotFile ->
        return@map ExternalFile(
          appContext,
          badPathSymbolResolutionStrategy,
          Root.DirRoot(snapshotFile)
        )
      }
  }

  override fun lastModified(file: AbstractFile): Long {
    return toDocumentFile(file.clone())?.lastModified() ?: 0L
  }

  override fun getParcelFileDescriptor(
    file: AbstractFile,
    fileDescriptorMode: FileDescriptorMode
  ): ParcelFileDescriptor? {
    return appContext.contentResolver.openFileDescriptor(
      file.getFileRoot<CachingDocumentFile>().holder.uri(),
      fileDescriptorMode.mode
    )
  }

  override fun <T> withFileDescriptor(
    file: AbstractFile,
    fileDescriptorMode: FileDescriptorMode,
    func: (FileDescriptor) -> T?
  ): T? {
    if (isDirectory(file)) {
      Log.e(TAG, "withFileDescriptor() only works with files ")
      return null
    }

    return getParcelFileDescriptor(file, fileDescriptorMode)
      ?.use { pfd -> func(pfd.fileDescriptor) }
      ?: throw IllegalStateException(
        "withFileDescriptor() Could not get ParcelFileDescriptor " +
          "from root with uri = ${file.getFileRoot<DocumentFile>().holder.uri}"
      )
  }

  private fun toDocumentFile(file: AbstractFile): CachingDocumentFile? {
    val segments = file.getFileSegments()
    if (segments.isEmpty()) {
      return file.getFileRoot<CachingDocumentFile>().holder
    }

    val parentUri = file.getFileRoot<CachingDocumentFile>()
      .holder
      .uri()

    return SAFHelper.findDeepFile(
      appContext,
      parentUri,
      segments,
      directoryManager
    )
  }

  companion object {
    private const val TAG = "ExternalFileManager"
  }
}