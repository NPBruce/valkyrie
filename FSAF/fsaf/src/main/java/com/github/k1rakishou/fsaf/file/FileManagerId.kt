package com.github.k1rakishou.fsaf.file

class FileManagerId(val id: String) {

  override fun equals(other: Any?): Boolean {
    if (this === other) return true
    if (javaClass != other?.javaClass) return false

    other as FileManagerId

    if (id != other.id) return false

    return true
  }

  override fun hashCode(): Int {
    return id.hashCode()
  }

  override fun toString(): String {
    return "FileManagerId(id='$id')"
  }

}