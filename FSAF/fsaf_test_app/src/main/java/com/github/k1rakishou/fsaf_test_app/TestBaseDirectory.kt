package com.github.k1rakishou.fsaf_test_app

import android.net.Uri
import com.github.k1rakishou.fsaf.manager.base_directory.BaseDirectory
import java.io.File

class TestBaseDirectory(
  private val getBaseDirUriFunc: () -> Uri?,
  private val getBaseDirFileFunc: () -> File?
) : BaseDirectory() {

  override fun getDirUri(): Uri? = getBaseDirUriFunc.invoke()
  override fun getDirFile(): File? = getBaseDirFileFunc.invoke()
  override fun currentActiveBaseDirType(): ActiveBaseDirType = ActiveBaseDirType.SafBaseDir
}