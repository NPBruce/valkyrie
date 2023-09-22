package com.github.k1rakishou.fsaf.document_file

import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import android.provider.DocumentsContract
import android.text.TextUtils
import androidx.documentfile.provider.DocumentFile

class SnapshotDocumentFile(
  appContext: Context,
  delegate: DocumentFile,
  private val fileName: String?,
  private val fileMimeType: String?,
  private val fileFlags: Int,
  private val fileLastModified: Long,
  private val fileLength: Long
) : CachingDocumentFile(appContext, delegate) {

  override fun exists(): Boolean = true

  override fun isFile(): Boolean {
    return !(DocumentsContract.Document.MIME_TYPE_DIR == fileMimeType
      || TextUtils.isEmpty(fileMimeType))
  }

  override fun isDirectory(): Boolean {
    return DocumentsContract.Document.MIME_TYPE_DIR == fileMimeType
  }

  override fun isVirtual(): Boolean {
    if (Build.VERSION.SDK_INT < Build.VERSION_CODES.N) {
      return false
    }

    return fileFlags and DocumentsContract.Document.FLAG_VIRTUAL_DOCUMENT != 0
  }

  override fun canRead(): Boolean {
    val result = appContext.checkCallingOrSelfUriPermission(
      uri(),
      Intent.FLAG_GRANT_READ_URI_PERMISSION
    )

    if (result != PackageManager.PERMISSION_GRANTED) {
      return false
    }

    return !TextUtils.isEmpty(fileMimeType)
  }

  override fun canWrite(): Boolean {
    val result = appContext.checkCallingOrSelfUriPermission(
      uri(),
      Intent.FLAG_GRANT_READ_URI_PERMISSION
    )

    if (result != PackageManager.PERMISSION_GRANTED) {
      return false
    }

    if (TextUtils.isEmpty(fileMimeType)) {
      return false
    }

    // Documents with DELETE flag are considered writable
    if (fileFlags and DocumentsContract.Document.FLAG_SUPPORTS_DELETE != 0) {
      return true
    }

    if (DocumentsContract.Document.MIME_TYPE_DIR == fileMimeType
      && (fileFlags and DocumentsContract.Document.FLAG_DIR_SUPPORTS_CREATE != 0)
    ) {
      return true
    } else if (!TextUtils.isEmpty(fileMimeType)
      && (fileFlags and DocumentsContract.Document.FLAG_SUPPORTS_WRITE != 0)
    ) {
      return true
    }

    return false
  }

  override fun name(): String? = fileName

  override fun length(): Long {
    check(!isDirectory()) { "Cannot get the length of a directory" }
    return fileLength
  }

  override fun lastModified(): Long = fileLastModified

}