package com.github.k1rakishou.fsaf_test_app.tests

import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.TraverseMode
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.DirectorySegment
import java.io.DataInputStream
import java.io.DataOutputStream
import kotlin.system.measureTimeMillis

class CopyTest(
  tag: String
) : BaseTest(tag) {
  private val dirs = 5
  private val files = 25

  fun runTests(fileManager: FileManager, _scrDir: AbstractFile, _dstDir: AbstractFile, copyTest: CopyTestType) {
    runTest(fileManager, _scrDir) {
      val srcDir = fileManager.createDir(_scrDir, "src")
      if (srcDir == null || !fileManager.exists(srcDir) || !fileManager.isDirectory(srcDir)) {
        throw TestException("Couldn't create src directory")
      }

      val dstDir = fileManager.createDir(_dstDir, "dst")
      if (dstDir == null || !fileManager.exists(dstDir) || !fileManager.isDirectory(dstDir)) {
        throw TestException("Couldn't create dst directory")
      }

      val time = measureTimeMillis {
        copyTest(fileManager, srcDir, dstDir)
      }

      log("copyTest (${copyTest.text}) took ${time}ms")
    }
  }

  private fun copyTest(fileManager: FileManager, srcDir: AbstractFile, dstDir: AbstractFile) {
    createFiles(fileManager, srcDir)

    val copyResult = fileManager.copyDirectoryWithContent(srcDir, dstDir, true)
    if (!copyResult) {
      throw TestException("Couldn't copy file from one directory to another")
    }

    val destDirFiles = mutableListOf<AbstractFile>()

    fileManager.traverseDirectory(dstDir, true, TraverseMode.Both) { file ->
      destDirFiles.add(file)
    }

    val expectedFilesCount = dirs * (dirs + files)
    if (destDirFiles.size != expectedFilesCount) {
      throw TestException("Some files were not copied, " +
        "expectedSize = $expectedFilesCount, actual = ${destDirFiles.size}")
    }

    destDirFiles.forEach { file ->
      if (fileManager.isFile(file)) {
        fileManager.getInputStream(file)?.use { inputStream ->
          DataInputStream(inputStream).use { dis ->
            val expected = fileManager.getName(file)
            val actual = dis.readUTF()

            if (expected != actual) {
              throw TestException("Couldn't read the same value out of the file, " +
                "expected = ${expected}, actual = ${actual}")
            }
          }
        } ?: throw TestException("Couldn't open input stream for file ${file.getFullPath()}")
      }
    }
  }

  private fun createFiles(
    fileManager: FileManager,
    srcDir: AbstractFile
  ) {
    if (!fileManager.exists(srcDir) || !fileManager.isDirectory(srcDir)) {
      throw TestException("Couldn't create directory")
    }

    for (dir in 0 until dirs) {
      val dirName = dir.toString()

      val createdDir = fileManager.create(
        srcDir,
        DirectorySegment(dirName),
        DirectorySegment("$dirName$dirName"),
        DirectorySegment("$dirName$dirName$dirName"),
        DirectorySegment("$dirName$dirName$dirName$dirName"),
        DirectorySegment("$dirName$dirName$dirName$dirName$dirName")
      )

      if (createdDir == null
        || !fileManager.exists(createdDir)
        || !fileManager.isDirectory(createdDir)
      ) {
        throw TestException("Couldn't create directories")
      }

      for (file in 0 until files) {
        val fileName = "${file}.txt"

        val createdFile = fileManager.createFile(createdDir, fileName)
        if (createdFile == null
          || !fileManager.exists(createdFile)
          || !fileManager.isFile(createdFile)
        ) {
          throw TestException("Couldn't create file ${fileName}")
        }

        fileManager.getOutputStream(createdFile)?.use { outputStream ->
          DataOutputStream(outputStream).use { dos ->
            dos.writeUTF(fileName)
          }
        }
      }
    }
  }
}


enum class CopyTestType(val text: String) {
  FromSafDirToRegularDir("SAF dir to Regular dir"),
  FromRegularDitToSafDir("Regular dir to SAF dir")
}