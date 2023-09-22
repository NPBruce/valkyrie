package com.github.k1rakishou.fsaf_test_app.tests

import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile

abstract class BaseTest(
  private val tag: String
) {

  protected fun log(message: String) {
    println("$tag, $message")
  }

  protected fun checkDirEmpty(fileManager: FileManager, dir: AbstractFile) {
    val files = fileManager.listFiles(dir)
    if (files.isNotEmpty()) {
      throw TestException("Couldn't not delete some files in the base directory: ${files}")
    }
  }

  protected fun runTest(fileManager: FileManager, dir: AbstractFile, block: () -> Unit) {
    fileManager.deleteContent(dir)

    block()
  }

}