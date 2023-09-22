package com.github.k1rakishou.fsaf.file

/**
 * Segment represents a sub directory or a file name, e.g:
 * /test/123/test2/filename.txt
 *  ^   ^    ^     ^
 *  |   |    |     +--- File name segment (name = filename.txt, isFileName == true)
 *  +---+----+-- Directory segments (names = [test, 123, test2], isFileName == false)
 * */
abstract class Segment(
  val name: String,
  val isFileName: Boolean = false
) {
  override fun toString(): String {
    return "Segment(name='$name', isFileName=$isFileName)"
  }
}

class DirectorySegment(name: String) : Segment(name, false)
class FileSegment(name: String) : Segment(name, true)