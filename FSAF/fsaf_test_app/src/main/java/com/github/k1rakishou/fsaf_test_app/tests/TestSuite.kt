package com.github.k1rakishou.fsaf_test_app.tests

import android.content.Context
import android.net.Uri
import com.github.k1rakishou.fsaf.FileManager
import com.github.k1rakishou.fsaf.file.AbstractFile
import com.github.k1rakishou.fsaf.file.ExternalFile
import com.github.k1rakishou.fsaf_test_app.TestBaseDirectory
import kotlin.system.measureTimeMillis

class TestSuite(
  private val fileManager: FileManager,
  private val context: Context
) {
  private val TAG = "TestSuite"

  fun runTests(baseDirSAF: AbstractFile, baseDirFile: AbstractFile) {
    try {
      println("$TAG =============== START TESTS ===============")
      println("$TAG baseDirSAF = ${baseDirSAF.getFullPath()}")
      println("$TAG baseDirFile = ${baseDirFile.getFullPath()}")

      check(fileManager.baseDirectoryExists<TestBaseDirectory>()) {
        "Base directory does not exist!"
      }

      check(fileManager.exists(baseDirSAF)) {
        "Base directory does not exist! path = ${baseDirSAF.getFullPath()}"
      }
      check(fileManager.exists(baseDirFile)) {
        "Base directory does not exist! path = ${baseDirFile.getFullPath()}"
      }

      runTestsWithSAFFiles(fileManager, baseDirSAF, baseDirFile)
      runTestsWithJavaFiles(fileManager, baseDirSAF, baseDirFile)

      check(fileManager.deleteContent(baseDirFile)) { "deleteContent baseDirFile returned false" }
      check(fileManager.deleteContent(baseDirSAF)) { "deleteContent baseDirSAF returned false" }

      if (baseDirSAF is ExternalFile) {
        val baseDirUri = Uri.parse(baseDirSAF.getFullPath())
        val dir = checkNotNull(fileManager.fromUri(baseDirUri)) { "fileManager.fromUri(${baseDirUri}) failure" }

        check(fileManager.exists(dir)) { "Does not exist" }
        check(fileManager.isDirectory(dir)) { "Not a dir" }
      }

      println("$TAG =============== END TESTS ===============")
    } catch (error: Throwable) {
      println("$TAG =============== ERROR ===============")
      throw error
    }
  }

  private fun runTestsWithSAFFiles(
    fileManager: FileManager,
    baseDirSAF: AbstractFile,
    baseDirFile: AbstractFile
  ) {
    val time = measureTimeMillis {
      SimpleTest("$TAG(SAF) SimpleTest").runTests(fileManager, baseDirSAF)
      CreateFilesTest("$TAG(SAF) CreateFilesTest").runTests(fileManager, baseDirSAF)
      SnapshotTest(context, "$TAG(SAF) SnapshotTest").runTests(fileManager, baseDirSAF)
      DeleteTest("$TAG(SAF) DeleteTest").runTests(fileManager, baseDirSAF)
      CopyTest("$TAG(SAF) CopyTest").runTests(
        fileManager,
        baseDirSAF,
        baseDirFile,
        CopyTestType.FromSafDirToRegularDir
      )
      FindTests("$TAG(SAF) FindTests").runTests(fileManager, baseDirSAF)
      BadPathSymbolResolutionTest(context.applicationContext, "$TAG(SAF) BadPathSymbolResolutionTest").runTests(baseDirSAF)
    }

    println("$TAG runTestsWithSAFFiles took ${time}ms")
  }

  private fun runTestsWithJavaFiles(
    fileManager: FileManager,
    baseDirSAF: AbstractFile,
    baseDirFile: AbstractFile
  ) {
    val time = measureTimeMillis {
      SimpleTest("$TAG(Java) SimpleTest").runTests(fileManager, baseDirFile)
      CreateFilesTest("$TAG(Java) CreateFilesTest").runTests(fileManager, baseDirFile)
      SnapshotTest(context, "$TAG(Java) SnapshotTest").runTests(fileManager, baseDirFile)
      DeleteTest("$TAG(Java) DeleteTest").runTests(fileManager, baseDirFile)
      CopyTest("$TAG(Java) CopyTest").runTests(
        fileManager,
        baseDirFile,
        baseDirSAF,
        CopyTestType.FromRegularDitToSafDir
      )
      FindTests("$TAG(Java) FindTests").runTests(fileManager, baseDirFile)
      BadPathSymbolResolutionTest(context.applicationContext, "$TAG(Java) BadPathSymbolResolutionTest").runTests(baseDirSAF)
    }

    println("$TAG runTestsWithJavaFiles took ${time}ms")
  }
}