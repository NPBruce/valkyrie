package com.github.k1rakishou.fsaf

import com.github.k1rakishou.fsaf.util.FSAFUtils
import junit.framework.Assert.assertEquals
import junit.framework.Assert.assertTrue
import org.junit.Test

class UtilsTest {

  @Test
  fun `test merge paths some paths are not unique`() {
    val paths = listOf(
      "/123/456/7",
      "/123",
      "/123/456",
      "/0/1/2",
      "/0/1",
      "/0/",
      "/0/",
      "/123/456"
    )

    val result = FSAFUtils.mergePaths(paths)
    assertEquals(2, result.size)

    assertTrue("/123/456/7" in result)
    assertTrue("/0/1/2" in result)
  }

  @Test
  fun `test merge paths all paths are unique`() {
    val paths = listOf(
      "/1/2/3/4/5",
      "/2/3/4/5/6",
      "/3/4/5/6/7",
      "/4/5/6/7/8",
      "/5/6/7/8/9"
    )

    val result = FSAFUtils.mergePaths(paths)
    assertEquals(5, result.size)

    assertTrue(paths[0] in result)
    assertTrue(paths[1] in result)
    assertTrue(paths[2] in result)
    assertTrue(paths[3] in result)
    assertTrue(paths[4] in result)
  }

}