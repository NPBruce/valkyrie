package com.github.k1rakishou.fsaf

import com.github.k1rakishou.fsaf.extensions.splitIntoSegments
import junit.framework.Assert.assertEquals
import org.junit.Test

class ExtensionsTest {

  @Test
  fun testSplitIntoSegments() {
    val string = "content://com.android.externalstorage.documents/tree/0EF0-1C1F%3Atest/document/0EF0-1C1F%3Atest%2Ftest"

    /**
     * com.android.externalstorage.documents
     * tree
     * 0EF0-1C1F%3Atest
     * document
     * 0EF0-1C1F%3Atest
     * test
     * */

    val segments = string.splitIntoSegments()
    assertEquals(6, segments.size)

    assertEquals("com.android.externalstorage.documents", segments[0])
    assertEquals("tree", segments[1])
    assertEquals("0EF0-1C1F%3Atest", segments[2])
    assertEquals("document", segments[3])
    assertEquals("0EF0-1C1F%3Atest", segments[4])
    assertEquals("test", segments[5])
  }

  @Test
  fun testSplitBadSegments() {
    val string = "%2F0"

    val segments = string.splitIntoSegments()
    assertEquals(1, segments.size)

    assertEquals("0", segments.first())
  }

  @Test
  fun testSplitIntoSegments2() {
    kotlin.run {
      val path = "/123/456/"

      val segments = path.splitIntoSegments()
      assertEquals(2, segments.size)
    }

    kotlin.run {
      val path = "123/456/"

      val segments = path.splitIntoSegments()
      assertEquals(2, segments.size)
    }

    kotlin.run {
      val path = "123/456"

      val segments = path.splitIntoSegments()
      assertEquals(2, segments.size)
    }
  }
}