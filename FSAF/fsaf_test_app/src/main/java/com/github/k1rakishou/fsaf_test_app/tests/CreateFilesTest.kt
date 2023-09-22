package com.github.k1rakishou.fsaf_test_app.tests

import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.DirectorySegment
import com.github.k1rakishou.fsaf.file.FileSegment
import java.io.DataInputStream
import java.io.DataOutputStream
import kotlin.system.measureTimeMillis

class CreateFilesTest(
  tag: String
) : BaseTest(tag) {

  fun runTests(fileManager: FileManager, baseDir: AbstractFile) {
    runTest(fileManager, baseDir) {
      val time = measureTimeMillis {
        testCreateBunchOfFilesEachAtATime(fileManager, baseDir)
      }

      log("testCreateBunchOfFilesEachAtATime took ${time}ms")
    }

    runTest(fileManager, baseDir) {
      testCreateFileWithTheSameNameShouldNotCreateNewFile(fileManager, baseDir)
    }
  }

  private fun testCreateFileWithTheSameNameShouldNotCreateNewFile(
    fileManager: FileManager,
    baseDir: AbstractFile
  ) {
    val dir1 = fileManager.createDir(baseDir, "test")
    if (dir1 == null || !fileManager.exists(dir1) || !fileManager.isDirectory(dir1)) {
      throw TestException("Couldn't create directory")
    }

    if (fileManager.create(dir1) == null) {
      throw TestException("Couldn't create already existing directory")
    }

    val dir2 = fileManager.createDir(baseDir, "test")
    if (dir2 == null || !fileManager.exists(dir2) || !fileManager.isDirectory(dir2)) {
      throw TestException("Couldn't create directory")
    }

    if (fileManager.create(dir2) == null) {
      throw TestException("Couldn't create already existing directory")
    }

    val totalFiles = fileManager.listFiles(baseDir).size
    if (totalFiles != 1) {
      throw TestException("New file was created when it shouldn't have been, totalFiles = $totalFiles")
    }
  }

  private fun testCreateBunchOfFilesEachAtATime(fileManager: FileManager, baseDir: AbstractFile) {
    val dir = fileManager.createDir(
      baseDir,
      "test"
    )

    if (dir == null || !fileManager.exists(dir) || !fileManager.isDirectory(dir)) {
      throw TestException("Couldn't create directory")
    }

    val files = 25

    for (i in 0 until files) {
      val fileName = "${i}.txt"

      val createdFile = fileManager.create(
        dir,
        DirectorySegment(i.toString()),
        FileSegment(fileName)
      )

      if (createdFile == null || !fileManager.exists(createdFile) || !fileManager.isFile(createdFile)) {
        throw TestException("Couldn't create file name")
      }

      if (fileManager.getName(createdFile) != fileName) {
        throw TestException("Bad name ${fileManager.getName(createdFile)}")
      }

      fileManager.getOutputStream(createdFile)?.use { os ->
        DataOutputStream(os).use { dos ->
          dos.writeUTF(fileName)
        }
      } ?: throw TestException("Couldn't get output stream, file = ${createdFile.getFullPath()}")

      fileManager.getInputStream(createdFile)?.use { `is` ->
        DataInputStream(`is`).use { dis ->
          val readString = dis.readUTF()

          if (readString != fileName) {
            throw TestException("Wrong value read, expected = ${fileName}, actual = ${readString}")
          }
        }
      } ?: throw TestException("Couldn't get input stream, file = ${createdFile.getFullPath()}")
    }
  }

}