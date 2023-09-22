package com.github.k1rakishou.fsaf_test_app.tests

import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.TraverseMode
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.DirectorySegment
import com.github.k1rakishou.fsaf.file.FileSegment
import com.github.k1rakishou.fsaf_test_app.TestBaseDirectory
import com.github.k1rakishou.fsaf_test_app.extensions.splitIntoSegments
import kotlin.system.measureTimeMillis

class SimpleTest(
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

    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        basicFileTests(fileManager, baseDir)
      }

      log("basicFileTests took ${time}ms")
    }

    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        badFileNamesTest(fileManager, baseDir)
      }

      log("badFileNamesTest took ${time}ms")
    }

    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        listFilesOrderingTest(fileManager, baseDir)
      }

      log("listFilesOrderingTest took ${time}ms")
    }
  }

  private fun listFilesOrderingTest(fileManager: FileManager, baseDir: AbstractFile) {
    fileManager.create(baseDir, DirectorySegment("1"))?.also { dir1 ->
      checkNotNull(fileManager.create(dir1, FileSegment("1.txt"))) { "Couldn't create 1.txt" }
      checkNotNull(fileManager.create(dir1, FileSegment("156.txt"))) { "Couldn't create 156.txt" }
      checkNotNull(fileManager.create(dir1, FileSegment("10.txt"))) { "Couldn't create 10.txt" }

      fileManager.create(dir1, DirectorySegment("234"))?.also { dir2 ->
        checkNotNull(fileManager.create(dir2, FileSegment("2.txt"))) { "Couldn't create 2.txt" }
        checkNotNull(fileManager.create(dir2, FileSegment("256.txt"))) { "Couldn't create 256.txt" }
        checkNotNull(fileManager.create(dir2, FileSegment("20.txt"))) { "Couldn't create 20.txt" }

        fileManager.create(dir2, DirectorySegment("333444"))?.also { dir3 ->
          checkNotNull(fileManager.create(dir3, FileSegment("2_1_3.txt"))) { "Couldn't create 2_1_3.txt" }

          fileManager.create(dir3, DirectorySegment("555666"))?.also { dir4 ->
            checkNotNull(fileManager.create(dir4, FileSegment("innermost.txt"))) { "Couldn't create innermost.txt" }
          } ?: throw IllegalStateException("Couldn't create dir 555666")
        } ?: throw IllegalStateException("Couldn't create dir 333444")
      } ?: throw IllegalStateException("Couldn't create dir 234")

      fileManager.create(dir1, DirectorySegment("4562"))?.also { dir2 ->
        checkNotNull(fileManager.create(dir2, FileSegment("2.txt"))) { "Couldn't create 2.txt" }
        checkNotNull(fileManager.create(dir2, FileSegment("256.txt"))) { "Couldn't create 256.txt" }
        checkNotNull(fileManager.create(dir2, FileSegment("20.txt"))) { "Couldn't create 20.txt" }
      } ?: throw IllegalStateException("Couldn't create dir 456")

      fileManager.create(dir1, DirectorySegment("78"))?.also { dir2 ->
        checkNotNull(fileManager.create(dir2, FileSegment("2.txt"))) { "Couldn't create 2.txt" }
        checkNotNull(fileManager.create(dir2, FileSegment("256.txt"))) { "Couldn't create 256.txt" }
        checkNotNull(fileManager.create(dir2, FileSegment("20.txt"))) { "Couldn't create 20.txt" }
      } ?: throw IllegalStateException("Couldn't create dir 789")
    } ?: throw IllegalStateException("Couldn't create dir 1")

    val files = mutableListOf<AbstractFile>()
    fileManager.traverseDirectory(baseDir, true, TraverseMode.Both) { file -> files += file }

    val checkedDirectories = mutableSetOf<String>()

    files.forEach { file ->
      if (fileManager.isDirectory(file)) {
        checkedDirectories += file.getFullPath().splitIntoSegments().joinToString(separator = "/")
      } else {
        val path = file.getFullPath().splitIntoSegments().dropLast(1).joinToString(separator = "/")
        if (path !in checkedDirectories) {
          throw IllegalStateException("File was added before it's parent directory")
        }
      }
    }
  }

  private fun badFileNamesTest(fileManager: FileManager, baseDir: AbstractFile) {
    val externalFile = fileManager.create(
      baseDir,
      DirectorySegment("12 3")
    )

    if (externalFile == null || !fileManager.exists(externalFile)) {
      throw TestException("Couldn't create 12_3 directory")
    }

    check(fileManager.getName(externalFile) == "12_3") {
      "bad directory name after replacing bad symbols: ${fileManager.getName(externalFile)}"
    }

    val externalFile2 = fileManager.create(externalFile, FileSegment("123 4 5 .txt"))
    if (externalFile2 == null || !fileManager.exists(externalFile2)) {
      throw TestException("Couldn't create 123_4_5_.txt file")
    }

    check(fileManager.getName(externalFile2) == "123_4_5_.txt") {
      "bad directory name after replacing bad symbols: ${fileManager.getName(externalFile2)}"
    }

    val externalFile3 = fileManager.create(
      externalFile,
      DirectorySegment("test.dir"),
      FileSegment("Kuroba-dev v4.10.2-a9551c9.apk")
    )

    if (externalFile3 == null || !fileManager.exists(externalFile3)) {
      throw TestException("Couldn't create Kuroba-dev v4.10.2-a9551c9.apk file")
    }

    check(fileManager.getName(externalFile3) == "Kuroba-dev_v4.10.2-a9551c9.apk") {
      "bad directory name after replacing bad symbols: ${fileManager.getName(externalFile3)}"
    }

    val fileWithMultiplePeriods = fileManager.create(
      externalFile,
      DirectorySegment("test....dir"),
      FileSegment("t.e.s.t......txt")
    )

    if (fileWithMultiplePeriods == null || !fileManager.exists(fileWithMultiplePeriods)) {
      throw TestException("Couldn't create t.e.s.t......txt file")
    }

    check(fileManager.getName(fileWithMultiplePeriods) == "t.e.s.t......txt") {
      "bad directory name after replacing bad symbols: ${fileManager.getName(fileWithMultiplePeriods)}"
    }
  }

  private fun test1(fileManager: FileManager, baseDir: AbstractFile) {
    val externalFile = fileManager.create(
      baseDir,
      DirectorySegment("123"),
      DirectorySegment("456"),
      DirectorySegment("789"),
      FileSegment("test123.txt")
    )

    if (externalFile == null || !fileManager.exists(externalFile)) {
      throw TestException("Couldn't create test123.txt")
    }

    if (!fileManager.isFile(externalFile)) {
      throw TestException("test123.txt is not a file")
    }

    if (fileManager.isDirectory(externalFile)) {
      throw TestException("test123.txt is a directory")
    }

    if (fileManager.getName(externalFile) != "test123.txt") {
      throw TestException("externalFile name != test123.txt")
    }

    val externalFile2Exists = baseDir.clone(
      DirectorySegment("123"),
      DirectorySegment("456"),
      DirectorySegment("789")
    )

    if (!fileManager.exists(externalFile2Exists)) {
      throw TestException("789 directory does not exist")
    }

    val dirToDelete = baseDir.clone(
      DirectorySegment("123")
    )

    if (!fileManager.delete(dirToDelete) && fileManager.exists(dirToDelete)) {
      throw TestException("Couldn't delete test123.txt")
    }

    checkDirEmpty(fileManager, baseDir)
  }

  private fun test2(fileManager: FileManager, baseDir: AbstractFile) {
    val externalFile = fileManager.create(
      baseDir,
      DirectorySegment("1234"),
      DirectorySegment("4566"),
      FileSegment("filename.json")
    )

    if (externalFile == null || !fileManager.exists(externalFile)) {
      throw TestException("Couldn't create filename.json")
    }

    if (!fileManager.isFile(externalFile)) {
      throw TestException("filename.json is not a file")
    }

    if (fileManager.isDirectory(externalFile)) {
      throw TestException("filename.json is not a directory")
    }

    if (fileManager.getName(externalFile) != "filename.json") {
      throw TestException("externalFile1 name != filename.json")
    }

    val dir = baseDir.clone(
      DirectorySegment("1234"),
      DirectorySegment("4566")
    )

    if (fileManager.getName(dir) != "4566") {
      throw TestException("dir.name != 4566, name = " + fileManager.getName(dir))
    }

    val foundFile = fileManager.findFile(dir, "filename.json")
    if (foundFile == null || !fileManager.exists(foundFile)) {
      throw TestException("Couldn't find filename.json")
    }
  }

  private fun test3(fileManager: FileManager, baseDir: AbstractFile) {
    val externalFile = fileManager.create(
      baseDir,
      DirectorySegment("123"),
      DirectorySegment("456"),
      DirectorySegment("789"),
      FileSegment("test123.txt")
    )

    if (externalFile == null || !fileManager.exists(externalFile)) {
      throw TestException("Couldn't create test123.txt")
    }

    for (i in 0 until 1000) {
      if (!fileManager.exists(externalFile)) {
        throw TestException("Does not exist")
      }

      if (!fileManager.isFile(externalFile)) {
        throw TestException("Not a file")
      }

      if (fileManager.isDirectory(externalFile)) {
        throw TestException("Is a directory")
      }
    }
  }

  private fun basicFileTests(fileManager: FileManager, baseDir: AbstractFile) {
    val createdDir1 =  run {
      val createdDir1 = fileManager.createDir(baseDir, "1")
        ?: throw TestException("Couldn't create dir 1")
      val createdDir2 = fileManager.createDir(createdDir1, "2")
        ?: throw TestException("Couldn't create dir 2")
      val createdDir3 = fileManager.createDir(createdDir2, "3")
        ?: throw TestException("Couldn't create dir 3")
      fileManager.createFile(createdDir3, "file.txt")
        ?: throw TestException("Couldn't create file.txt")

      return@run createdDir1
    }

    check(fileManager.isDirectory(createdDir1)) { "Dir 1 is not a dir" }
    check(!fileManager.isFile(createdDir1)) { "Dir 1 is a file" }
    check(fileManager.exists(createdDir1)) { "Dir 1 does not exist" }
    check(fileManager.getName(createdDir1) == "1") { "Dir 1 has wrong name" }
    check(fileManager.canRead(createdDir1)) { "Cannot read dir 1" }
    check(fileManager.canWrite(createdDir1)) { "Cannot write to dir 1" }

    val createdDir2 = checkNotNull(fileManager.findFile(createdDir1, "2")) { "Couldn't find dir 2" }
    check(fileManager.isDirectory(createdDir2)) { "Dir 2 is not a dir" }
    check(!fileManager.isFile(createdDir2)) { "Dir 2 is a file" }
    check(fileManager.exists(createdDir2)) { "Dir 2 does not exist" }
    check(fileManager.getName(createdDir2) == "2") { "Dir 2 has wrong name" }
    check(fileManager.canRead(createdDir2)) { "Cannot read dir 2" }
    check(fileManager.canWrite(createdDir2)) { "Cannot write to dir 2" }

    val createdDir3 = checkNotNull(fileManager.findFile(createdDir2, "3")) { "Couldn't find dir 3" }
    check(fileManager.isDirectory(createdDir3)) { "Dir 3 is not a dir" }
    check(!fileManager.isFile(createdDir3)) { "Dir 3 is a file" }
    check(fileManager.exists(createdDir3)) { "Dir 3 does not exist" }
    check(fileManager.getName(createdDir3) == "3") { "Dir 3 has wrong name" }
    check(fileManager.canRead(createdDir3)) { "Cannot read dir 3" }
    check(fileManager.canWrite(createdDir3)) { "Cannot write to dir 3" }

    val createdFile = checkNotNull(fileManager.findFile(createdDir3, "file.txt")) { "Couldn't find file.txt" }
    check(!fileManager.isDirectory(createdFile)) { "file.txt is a dir" }
    check(fileManager.isFile(createdFile)) { "file.txt is not a file" }
    check(fileManager.exists(createdFile)) { "file.txt does not exist" }
    check(fileManager.getName(createdFile) == "file.txt") { "file.txt has wrong name" }
    check(fileManager.canRead(createdFile)) { "Cannot read file.txt" }
    check(fileManager.canWrite(createdFile)) { "Cannot write to file.txt" }

    val nonExistingDir = fileManager.newBaseDirectoryFile<TestBaseDirectory>()!!
      .clone(DirectorySegment("211314"))

    if (fileManager.exists(nonExistingDir)) {
      throw TestException("TestBaseDirectory exists when it shouldn't")
    }
  }

}