package com.github.k1rakishou.fsaf_test_app.tests

import android.content.Context
import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile

class BadPathSymbolResolutionTest(
  private val appContext: Context,
  tag: String
) : BaseTest(tag) {

  fun runTests(baseDir: AbstractFile) {
    val fileManager = FileManager(
      appContext = appContext,
      badPathSymbolResolutionStrategy = BadPathSymbolResolutionStrategy.Ignore
    )

    runTest(fileManager, baseDir) {
      val dirName = "dir (1)"

      val createdDir = fileManager.createDir(baseDir, dirName)
      if (createdDir == null || !fileManager.exists(createdDir) || !fileManager.isDirectory(createdDir)) {
        throw TestException("Couldn't create directory")
      }

      val resultDirName = fileManager.getName(createdDir)
      if (resultDirName != dirName) {
        throw TestException("Bad dir name. Expected: ${dirName} got ${resultDirName}")
      }

      val filesCount = 10

      repeat(filesCount) { index ->
        val fileName = "test ${index} 2 3.txt"

        val file = fileManager.createFile(createdDir, fileName)
        if (file == null || !fileManager.exists(file) || !fileManager.isFile(file)) {
          throw TestException("Couldn't create file")
        }

        val resultFileName = fileManager.getName(file)
        if (resultFileName != fileName) {
          throw TestException("Bad file name. Expected: ${fileName} got ${resultFileName}")
        }
      }

      val files = fileManager.listFiles(createdDir)
      if (files.size != filesCount) {
        throw TestException("Expected ${filesCount} got ${files.size}")
      }

      repeat(filesCount) { index ->
        val fileName = "test ${index} 2 3.txt"
        val actualName = fileManager.getName(files[index])

        if (fileName != actualName) {
          throw TestException("Bad file name. Expected: ${fileName} got ${actualName}")
        }
      }
    }
  }

}