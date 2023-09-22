package com.github.k1rakishou.fsaf_test_app.tests

import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.DirectorySegment
import com.github.k1rakishou.fsaf.file.FileSegment
import kotlin.system.measureTimeMillis

class FindTests(
  tag: String
) : BaseTest(tag) {

  fun runTests(fileManager: FileManager, baseDir: AbstractFile) {
    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        test1(fileManager, baseDir)
      }

      log("test1 took ${time}ms")
    }

    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        test2(fileManager, baseDir)
      }

      log("test2 took ${time}ms")
    }

    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        test3(fileManager, baseDir)
      }

      log("test3 took ${time}ms")
    }
  }

  private fun test3(fileManager: FileManager, baseDir: AbstractFile) {
    val clonedBaseDir = baseDir.cloneUnsafe("/1/2/3/4/5/6/7/8/")
    val result = fileManager.flattenSegments(clonedBaseDir)
    if (result != null) {
      throw IllegalStateException("Expected null but got ${result.getFullPath()}")
    }
  }

  private fun test2(fileManager: FileManager, baseDir: AbstractFile) {
    checkNotNull(
      fileManager.create(
        baseDir,
        DirectorySegment("1"),
        DirectorySegment("2"),
        DirectorySegment("3"),
        DirectorySegment("4"),
        DirectorySegment("5"),
        DirectorySegment("6"),
        DirectorySegment("7"),
        DirectorySegment("8"),
        FileSegment("test123.txt")
      )
    ) { "Couldn't create \"/1/2/3/4/5/6/7/8/test123.txt\"" }

    val clonedBaseDir = baseDir.cloneUnsafe("/1/2/3/4/5/6/7/8/")
    val flattenedDir = checkNotNull(fileManager.flattenSegments(clonedBaseDir)) {
      "Couldn't flatten segments for ${clonedBaseDir.getFullPath()}"
    }

    fileManager.findFile(flattenedDir, "test123.txt").let { test123 ->
      checkNotNull(test123) { "Couldn't find file test123" }

      check(fileManager.exists(test123)) { "test123.txt does not exist" }
      check(fileManager.isFile(test123)) { "test123.txt is not a file" }
      check(!fileManager.isDirectory(test123)) { "test123.txt is a directory" }
      check(fileManager.getLength(test123) == 0L) { "test123.txt is not empty" }
    }
  }

  private fun test1(fileManager: FileManager, baseDir: AbstractFile) {
    checkNotNull(
      fileManager.create(
        baseDir,
        DirectorySegment("1"),
        DirectorySegment("2"),
        DirectorySegment("3"),
        DirectorySegment("4"),
        DirectorySegment("5"),
        DirectorySegment("6"),
        DirectorySegment("7"),
        DirectorySegment("8"),
        FileSegment("test123.txt")
      )
    ) { "Couldn't create \"/1/2/3/4/5/6/7/8/test123.txt\"" }

    fileManager.findFile(baseDir, "1").let { dir1 ->
      checkNotNull(dir1) { "Couldn't find dir 1" }

      fileManager.findFile(dir1, "2").let { dir2 ->
        checkNotNull(dir2) { "Couldn't find dir 2" }

        fileManager.findFile(dir2, "3").let { dir3 ->
          checkNotNull(dir3) { "Couldn't find dir 3" }

          fileManager.findFile(dir3, "4").let { dir4 ->
            checkNotNull(dir4) { "Couldn't find dir 4" }

            fileManager.findFile(dir4, "5").let { dir5 ->
              checkNotNull(dir5) { "Couldn't find dir 5" }

              fileManager.findFile(dir5, "6").let { dir6 ->
                checkNotNull(dir6) { "Couldn't find dir 6" }

                fileManager.findFile(dir6, "7").let { dir7 ->
                  checkNotNull(dir7) { "Couldn't find dir 7" }

                  fileManager.findFile(dir7, "8").let { dir8 ->
                    checkNotNull(dir8) { "Couldn't find dir 8" }

                    fileManager.findFile(dir8, "test123.txt").let { test123 ->
                      checkNotNull(test123) { "Couldn't find file test123" }

                      check(fileManager.exists(test123)) { "test123.txt does not exist" }
                      check(fileManager.isFile(test123)) { "test123.txt is not a file" }
                      check(!fileManager.isDirectory(test123)) { "test123.txt is a directory" }
                      check(fileManager.getLength(test123) == 0L) { "test123.txt is not empty" }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}