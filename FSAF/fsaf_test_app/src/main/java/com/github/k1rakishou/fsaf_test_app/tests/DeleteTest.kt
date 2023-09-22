package com.github.k1rakishou.fsaf_test_app.tests

import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.DirectorySegment
import com.github.k1rakishou.fsaf.file.FileSegment
import com.github.k1rakishou.fsaf_test_app.TestBaseDirectory
import kotlin.system.measureTimeMillis

class DeleteTest(
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
  }

  private fun test2(fileManager: FileManager, baseDir: AbstractFile) {
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/1.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/2.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/3.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/4.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/5.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/6.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/7.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/8.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/9.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/10.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/11.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/12.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/13.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/14.txt")) { "Create returned null" }
    checkNotNull(fileManager.createUnsafe(baseDir, "/123/456/789/15.txt")) { "Create returned null" }

    val outerDirectory = baseDir.cloneUnsafe("123")
    check(fileManager.delete(outerDirectory)) { "Couldn't delete 123 directory" }

    check(!fileManager.exists(baseDir.cloneUnsafe("123"))) { "123 still exists" }
    check(fileManager.findFile(baseDir, "123") == null) { "123 still exists" }
  }

  fun test1(fileManager: FileManager, baseDir: AbstractFile) {
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

    kotlin.run {
      val file = fileManager.newBaseDirectoryFile<TestBaseDirectory>()!!
        .cloneUnsafe("/123/456/678/test123.txt")

      check(fileManager.delete(file)) { "Couldn't delete test123.txt" }
      check(!fileManager.exists(file)) { "test123.txt still exists" }
    }

    kotlin.run {
      val file = fileManager.newBaseDirectoryFile<TestBaseDirectory>()!!
        .cloneUnsafe("/123/456/678")

      check(fileManager.delete(file)) { "Couldn't delete 678" }
      check(!fileManager.exists(file)) { "678 still exists" }
    }

    kotlin.run {
      val file = fileManager.newBaseDirectoryFile<TestBaseDirectory>()!!
        .cloneUnsafe("/123/456")

      check(fileManager.delete(file)) { "Couldn't delete 456" }
      check(!fileManager.exists(file)) { "456 still exists" }
    }

    kotlin.run {
      val file = fileManager.newBaseDirectoryFile<TestBaseDirectory>()!!
        .cloneUnsafe("/123")

      check(fileManager.delete(file)) { "Couldn't delete 123" }
      check(!fileManager.exists(file)) { "123 still exists" }
    }

    kotlin.run {
      val file = fileManager.newBaseDirectoryFile<TestBaseDirectory>()!!
        .cloneUnsafe("/123")

      check(fileManager.delete(file)) { "fileManager.delete even though file shouldn't exist" }
    }
  }
}