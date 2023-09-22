package com.github.k1rakishou.fsaf

import com.github.k1rakishou.fsaf.util.FSAFUtils

enum class BadPathSymbolResolutionStrategy {
  /**
   * Good for debug/dev builds. Whenever there is an attempt to create/clone a file/directory
   * with bad characters in their name an IllegalArgumentException will be thrown right away.
   * */
  ThrowAnException,

  /**
   * Good for release builds. Instead of crashing upon encountering a bad symbol it will be
   * replaced with a corresponding symbols from the replacement array. This is the default
   * behavior.
   * see [FSAFUtils.BAD_SYMBOLS_REPLACEMENTS]
   * */
  ReplaceBadSymbols,

  /**
   * Bad symbols will be ignored. This may cause crashes and other weird behavior.
   * */
  Ignore
}