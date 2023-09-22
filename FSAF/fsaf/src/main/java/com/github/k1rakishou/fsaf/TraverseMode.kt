package com.github.k1rakishou.fsaf

enum class TraverseMode {
  OnlyFiles,
  OnlyDirs,
  Both;

  fun includeDirs(): Boolean = this == OnlyDirs || this == Both
  fun includeFiles(): Boolean = this == OnlyFiles || this == Both
}