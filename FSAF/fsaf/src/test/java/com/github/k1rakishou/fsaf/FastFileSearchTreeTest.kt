package com.github.k1rakishou.fsaf

import org.junit.Assert.*
import org.junit.Test

class FastFileSearchTreeTest {

  /**
   * 0/00/000.txt
   * 1/11/111.txt
   * 2/22/222.txt
   * 3/33/333.txt
   * 4/44/444.txt
   *
   * etc.
   * */
  private val balancedSegmentsList = listOf(
    listOf("0", "00", "000.txt"),
    listOf("1", "11", "111.txt"),
    listOf("2", "22", "222.txt"),
    listOf("3", "33", "333.txt"),
    listOf("4", "44", "444.txt"),
    listOf("5", "55", "555.txt"),
    listOf("6", "66", "666.txt"),
    listOf("7", "77", "777.txt"),
    listOf("8", "88", "888.txt"),
    listOf("9", "99", "999.txt")
  )

  /**
   * 0/00/000.txt
   * 0/00/111.txt
   * 0/00/222.txt
   * 0/00/333.txt
   * 0/00/444.txt
   *
   * etc.
   * */
  private val manySegmentsInOneParentSegment = listOf(
    listOf("0", "00", "000.txt"),
    listOf("0", "00", "111.txt"),
    listOf("0", "00", "222.txt"),
    listOf("0", "00", "333.txt"),
    listOf("0", "00", "444.txt"),
    listOf("0", "00", "555.txt"),
    listOf("0", "00", "666.txt"),
    listOf("0", "00", "777.txt"),
    listOf("0", "00", "888.txt"),
    listOf("0", "00", "999.txt")
  )

  private val valuesList = listOf(
    0,
    111,
    222,
    333,
    444,
    555,
    666,
    777,
    888,
    999
  )

  @Test
  fun `Empty FastFileSearchTree must only have Root node`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    assertEquals(FastFileSearchTreeNode.ROOT, fastFileSearchTree.root.getNodeName())
  }

  @Test
  fun `test insert many unique segments`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val pairs = balancedSegmentsList.zip(valuesList) { first, second -> Pair(first, second) }

    assertTrue(fastFileSearchTree.insertManySegments(pairs))
    assertEquals(10, fastFileSearchTree.root.getNodeChildren().size)

    var index = 0

    balancedSegmentsList.forEach { pathToVisit ->
      fastFileSearchTree.visitEverySegmentDuringPath(pathToVisit) { nodeIndex, node ->
        assertEquals(pathToVisit[nodeIndex], node.getNodeName())

        if (node.isLeaf()) {
          assertEquals(valuesList[index], node.getNodeValue())
          assertEquals(0, node.getNodeChildren().size)

          ++index
        } else {
          assertNull(node.getNodeValue())
          assertEquals(1, node.getNodeChildren().size)
        }
      }
    }

    val children = fastFileSearchTree.root.getNodeChildren()

    assertChildrenAreLeafs(children)
    nodeTypesCheck(fastFileSearchTree)
  }

  @Test
  fun `test many not unique segments`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val pairs = balancedSegmentsList.zip(valuesList) { first, second -> Pair(first, second) }

    assertTrue(fastFileSearchTree.insertManySegments(pairs))
    assertTrue(fastFileSearchTree.insertManySegments(pairs))
    assertEquals(10, fastFileSearchTree.root.getNodeChildren().size)

    var index = 0

    balancedSegmentsList.forEach { pathToVisit ->
      fastFileSearchTree.visitEverySegmentDuringPath(pathToVisit) { nodeIndex, node ->
        assertEquals(pathToVisit[nodeIndex], node.getNodeName())

        if (node.isLeaf()) {
          assertEquals(valuesList[index], node.getNodeValue())
          assertEquals(0, node.getNodeChildren().size)

          ++index
        } else {
          assertNull(node.getNodeValue())
          assertEquals(1, node.getNodeChildren().size)
        }
      }
    }

    val children = fastFileSearchTree.root.getNodeChildren()

    assertChildrenAreLeafs(children)
    nodeTypesCheck(fastFileSearchTree)
  }

  @Test
  fun `test many segments in one parent segment`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val pairs = manySegmentsInOneParentSegment.zip(valuesList) { first, second -> Pair(first, second) }

    assertTrue(fastFileSearchTree.insertManySegments(pairs))
    assertTrue(fastFileSearchTree.insertManySegments(pairs))

    val firstGenChildren = fastFileSearchTree.root.getNodeChildren()
    assertEquals(1, firstGenChildren.size)

    val secondGenChildren = firstGenChildren.map { child -> child.value.getNodeChildren() }.first()
    assertEquals(1, secondGenChildren.size)

    val thirdGenChildren = secondGenChildren.map { child -> child.value.getNodeChildren() }.first()
    assertEquals(10, thirdGenChildren.size)
  }

  @Test
  fun `test contains not existing node returns false`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    assertFalse(fastFileSearchTree.containsSegment(listOf("123", "456", "111.txt")))
  }

  @Test
  fun `test contains existing returns true`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val segments = listOf("123", "456", "111.txt")

    assertTrue(fastFileSearchTree.insertSegments(segments, 1))
    assertTrue(fastFileSearchTree.containsSegment(segments))
    assertEquals(1, fastFileSearchTree.findSegment(segments))
  }

  @Test
  fun `test contains non existing returns false`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val segments = listOf("123", "456", "111.txt")
    val segmentsNotExisting = listOf("123", "456", "789", "111.txt")

    assertTrue(fastFileSearchTree.insertSegments(segments, 1))
    assertFalse(fastFileSearchTree.containsSegment(segmentsNotExisting))
    assertNull(fastFileSearchTree.findSegment(segmentsNotExisting))
  }

  @Test
  fun `test remove first segment`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val segments = listOf("123", "456", "111.txt")

    assertTrue(fastFileSearchTree.insertSegments(segments, 1))
    assertTrue(fastFileSearchTree.containsSegment(segments))
    assertTrue(fastFileSearchTree.removeSegments(segments.take(1)))
    assertFalse(fastFileSearchTree.containsSegment(segments))
    assertFalse(fastFileSearchTree.containsSegment(segments.take(2)))
    assertFalse(fastFileSearchTree.containsSegment(segments.take(1)))
  }

  @Test
  fun `test remove last segment`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val segments = listOf("123", "456", "111.txt")

    assertTrue(fastFileSearchTree.insertSegments(segments, 1))
    assertTrue(fastFileSearchTree.containsSegment(segments))
    assertTrue(fastFileSearchTree.removeSegments(segments))
    assertFalse(fastFileSearchTree.containsSegment(segments))
    assertTrue(fastFileSearchTree.containsSegment(segments.take(2)))
    assertTrue(fastFileSearchTree.containsSegment(segments.take(1)))
  }

  @Test
  fun `test remove non existing segment`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val segments = listOf("123", "456", "111.txt")
    val segmentsNotExisting = listOf("123", "456", "789", "111.txt")

    assertTrue(fastFileSearchTree.insertSegments(segments, 1))
    assertTrue(fastFileSearchTree.removeSegments(segmentsNotExisting))
    assertNull(fastFileSearchTree.findSegment(segmentsNotExisting))
    assertEquals(1, fastFileSearchTree.findSegment(segments))
  }

  @Test
  fun `test try to find inner segment`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()
    val segments = listOf("123", "456", "111.txt")

    assertTrue(fastFileSearchTree.insertSegments(segments, 1))
    assertTrue(fastFileSearchTree.containsSegment(segments.take(1)))
  }

  @Test
  fun `test visit during path`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()

    val testSegments = listOf(
      listOf("0", "00", "000.txt"),
      listOf("0", "00", "111.txt"),
      listOf("0", "00", "222.txt"),
      listOf("0", "00", "333.txt")
    )

    for ((index, segments) in testSegments.withIndex()) {
      assertTrue(fastFileSearchTree.insertSegments(segments, index))
    }

    val resultNodeNames = mutableListOf<String>()

    fastFileSearchTree.visitEverySegmentDuringPath(listOf("0", "00", "000.txt")) { index, node ->
      resultNodeNames += node.getNodeName()!!
    }

    assertEquals(3, resultNodeNames.size)
    assertTrue("0" in resultNodeNames)
    assertTrue("00" in resultNodeNames)
    assertTrue("000.txt" in resultNodeNames)
  }

  @Test
  fun `test visit after path not recursively`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()

    val testSegments = listOf(
      listOf("0", "00", "000.txt"),
      listOf("0", "00", "111.txt"),
      listOf("0", "00", "222.txt"),
      listOf("0", "00", "333.txt"),
      listOf("0", "00", "000"),
      listOf("0", "00", "000", "444.txt"),
      listOf("0", "00", "000", "555.txt"),
      listOf("0", "00", "000", "666.txt"),
      listOf("0", "00", "111"),
      listOf("0", "00", "111", "777.txt"),
      listOf("0", "00", "111", "888.txt"),
      listOf("0", "00", "111", "999.txt")
    )

    for ((index, segments) in testSegments.withIndex()) {
      assertTrue(fastFileSearchTree.insertSegments(segments, index))
    }

    val resultNodeNames = mutableListOf<String>()

    fastFileSearchTree.visitEverySegmentAfterPath(listOf("0", "00"), false) { node ->
      resultNodeNames += node.getNodeName()!!
    }

    assertEquals(6, resultNodeNames.size)

    assertTrue("000.txt" in resultNodeNames)
    assertTrue("111.txt" in resultNodeNames)
    assertTrue("222.txt" in resultNodeNames)
    assertTrue("333.txt" in resultNodeNames)
    assertTrue("000" in resultNodeNames)
    assertTrue("111" in resultNodeNames)
  }

  @Test
  fun `test visit after path recursively`() {
    val fastFileSearchTree = FastFileSearchTree<Int>()

    val testSegments = listOf(
      listOf("0", "00", "000.txt"),
      listOf("0", "00", "111.txt"),
      listOf("0", "00", "222.txt"),
      listOf("0", "00", "333.txt"),
      listOf("0", "00", "000"),
      listOf("0", "00", "000", "444.txt"),
      listOf("0", "00", "000", "555.txt"),
      listOf("0", "00", "000", "666.txt"),
      listOf("0", "00", "111"),
      listOf("0", "00", "111", "777.txt"),
      listOf("0", "00", "111", "888.txt"),
      listOf("0", "00", "111", "999.txt")
    )

    for ((index, segments) in testSegments.withIndex()) {
      assertTrue(fastFileSearchTree.insertSegments(segments, index))
    }

    val resultNodeNames = mutableListOf<String>()

    fastFileSearchTree.visitEverySegmentAfterPath(listOf("0", "00"), true) { node ->
      resultNodeNames += node.getNodeName()!!
    }

    assertEquals(12, resultNodeNames.size)

    assertTrue("000.txt" in resultNodeNames)
    assertTrue("111.txt" in resultNodeNames)
    assertTrue("222.txt" in resultNodeNames)
    assertTrue("333.txt" in resultNodeNames)
    assertTrue("000" in resultNodeNames)
    assertTrue("111" in resultNodeNames)
    assertTrue("444.txt" in resultNodeNames)
    assertTrue("555.txt" in resultNodeNames)
    assertTrue("666.txt" in resultNodeNames)
    assertTrue("777.txt" in resultNodeNames)
    assertTrue("888.txt" in resultNodeNames)
    assertTrue("999.txt" in resultNodeNames)
  }

  // TODO: clear tests

  private fun assertChildrenAreLeafs(children: MutableMap<String, FastFileSearchTreeNode<Int>>) {
    assertEquals(0, children["0"]!!.getNodeChildren()["00"]!!.getNodeChildren()["000.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["1"]!!.getNodeChildren()["11"]!!.getNodeChildren()["111.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["2"]!!.getNodeChildren()["22"]!!.getNodeChildren()["222.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["3"]!!.getNodeChildren()["33"]!!.getNodeChildren()["333.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["4"]!!.getNodeChildren()["44"]!!.getNodeChildren()["444.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["5"]!!.getNodeChildren()["55"]!!.getNodeChildren()["555.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["6"]!!.getNodeChildren()["66"]!!.getNodeChildren()["666.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["7"]!!.getNodeChildren()["77"]!!.getNodeChildren()["777.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["8"]!!.getNodeChildren()["88"]!!.getNodeChildren()["888.txt"]!!.getNodeChildren().size)
    assertEquals(0, children["9"]!!.getNodeChildren()["99"]!!.getNodeChildren()["999.txt"]!!.getNodeChildren().size)
  }

  private fun nodeTypesCheck(fastFileSearchTree: FastFileSearchTree<Int>) {
    val collectedNodes = mutableListOf<FastFileSearchTreeNode<Int>>()
    fastFileSearchTree.visitAllSegments { node ->
      collectedNodes += node
    }

    val rootNodes = collectedNodes.filter { node -> node.isRoot() }
    val leafNodes = collectedNodes.filter { node -> node.isLeaf() }
    val nodes = collectedNodes.filter { node -> node.isNode() }

    assertEquals(1, rootNodes.size)
    assertEquals(10, leafNodes.size)
    assertEquals(20, nodes.size)
  }
}