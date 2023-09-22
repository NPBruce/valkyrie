package com.github.k1rakishou.fsaf.document_file

import android.content.Context
import android.net.Uri
import androidx.documentfile.provider.DocumentFile

open class CachingDocumentFile(
  protected val appContext: Context,
  val delegate: DocumentFile
) {
  private var cachedExists: Boolean? = null
  private var cachedIsFile: Boolean? = null
  private var cachedIsDirectory: Boolean? = null
  private var cachedIsVirtual: Boolean? = null
  private var cachedCanRead: Boolean? = null
  private var cachedCanWrite: Boolean? = null
  private var cachedName: String? = null
  private var cachedLen: Long? = null
  private var cachedLastModified: Long? = null

  @Synchronized
  open fun exists(): Boolean {
    if (cachedExists != null) {
      return cachedExists!!
    }

    cachedExists = delegate.exists()
    return cachedExists!!
  }

  @Synchronized
  open fun isFile(): Boolean {
    if (cachedIsFile != null) {
      return cachedIsFile!!
    }

    cachedIsFile = delegate.isFile
    return cachedIsFile!!
  }

  @Synchronized
  open fun isDirectory(): Boolean {
    if (cachedIsDirectory != null) {
      return cachedIsDirectory!!
    }

    cachedIsDirectory = delegate.isDirectory
    return cachedIsDirectory!!
  }

  @Synchronized
  open fun isVirtual(): Boolean {
    if (cachedIsVirtual != null) {
      return cachedIsVirtual!!
    }

    cachedIsVirtual = delegate.isVirtual
    return cachedIsVirtual!!
  }

  @Synchronized
  open fun canRead(): Boolean {
    if (cachedCanRead != null) {
      return cachedCanRead!!
    }

    cachedCanRead = delegate.canRead()
    return cachedCanRead!!
  }

  @Synchronized
  open fun canWrite(): Boolean {
    if (cachedCanWrite != null) {
      return cachedCanWrite!!
    }

    cachedCanWrite = delegate.canWrite()
    return cachedCanWrite!!
  }

  @Synchronized
  open fun name(): String? {
    if (cachedName != null) {
      return cachedName!!
    }

    cachedName = delegate.name
    return cachedName
  }

  @Synchronized
  open fun length(): Long {
    if (cachedLen != null) {
      return cachedLen!!
    }

    cachedLen = delegate.length()
    return cachedLen!!
  }

  @Synchronized
  open fun lastModified(): Long {
    if (cachedLastModified != null) {
      return cachedLastModified!!
    }

    cachedLastModified = delegate.lastModified()
    return cachedLastModified!!
  }

  open fun uri(): Uri {
    return delegate.uri
  }

  fun delete(): Boolean {
    return delegate.delete()
  }
}