package com.github.k1rakishou.fsaf_test_app.tests

import android.content.Context
import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.DirectorySegment
import com.github.k1rakishou.fsaf.file.FileSegment
import java.io.DataInputStream
import java.io.DataOutputStream
import kotlin.system.measureTimeMillis

class SnapshotTest(
  private val context: Context,
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
    val snapshotFileManager = fileManager.createSnapshot(baseDir, true)
    val testString = "test string"

    snapshotFileManager.create(baseDir, listOf(DirectorySegment("123")))!!.also { innerDir1 ->
      snapshotFileManager.create(innerDir1, listOf(FileSegment("test.txt")))!!.also { file ->
        check(snapshotFileManager.canRead(file)) { "cannot read ${file.getFullPath()}" }
        check(snapshotFileManager.canWrite(file)) { "cannot write to ${file.getFullPath()}" }
        check(snapshotFileManager.isFile(file)) { "file ${file.getFullPath()} is not a file" }
        check(!snapshotFileManager.isDirectory(file)) { "file ${file.getFullPath()} is a directory" }

        snapshotFileManager.getOutputStream(file)!!.use { stream ->
          DataOutputStream(stream).use { dos ->
            dos.writeUTF(testString)
          }
        }

        snapshotFileManager.getInputStream(file)!!.use { stream ->
          DataInputStream(stream).use { dis ->
            val actual = dis.readUTF()

            check(actual == testString) { "Expected to read ${testString} but actual is $actual" }
          }
        }

        check(snapshotFileManager.delete(file)) { "Couldn't delete ${file.getFullPath()}" }
        check(!snapshotFileManager.exists(file)) { "file ${file.getFullPath()} still exists" }
      }

      check(snapshotFileManager.delete(innerDir1)) { "Couldn't delete ${innerDir1.getFullPath()}" }
      check(!snapshotFileManager.exists(innerDir1)) { "directory ${innerDir1} still exists" }
    }

    check(fileManager.exists(baseDir)) { "BaseDir was deleted during test!" }
  }

  private fun test1(fileManager: FileManager, baseDir: AbstractFile) {
    val dir = fileManager.createDir(
      baseDir,
      "test"
    )

    if (dir == null || !fileManager.exists(dir) || !fileManager.isDirectory(dir)) {
      throw TestException("Couldn't create directory")
    }

    createFiles(fileManager, dir)

    val snapshotFileManager = fileManager.createSnapshot(dir, true)
    val innerDir1 = snapshotFileManager.listFiles(dir).firstOrNull()
      ?: throw TestException("Failed to find inner_dir1")
    val innerDir2 = snapshotFileManager.listFiles(innerDir1).firstOrNull()
      ?: throw TestException("Failed to find inner_dir2")

    val files = snapshotFileManager.listFiles(innerDir2)
    val tests = 5

    for (i in 0 until tests) {
      val time = measureTimeMillis {
        val fileNames = files.map { file -> fileManager.getName(file) }.toSet()

        for ((index, file) in files.withIndex()) {
          val expectedName = "${index}.txt"

          if (expectedName !in fileNames) {
            throw TestException("[Iteration $i] File name ${expectedName} does not exist in fileNames")
          }

          if (!fileManager.exists(file)) {
            throw TestException("[Iteration $i] File ${file.getFullPath()} does not exist")
          }

          if (!fileManager.isFile(file)) {
            throw TestException("[Iteration $i] File ${file.getFullPath()} is not a file")
          }

          fileManager.getLength(file)
          fileManager.lastModified(file)

          if (!fileManager.canRead(file)) {
            throw TestException("[Iteration $i] Cannot read ${file.getFullPath()}")
          }

          if (!fileManager.canWrite(file)) {
            throw TestException("[Iteration $i] Cannot write to ${file.getFullPath()}")
          }
        }
      }

      log("withSnapshot test ${i} out of $tests, time = ${time}ms")
    }
  }

  private fun createFiles(
    fileManager: FileManager,
    dir: AbstractFile
  ) {
    val count = 25

    val innerDir1 = fileManager.createDir(dir, "inner_dir1")
      ?: throw TestException("Failed to create innerDir1")
    val innerDir2 = fileManager.createDir(innerDir1, "innerDir2")
      ?: throw TestException("Failed to create innerDir2")

    for (i in 0 until count) {
      val fileName = "${i}.txt"

      val createdFile = fileManager.createFile(
        innerDir2,
        fileName
      )

      if (createdFile == null
        || !fileManager.exists(createdFile)
        || !fileManager.isFile(createdFile)
      ) {
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