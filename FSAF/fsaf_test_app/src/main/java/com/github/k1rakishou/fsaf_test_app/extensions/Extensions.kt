package com.github.k1rakishou.fsaf_test_app.extensions

import android.content.ContentResolver
import java.util.regex.Pattern

internal const val CONTENT_TYPE = "${ContentResolver.SCHEME_CONTENT}://"
internal const val FILE_TYPE = "${ContentResolver.SCHEME_FILE}://"
internal val uriTypes = arrayOf(CONTENT_TYPE, FILE_TYPE)

internal const val ENCODED_SEPARATOR = "%2F"
internal const val FILE_SEPARATOR1 = "/"
internal const val FILE_SEPARATOR2 = "\\"

// Either "%2F" or "/" or "\"
private val SPLIT_PATTERN = Pattern.compile("%2F|/|\\\\")

internal fun String.splitIntoSegments(): List<String> {
  if (this.isEmpty()) {
    return emptyList()
  }

  val uriType = uriTypes.firstOrNull { type -> this.startsWith(type) }
  val string = if (uriType != null) {
    this.substring(uriType.length, this.length)
  } else {
    this
  }

  return if (string.contains(FILE_SEPARATOR1)
    || string.contains(FILE_SEPARATOR2)
    || string.contains(ENCODED_SEPARATOR)
  ) {
    val split = string
      .split(SPLIT_PATTERN)
      .filter { name -> name.isNotBlank() }

    split
  } else {
    listOf(string)
  }
}