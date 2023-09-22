package com.github.k1rakishou.fsaf.util

import com.github.k1rakishou.fsaf.BadPathSymbolResolutionStrategy
import junit.framework.Assert.assertEquals
import org.junit.Test

class FSAFUtilsTest {

  @Test
  fun `test replace bad symbols without bad symbols`() {
    val result = FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
      BadPathSymbolResolutionStrategy.ReplaceBadSymbols,
      "123"
    )

    assertEquals("123", result)
  }

  @Test
  fun `test throw when there are bad symbols without bad symbols`() {
    val result = FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
      BadPathSymbolResolutionStrategy.ThrowAnException,
      "123"
    )

    assertEquals("123", result)
  }

  @Test
  fun `test ignore when there are bad symbols without bad symbols`() {
    val result = FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
      BadPathSymbolResolutionStrategy.Ignore,
      "123"
    )

    assertEquals("123", result)
  }

  @Test
  fun `test replace bad symbols with bad symbols`() {
    val result = FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
      BadPathSymbolResolutionStrategy.ReplaceBadSymbols,
      "1 2 3 .txt"
    )

    assertEquals("1_2_3_.txt", result)
  }

  @Test(expected = IllegalArgumentException::class)
  fun `test throw when there are bad symbols with bad symbols`() {
    FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
      BadPathSymbolResolutionStrategy.ThrowAnException,
      "1 2 3 .txt"
    )
  }

  @Test
  fun `test ignore bad symbols with bad symbols`() {
    val result = FSAFUtils.checkBadSymbolsAndApplyResolutionStrategy(
      BadPathSymbolResolutionStrategy.Ignore,
      "1 2 3 .txt"
    )

    assertEquals("1 2 3 .txt", result)
  }

}