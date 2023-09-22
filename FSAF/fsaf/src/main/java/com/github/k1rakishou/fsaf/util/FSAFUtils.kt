package com.github.k1rakishou.fsaf.util

import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import com.github.k1rakishou.fsaf.extensions.safeCapacity
import com.github.k1rakishou.fsaf.extensions.uriTypes
import com.github.k1rakishou.fsaf.file.DirectorySegment
import com.github.k1rakishou.fsaf.file.FileSegment
import com.github.k1rakishou.fsaf.file.Segment
import java.io.File
import java.util.regex.Pattern


object FSAFUtils {

  /**
   * Only spaces for now since SAF will crash if you try to pass a file or directory name with
   * spaces in it. Every symbol must be unique!
   * */
  private const val BAD_SYMBOLS = " "

  /**
   * Order of the symbols should be the same as in [BAD_SYMBOLS].
   * */
  private const val BAD_SYMBOLS_REPLACEMENTS = "_"

  /**
   * I've encountered at least 3 different types of separators
   * */
  internal const val ENCODED_SEPARATOR = "%2F"
  private const val FILE_SEPARATOR1 = "/"
  private const val FILE_SEPARATOR2 = "\\"
  private val SPLIT_PATTERN = Pattern.compile("%2F|/|\\\\")

  @JvmStatic
  fun splitIntoSegments(path: String): List<String> {
    if (path.isEmpty()) {
      return emptyList()
    }

    val uriType = uriTypes.firstOrNull { type -> path.startsWith(type) }
    val string = if (uriType != null) {
      path.substring(uriType.length, path.length)
    } else {
      path
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

  /**
   * Merges paths that are fully contained in other paths, e.g.:
   *
   * "/123" and "/123/456" and "/123/456/789" -> "/123/456/789"
   * or
   * "/123/456" and "/123/456" -> "/123/456"
   * or
   * "/123/456" and "/123" -> "/123/456"
   * */
  internal fun mergePaths(pathList: List<String>): List<String> {
    if (pathList.isEmpty()) {
      return emptyList()
    }

    val capacity = if (pathList.size <= 1) {
      1
    } else {
      pathList.size / 2
    }

    val processed = HashSet<Int>(capacity)
    val filtered = HashSet<Int>(capacity)

    for (i in pathList.indices) {
      for (j in pathList.indices) {
        if (i == j) {
          continue
        }

        if (i in processed || i in filtered || j in processed || j in filtered) {
          continue
        }

        val path1 = pathList[i]
        val path2 = pathList[j]

        if (path1.length > path2.length) {
          continue
        }

        if (path2.contains(path1)) {
          filtered += i
          processed += i
        }
      }
    }

    return pathList.filterIndexed { index, _ -> index !in filtered }
  }

  /**
   * Deletes all of the directory's contents and the directory itself if [deleteRootDir] is true
   * */
  internal fun deleteDirectory(directory: File, deleteRootDir: Boolean, depth: Int = 0): Boolean {
    if (!directory.isDirectory) {
      return false
    }

    val files = directory.listFiles()
    if (files != null) {
      for (f in files) {
        if (f.isDirectory) {
          if (!deleteDirectory(f, deleteRootDir, depth + 1)) {
            return false
          }
        } else {
          if (!f.delete()) {
            return false
          }
        }
      }
    }

    if (deleteRootDir || depth > 0) {
      return directory.delete()
    }

    return true
  }

  internal fun checkBadSymbolsAndApplyResolutionStrategy(
    badSymbolResolutionStrategy: BadPathSymbolResolutionStrategy,
    segments: List<Segment>
  ): List<Segment> {
    return segments.map { segment ->
      val resultSegment = checkBadSymbolsAndApplyResolutionStrategy(
        badSymbolResolutionStrategy,
        segment.name
      )

      return@map when (segment.isFileName) {
        true -> FileSegment(resultSegment)
        false -> DirectorySegment(resultSegment)
      }
    }
  }

  /**
   * Check whether an input string (that is either a file name or a directory name) has bad symbols
   * (see [BAD_SYMBOLS]) and if has then applies a bad symbol resolution strategy (Either filters
   * them or throws an exception). The default behavior is to replace all bad symbols with their
   * replacements (see [BAD_SYMBOLS_REPLACEMENTS])
   * */
  internal fun checkBadSymbolsAndApplyResolutionStrategy(
    badSymbolResolutionStrategy: BadPathSymbolResolutionStrategy,
    inputString: String
  ): String {
    if (badSymbolResolutionStrategy == BadPathSymbolResolutionStrategy.Ignore) {
      return inputString
    }

    val firstBadSymbol = returnFirstBadSymbolIndexOrMinusOne(inputString)
    if (firstBadSymbol == -1) {
      // No bad symbols
      return inputString
    }

    when (badSymbolResolutionStrategy) {
      BadPathSymbolResolutionStrategy.ReplaceBadSymbols -> {
        return replaceBadSymbols(firstBadSymbol, inputString)
      }
      BadPathSymbolResolutionStrategy.ThrowAnException -> {
        throw IllegalArgumentException("Bad symbols encountered at index ${firstBadSymbol}, " +
          "symbol = \'${inputString.getOrNull(firstBadSymbol)}\'")
      }
      else -> {
        throw NotImplementedError("Not implemented for ${badSymbolResolutionStrategy.name}")
      }
    }
  }

  /**
   * Check whether an input string has any characters from [BAD_SYMBOLS] and returns first
   * encountered bad symbols or null if there are no bad symbols
   * */
  private fun returnFirstBadSymbolIndexOrMinusOne(inputString: String): Int {
    if (inputString.isEmpty()) {
      return -1
    }

    return inputString.indexOfFirst { char -> char in BAD_SYMBOLS }
  }

  private fun replaceBadSymbols(startIndex: Int, inputString: String): String {
    if (inputString.isEmpty()) {
      return inputString
    }

    require(startIndex <= inputString.length) {
      "startIndex $startIndex is greater than the inputString length ${inputString.length}"
    }

    val resultStringBuilder = StringBuilder(safeCapacity(inputString))

    for (index in startIndex until inputString.length) {
      val ch = inputString[index]
      val badSymbolIndex = BAD_SYMBOLS.indexOf(ch)
      if (badSymbolIndex == -1) {
        resultStringBuilder.append(ch)
      } else {
        val replacement = checkNotNull(BAD_SYMBOLS_REPLACEMENTS.getOrNull(badSymbolIndex)) {
          "Couldn't find replacement for symbol \'$ch\' with index $badSymbolIndex"
        }

        resultStringBuilder.append(replacement)
      }
    }

    if (startIndex > 0) {
      resultStringBuilder.insert(0, inputString.substring(0, startIndex))
    }

    return resultStringBuilder.toString()
  }

}