using NUnit.Framework;
using System.Collections.Generic;
using ValkyrieTools;
using Assets.Scripts.Content;

namespace Valkyrie.UnitTests
{
    /// <summary>
    /// Unit tests for QuestData inner classes - QuestComponent subclasses that parse from Dictionary data.
    /// These tests focus on data parsing and initialization, avoiding runtime behavior that requires Game.Get().
    /// </summary>
    [TestFixture]
    public class QuestComponentTests
    {
        [SetUp]
        public void Setup()
        {
            // Disable ValkyrieDebug to prevent Unity logging during tests
            ValkyrieDebug.enabled = false;
        }

        [TearDown]
        public void TearDown()
        {
            ValkyrieDebug.enabled = true;
        }

        #region QuestComponent Base Class Tests

        [Test]
        public void QuestComponent_ParsesXPosition()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "xposition", "5.5" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(5.5f, tile.location.x, 0.001f);
            Assert.IsTrue(tile.locationSpecified);
        }

        [Test]
        public void QuestComponent_ParsesYPosition()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "yposition", "10.25" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(10.25f, tile.location.y, 0.001f);
            Assert.IsTrue(tile.locationSpecified);
        }

        [Test]
        public void QuestComponent_ParsesBothPositions()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "xposition", "3.0" },
                { "yposition", "7.0" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(3.0f, tile.location.x, 0.001f);
            Assert.AreEqual(7.0f, tile.location.y, 0.001f);
            Assert.IsTrue(tile.locationSpecified);
        }

        [Test]
        public void QuestComponent_DefaultPositionIsZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(0f, tile.location.x);
            Assert.AreEqual(0f, tile.location.y);
        }

        [Test]
        public void QuestComponent_ParsesComment()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "comment", "This is a test comment" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual("This is a test comment", tile.comment);
        }

        [Test]
        public void QuestComponent_ParsesOperations()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "operations", "health,=,100 gold,+,50" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.IsNotNull(tile.operations);
            Assert.AreEqual(2, tile.operations.Count);
            Assert.AreEqual("health", tile.operations[0].var);
            Assert.AreEqual("=", tile.operations[0].operation);
            Assert.AreEqual("100", tile.operations[0].value);
        }

        [Test]
        public void QuestComponent_ParsesVarTests()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "vartests", "VarOperation:health,>=,50 VarTestsLogicalOperator:AND VarOperation:gold,>,10" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.IsNotNull(tile.tests);
            Assert.AreEqual(3, tile.tests.VarTestsComponents.Count);
        }

        [Test]
        public void QuestComponent_ParsesLegacyConditions()
        {
            // Arrange - old format 'conditions' should be converted to vartests
            var data = new Dictionary<string, string>
            {
                { "conditions", "health,>=,50 gold,>,10" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.IsNotNull(tile.tests);
            // Should have 2 conditions with 1 AND operator between them = 3 components
            Assert.AreEqual(3, tile.tests.VarTestsComponents.Count);
        }

        [Test]
        public void QuestComponent_SetsSourcePath()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "custom_source.ini");

            // Assert
            Assert.AreEqual("custom_source.ini", tile.source);
        }

        [Test]
        public void QuestComponent_SetsSectionName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("MyTile", data, "test.ini");

            // Assert
            Assert.AreEqual("MyTile", tile.sectionName);
        }

        [Test]
        public void QuestComponent_RemoveFromArray_RemovesElement()
        {
            // Arrange
            string[] array = { "one", "two", "three", "two" };

            // Act
            string[] result = QuestData.QuestComponent.RemoveFromArray(array, "two");

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("one", result[0]);
            Assert.AreEqual("three", result[1]);
        }

        [Test]
        public void QuestComponent_RemoveFromArray_NoMatchReturnsOriginal()
        {
            // Arrange
            string[] array = { "one", "two", "three" };

            // Act
            string[] result = QuestData.QuestComponent.RemoveFromArray(array, "four");

            // Assert
            Assert.AreEqual(3, result.Length);
        }

        [Test]
        public void QuestComponent_GenKey_ReturnsCorrectFormat()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string key = tile.genKey("text");

            // Assert
            Assert.AreEqual("Tile1.text", key);
        }

        #endregion

        #region Tile Tests

        [Test]
        public void Tile_ParsesRotation()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "rotation", "90" },
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(90, tile.rotation);
        }

        [Test]
        public void Tile_ParsesTileSideName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "DungeonTile1A" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual("DungeonTile1A", tile.tileSideName);
        }

        [Test]
        public void Tile_DefaultRotationIsZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(0, tile.rotation);
        }

        [Test]
        public void Tile_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual("Tile", tile.typeDynamic);
        }

        [Test]
        public void Tile_LocationSpecifiedIsTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.IsTrue(tile.locationSpecified);
        }

        [Test]
        public void Tile_ToStringContainsSide()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsTrue(result.Contains("side=TestSide"));
        }

        [Test]
        public void Tile_ToStringContainsRotationWhenNonZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" },
                { "rotation", "180" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsTrue(result.Contains("rotation=180"));
        }

        [Test]
        public void Tile_ToStringOmitsRotationWhenZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsFalse(result.Contains("rotation="));
        }

        [Test]
        public void Tile_InvalidRotationDefaultsToZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "side", "TestSide" },
                { "rotation", "invalid" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(0, tile.rotation);
        }

        [Test]
        public void Tile_ParsesCustomImage()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "path/to/image.png" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual("path/to/image.png", tile.customImage);
        }

        [Test]
        public void Tile_ParsesTop()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "top", "105.5" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(105.5f, tile.top, 0.001f);
        }

        [Test]
        public void Tile_ParsesLeft()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "left", "-50.2" }
            };

            // Act
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Assert
            Assert.AreEqual(-50.2f, tile.left, 0.001f);
        }

        [Test]
        public void Tile_ToStringContainsCustomImage()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "myimage.png" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsTrue(result.Contains("customImage=myimage.png"));
        }

        [Test]
        public void Tile_ToStringContainsTopWhenCustomImageSet()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "myimage.png" },
                { "top", "10.5" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsTrue(result.Contains("top=10.5"));
        }

        [Test]
        public void Tile_ToStringContainsLeftWhenCustomImageSet()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "myimage.png" },
                { "left", "-5.5" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsTrue(result.Contains("left=-5.5"));
        }

        [Test]
        public void Tile_ToStringOmitsSideWhenSideEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "myimage.png" }
                // side is not set, so tileSideName is parsed as empty or from side if present
            };
            // Manually ensure tileSideName is empty as logic might default it if not present
            var tile = new QuestData.Tile("Tile1", data, "test.ini");
            tile.tileSideName = "";

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsFalse(result.Contains("side="));
        }

        [Test]
        public void Tile_ToStringOmitsTopWhenZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "myimage.png" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");
            // top defaults to 0

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsFalse(result.Contains("top="));
        }

        [Test]
        public void Tile_ToStringOmitsLeftWhenZero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "customImage", "myimage.png" }
            };
            var tile = new QuestData.Tile("Tile1", data, "test.ini");
            // left defaults to 0

            // Act
            string result = tile.ToString();

            // Assert
            Assert.IsFalse(result.Contains("left="));
        }

        #endregion

        #region MPlace Tests

        [Test]
        public void MPlace_ParsesMaster()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "master", "true" }
            };

            // Act
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Assert
            Assert.IsTrue(mplace.master);
        }

        [Test]
        public void MPlace_ParsesRotate()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "rotate", "true" }
            };

            // Act
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Assert
            Assert.IsTrue(mplace.rotate);
        }

        [Test]
        public void MPlace_DefaultMasterIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Assert
            Assert.IsFalse(mplace.master);
        }

        [Test]
        public void MPlace_DefaultRotateIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Assert
            Assert.IsFalse(mplace.rotate);
        }

        [Test]
        public void MPlace_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Assert
            Assert.AreEqual("MPlace", mplace.typeDynamic);
        }

        [Test]
        public void MPlace_LocationSpecifiedIsTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Assert
            Assert.IsTrue(mplace.locationSpecified);
        }

        [Test]
        public void MPlace_ToStringContainsMasterWhenTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "master", "true" }
            };
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Act
            string result = mplace.ToString();

            // Assert
            Assert.IsTrue(result.Contains("master=true"));
        }

        [Test]
        public void MPlace_ToStringContainsRotateWhenTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "rotate", "true" }
            };
            var mplace = new QuestData.MPlace("MPlace1", data, "test.ini");

            // Act
            string result = mplace.ToString();

            // Assert
            Assert.IsTrue(result.Contains("rotate=true"));
        }

        #endregion

        #region QItem Tests

        [Test]
        public void QItem_ParsesItemName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "itemname", "Sword Shield" }
            };

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual(2, qitem.itemName.Length);
            Assert.AreEqual("Sword", qitem.itemName[0]);
            Assert.AreEqual("Shield", qitem.itemName[1]);
        }

        [Test]
        public void QItem_ParsesStarting()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "starting", "true" }
            };

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.IsTrue(qitem.starting);
        }

        [Test]
        public void QItem_DefaultStartingWhenMissing()
        {
            // Arrange - when 'starting' key is missing, defaults to true (deprecated format 3)
            var data = new Dictionary<string, string>();

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.IsTrue(qitem.starting);
        }

        [Test]
        public void QItem_ParsesTraits()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "traits", "weapon melee blade" }
            };

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual(3, qitem.traits.Length);
            Assert.AreEqual("weapon", qitem.traits[0]);
            Assert.AreEqual("melee", qitem.traits[1]);
            Assert.AreEqual("blade", qitem.traits[2]);
        }

        [Test]
        public void QItem_ParsesTraitpool()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "traitpool", "magic ranged" }
            };

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual(2, qitem.traitpool.Length);
            Assert.AreEqual("magic", qitem.traitpool[0]);
            Assert.AreEqual("ranged", qitem.traitpool[1]);
        }

        [Test]
        public void QItem_ParsesInspect()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "inspect", "EventInspect" }
            };

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual("EventInspect", qitem.inspect);
        }

        [Test]
        public void QItem_DefaultItemNameIsEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual(0, qitem.itemName.Length);
        }

        [Test]
        public void QItem_DefaultTraitsIsEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual(0, qitem.traits.Length);
        }

        [Test]
        public void QItem_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Assert
            Assert.AreEqual("QItem", qitem.typeDynamic);
        }

        [Test]
        public void QItem_ChangeReference_UpdatesInspect()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "inspect", "OldEvent" }
            };
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Act
            qitem.ChangeReference("OldEvent", "NewEvent");

            // Assert
            Assert.AreEqual("NewEvent", qitem.inspect);
        }

        [Test]
        public void QItem_ChangeReference_UpdatesItemName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "itemname", "OldItem NewItem" }
            };
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Act
            qitem.ChangeReference("OldItem", "RenamedItem");

            // Assert
            Assert.AreEqual("RenamedItem", qitem.itemName[0]);
            Assert.AreEqual("NewItem", qitem.itemName[1]);
        }

        [Test]
        public void QItem_ToStringContainsItemName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "itemname", "Sword" }
            };
            var qitem = new QuestData.QItem("QItem1", data, "test.ini");

            // Act
            string result = qitem.ToString();

            // Assert
            Assert.IsTrue(result.Contains("itemname=Sword"));
        }

        #endregion

        #region Activation Tests

        [Test]
        public void Activation_ParsesMinionFirst()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "minionfirst", "true" }
            };

            // Act
            var activation = new QuestData.Activation("Activation1", data, "test.ini");

            // Assert
            Assert.IsTrue(activation.minionFirst);
        }

        [Test]
        public void Activation_ParsesMasterFirst()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "masterfirst", "true" }
            };

            // Act
            var activation = new QuestData.Activation("Activation1", data, "test.ini");

            // Assert
            Assert.IsTrue(activation.masterFirst);
        }

        [Test]
        public void Activation_DefaultMinionFirstIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var activation = new QuestData.Activation("Activation1", data, "test.ini");

            // Assert
            Assert.IsFalse(activation.minionFirst);
        }

        [Test]
        public void Activation_DefaultMasterFirstIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var activation = new QuestData.Activation("Activation1", data, "test.ini");

            // Assert
            Assert.IsFalse(activation.masterFirst);
        }

        [Test]
        public void Activation_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var activation = new QuestData.Activation("Activation1", data, "test.ini");

            // Assert
            Assert.AreEqual("Activation", activation.typeDynamic);
        }

        [Test]
        public void Activation_ToStringContainsMinionFirstWhenTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "minionfirst", "true" }
            };
            var activation = new QuestData.Activation("Activation1", data, "test.ini");

            // Act
            string result = activation.ToString();

            // Assert
            Assert.IsTrue(result.Contains("minionfirst=True"));
        }

        [Test]
        public void Activation_GenKeyFormatsCorrectly()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var activation = new QuestData.Activation("ActivationTest", data, "test.ini");

            // Act & Assert
            Assert.AreEqual("ActivationTest.ability", activation.ability_key);
            Assert.AreEqual("ActivationTest.minion", activation.minion_key);
            Assert.AreEqual("ActivationTest.master", activation.master_key);
            Assert.AreEqual("ActivationTest.movebutton", activation.movebutton_key);
            Assert.AreEqual("ActivationTest.move", activation.move_key);
        }

        #endregion

        #region CustomMonster Tests

        [Test]
        public void CustomMonster_ParsesBaseMonster()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "base", "Goblin" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual("Goblin", monster.baseMonster);
        }

        [Test]
        public void CustomMonster_ParsesTraits()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "traits", "humanoid cursed" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual(2, monster.traits.Length);
            Assert.AreEqual("humanoid", monster.traits[0]);
            Assert.AreEqual("cursed", monster.traits[1]);
        }

        [Test]
        public void CustomMonster_ParsesImagePath()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "image", "monsters\\goblin.png" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual("monsters/goblin.png", monster.imagePath); // backslash converted to forward slash
        }

        [Test]
        public void CustomMonster_ParsesImagePlace()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "imageplace", "monsters/goblin_place.png" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual("monsters/goblin_place.png", monster.imagePlace);
        }

        [Test]
        public void CustomMonster_ParsesActivations()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "activation", "GoblinAttack GoblinMove" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual(2, monster.activations.Length);
            Assert.AreEqual("GoblinAttack", monster.activations[0]);
            Assert.AreEqual("GoblinMove", monster.activations[1]);
        }

        [Test]
        public void CustomMonster_ParsesHealth()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "health", "10" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual(10f, monster.healthBase, 0.001f);
            Assert.IsTrue(monster.healthDefined);
        }

        [Test]
        public void CustomMonster_ParsesHealthPerHero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "healthperhero", "5.5" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual(5.5f, monster.healthPerHero, 0.001f);
            Assert.IsTrue(monster.healthDefined);
        }

        [Test]
        public void CustomMonster_ParsesEvadeEvent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "evadeevent", "EventEvade" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual("EventEvade", monster.evadeEvent);
        }

        [Test]
        public void CustomMonster_ParsesHorrorEvent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "horrorevent", "EventHorror" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual("EventHorror", monster.horrorEvent);
        }

        [Test]
        public void CustomMonster_ParsesHorror()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "horror", "3" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual(3, monster.horror);
            Assert.IsTrue(monster.horrorDefined);
        }

        [Test]
        public void CustomMonster_ParsesAwareness()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "awareness", "2" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual(2, monster.awareness);
            Assert.IsTrue(monster.awarenessDefined);
        }

        [Test]
        public void CustomMonster_ParsesAttacks()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "attacks", "melee:2 ranged:1" }
            };

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.IsTrue(monster.investigatorAttacks.ContainsKey("melee"));
            Assert.IsTrue(monster.investigatorAttacks.ContainsKey("ranged"));
            Assert.AreEqual(2, monster.investigatorAttacks["melee"].Count);
            Assert.AreEqual(1, monster.investigatorAttacks["ranged"].Count);
        }

        [Test]
        public void CustomMonster_DefaultHealthDefinedIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.IsFalse(monster.healthDefined);
        }

        [Test]
        public void CustomMonster_DefaultHorrorDefinedIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.IsFalse(monster.horrorDefined);
        }

        [Test]
        public void CustomMonster_DefaultAwarenessDefinedIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.IsFalse(monster.awarenessDefined);
        }

        [Test]
        public void CustomMonster_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Assert
            Assert.AreEqual("CustomMonster", monster.typeDynamic);
        }

        [Test]
        public void CustomMonster_ChangeReference_UpdatesActivations()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "activation", "OldActivation NewActivation" }
            };
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Act
            monster.ChangeReference("OldActivation", "RenamedActivation");

            // Assert
            Assert.AreEqual("RenamedActivation", monster.activations[0]);
        }

        [Test]
        public void CustomMonster_ChangeReference_UpdatesEvadeEvent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "evadeevent", "OldEvent" }
            };
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Act
            monster.ChangeReference("OldEvent", "NewEvent");

            // Assert
            Assert.AreEqual("NewEvent", monster.evadeEvent);
        }

        [Test]
        public void CustomMonster_ChangeReference_UpdatesHorrorEvent()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "horrorevent", "OldEvent" }
            };
            var monster = new QuestData.CustomMonster("CustomMonster1", data, "test.ini");

            // Act
            monster.ChangeReference("OldEvent", "NewEvent");

            // Assert
            Assert.AreEqual("NewEvent", monster.horrorEvent);
        }

        [Test]
        public void CustomMonster_GenKeyFormatsCorrectly()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var monster = new QuestData.CustomMonster("CustomMonsterTest", data, "test.ini");

            // Act & Assert
            Assert.AreEqual("CustomMonsterTest.monstername", monster.monstername_key);
            Assert.AreEqual("CustomMonsterTest.info", monster.info_key);
        }

        #endregion

        #region Event Tests

        [Test]
        public void Event_ParsesDisplay()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "display", "false" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.IsFalse(evt.display);
        }

        [Test]
        public void Event_ParsesHighlight()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "highlight", "true" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.IsTrue(evt.highlight);
        }

        [Test]
        public void Event_ParsesButtonCount()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "buttons", "3" },
                { "event1", "NextEvent1" },
                { "event2", "NextEvent2" },
                { "event3", "NextEvent3" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(3, evt.buttons.Count);
        }

        [Test]
        public void Event_DisplayEventGetsAtLeastOneButton()
        {
            // Arrange - display is true by default, buttons=0 should still give 1 button
            var data = new Dictionary<string, string>
            {
                { "display", "true" },
                { "buttons", "0" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(1, evt.buttons.Count);
        }

        [Test]
        public void Event_ParsesHeroListName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "hero", "HeroSelectEvent" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual("HeroSelectEvent", evt.heroListName);
        }

        [Test]
        public void Event_ParsesQuota()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "quota", "5" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(5, evt.quota);
        }

        [Test]
        public void Event_ParsesQuotaVar()
        {
            // Arrange - quota starting with non-digit is treated as variable
            var data = new Dictionary<string, string>
            {
                { "quota", "myVar" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual("myVar", evt.quotaVar);
        }

        [Test]
        public void Event_ParsesMinHeroes()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "minhero", "2" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(2, evt.minHeroes);
        }

        [Test]
        public void Event_ParsesMaxHeroes()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "maxhero", "4" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(4, evt.maxHeroes);
        }

        [Test]
        public void Event_ParsesAddComponents()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "add", "Component1 Component2 Component3" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(3, evt.addComponents.Length);
            Assert.AreEqual("Component1", evt.addComponents[0]);
            Assert.AreEqual("Component2", evt.addComponents[1]);
            Assert.AreEqual("Component3", evt.addComponents[2]);
        }

        [Test]
        public void Event_ParsesRemoveComponents()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "remove", "OldComponent1 OldComponent2" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(2, evt.removeComponents.Length);
            Assert.AreEqual("OldComponent1", evt.removeComponents[0]);
            Assert.AreEqual("OldComponent2", evt.removeComponents[1]);
        }

        [Test]
        public void Event_ParsesTrigger()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "trigger", "RoundStart" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual("RoundStart", evt.trigger);
        }

        [Test]
        public void Event_ParsesRandomEvents()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "randomevents", "true" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.IsTrue(evt.randomEvents);
        }

        [Test]
        public void Event_ParsesMinCam()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "mincam", "true" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.IsTrue(evt.minCam);
            Assert.IsFalse(evt.locationSpecified);
        }

        [Test]
        public void Event_ParsesMaxCam()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "maxcam", "true" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.IsTrue(evt.maxCam);
            Assert.IsFalse(evt.locationSpecified);
        }

        [Test]
        public void Event_ParsesAudio()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "audio", "sounds\\effect.ogg" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual("sounds/effect.ogg", evt.audio); // backslash converted
        }

        [Test]
        public void Event_ParsesMusic()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "music", "music\\track1.ogg music\\track2.ogg" }
            };

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual(2, evt.music.Count);
            Assert.AreEqual("music/track1.ogg", evt.music[0]);
            Assert.AreEqual("music/track2.ogg", evt.music[1]);
        }

        [Test]
        public void Event_DefaultDisplayIsTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act - display defaults to true unless specified
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            // Note: The code doesn't set display=true by default in constructor,
            // it only reads from data. So default bool is false unless 'display' key exists with 'true'
            // However, looking at code: display is not initialized to true, so default is false for the bool
            // But the code says "Displayed events must have a button" suggesting display might default to true conceptually
            // Let me check - in constructor: no default set for display, so it's false by default
            Assert.IsTrue(evt.display);
        }

        [Test]
        public void Event_DefaultTriggerIsEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual("", evt.trigger);
        }

        [Test]
        public void Event_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Assert
            Assert.AreEqual("Event", evt.typeDynamic);
        }

        [Test]
        public void Event_ChangeReference_UpdatesHeroListName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "hero", "OldHeroEvent" }
            };
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Act
            evt.ChangeReference("OldHeroEvent", "NewHeroEvent");

            // Assert
            Assert.AreEqual("NewHeroEvent", evt.heroListName);
        }

        [Test]
        public void Event_ChangeReference_UpdatesAddComponents()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "add", "OldComponent NewComponent" }
            };
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Act
            evt.ChangeReference("OldComponent", "RenamedComponent");

            // Assert
            Assert.AreEqual("RenamedComponent", evt.addComponents[0]);
        }

        [Test]
        public void Event_ChangeReference_UpdatesRemoveComponents()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "remove", "OldComponent KeepComponent" }
            };
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Act
            evt.ChangeReference("OldComponent", "RenamedComponent");

            // Assert
            Assert.AreEqual("RenamedComponent", evt.removeComponents[0]);
        }

        [Test]
        public void Event_ChangeReference_UpdatesTriggerForDefeated()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "trigger", "DefeatedOldMonster" }
            };
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Act
            evt.ChangeReference("OldMonster", "NewMonster");

            // Assert
            Assert.AreEqual("DefeatedNewMonster", evt.trigger);
        }

        [Test]
        public void Event_ChangeReference_UpdatesTriggerForDefeatedUnique()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "trigger", "DefeatedUniqueOldMonster" }
            };
            var evt = new QuestData.Event("Event1", data, "test.ini", 10);

            // Act
            evt.ChangeReference("OldMonster", "NewMonster");

            // Assert
            Assert.AreEqual("DefeatedUniqueNewMonster", evt.trigger);
        }

        [Test]
        public void Event_GenKeyFormatsCorrectly()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var evt = new QuestData.Event("EventTest", data, "test.ini", 10);

            // Act & Assert
            Assert.AreEqual("EventTest.text", evt.text_key);
        }

        #endregion

        #region Puzzle Tests

        [Test]
        public void Puzzle_ParsesPuzzleClass()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "class", "code" }
            };

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("code", puzzle.puzzleClass);
        }

        [Test]
        public void Puzzle_DefaultPuzzleClassIsSlide()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("slide", puzzle.puzzleClass);
        }

        [Test]
        public void Puzzle_ParsesImageType()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "image", "puzzles\\custom.png" }
            };

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("puzzles/custom.png", puzzle.imageType);
        }

        [Test]
        public void Puzzle_ParsesSkill()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "skill", "{lore}" }
            };

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("{lore}", puzzle.skill);
        }

        [Test]
        public void Puzzle_DefaultSkillIsObservation()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("{observation}", puzzle.skill);
        }

        [Test]
        public void Puzzle_ParsesPuzzleLevel()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "puzzlelevel", "6" }
            };

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual(6, puzzle.puzzleLevel);
        }

        [Test]
        public void Puzzle_DefaultPuzzleLevelIs4()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual(4, puzzle.puzzleLevel);
        }

        [Test]
        public void Puzzle_ParsesPuzzleAltLevel()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "puzzlealtlevel", "5" }
            };

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual(5, puzzle.puzzleAltLevel);
        }

        [Test]
        public void Puzzle_DefaultPuzzleAltLevelIs3()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual(3, puzzle.puzzleAltLevel);
        }

        [Test]
        public void Puzzle_ParsesPuzzleSolution()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "puzzlesolution", "1234" }
            };

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("1234", puzzle.puzzleSolution);
        }

        [Test]
        public void Puzzle_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Assert
            Assert.AreEqual("Puzzle", puzzle.typeDynamic);
        }

        [Test]
        public void Puzzle_ToStringContainsClassWhenNotSlide()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "class", "code" }
            };
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Act
            string result = puzzle.ToString();

            // Assert
            Assert.IsTrue(result.Contains("class=code"));
        }

        [Test]
        public void Puzzle_ToStringOmitsClassWhenSlide()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var puzzle = new QuestData.Puzzle("Puzzle1", data, "test.ini");

            // Act
            string result = puzzle.ToString();

            // Assert
            Assert.IsFalse(result.Contains("class="));
        }

        #endregion

        #region Door Tests (extends Event)

        [Test]
        public void Door_ParsesRotation()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "rotation", "90" }
            };

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(90, door.rotation);
        }

        [Test]
        public void Door_ParsesColor()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "color", "#FF0000" }
            };

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("#FF0000", door.colourName);
        }

        [Test]
        public void Door_DefaultColorIsWhite()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("white", door.colourName);
        }

        [Test]
        public void Door_DefaultRotationIsZero()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(0, door.rotation);
        }

        [Test]
        public void Door_IsCancelable()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(door.cancelable);
        }

        [Test]
        public void Door_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("Door", door.typeDynamic);
        }

        [Test]
        public void Door_LocationSpecifiedIsTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(door.locationSpecified);
        }

        [Test]
        public void Door_ToStringContainsColorWhenNotWhite()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "color", "red" }
            };
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Act
            string result = door.ToString();

            // Assert
            Assert.IsTrue(result.Contains("color=red"));
        }

        [Test]
        public void Door_ToStringOmitsColorWhenWhite()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var door = new QuestData.Door("Door1", data, null, "test.ini");

            // Act
            string result = door.ToString();

            // Assert
            Assert.IsFalse(result.Contains("color="));
        }

        #endregion

        #region Token Tests (extends Event)

        [Test]
        public void Token_ParsesTokenName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "type", "TokenSearch" }
            };

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("TokenSearch", token.tokenName);
        }

        [Test]
        public void Token_ParsesRotation()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "rotation", "45" }
            };

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(45, token.rotation);
        }

        [Test]
        public void Token_DefaultTokenNameIsEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("", token.tokenName);
        }

        [Test]
        public void Token_DefaultRotationIsZero()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(0, token.rotation);
        }

        [Test]
        public void Token_IsCancelable()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(token.cancelable);
        }

        [Test]
        public void Token_TestsIsNull()
        {
            // Arrange - Tokens don't have conditions, so tests should be null
            var data = new Dictionary<string, string>();

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.IsNull(token.tests);
        }

        [Test]
        public void Token_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("Token", token.typeDynamic);
        }

        [Test]
        public void Token_LocationSpecifiedIsTrue()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(token.locationSpecified);
        }

        [Test]
        public void Token_ToStringContainsType()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "type", "TokenExplore" }
            };
            var token = new QuestData.Token("Token1", data, null, "test.ini");

            // Act
            string result = token.ToString();

            // Assert
            Assert.IsTrue(result.Contains("type=TokenExplore"));
        }

        #endregion

        #region UI Tests (extends Event)

        [Test]
        public void UI_ParsesImageName()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "image", "ui\\button.png" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("ui/button.png", ui.imageName); // backslash converted
        }

        [Test]
        public void UI_ParsesVerticalUnits()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "vunits", "true" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(ui.verticalUnits);
        }

        [Test]
        public void UI_ParsesSize()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "size", "2.5" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(2.5f, ui.size, 0.001f);
        }

        [Test]
        public void UI_ParsesTextSize()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "textsize", "1.5" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(1.5f, ui.textSize, 0.001f);
        }

        [Test]
        public void UI_ParsesTextColor()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "textcolor", "red" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("red", ui.textColor);
        }

        [Test]
        public void UI_ParsesTextBackgroundColor()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "textbackgroundcolor", "black" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("black", ui.textBackgroundColor);
        }

        [Test]
        public void UI_ParsesHAlignLeft()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "halign", "left" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(-1, ui.hAlign);
        }

        [Test]
        public void UI_ParsesHAlignRight()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "halign", "right" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(1, ui.hAlign);
        }

        [Test]
        public void UI_ParsesVAlignTop()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "valign", "top" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(-1, ui.vAlign);
        }

        [Test]
        public void UI_ParsesVAlignBottom()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "valign", "bottom" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(1, ui.vAlign);
        }

        [Test]
        public void UI_ParsesTextAlignment()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "textAlignment", "TOP" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(TextAlignment.TOP, ui.textAlignment);
        }

        [Test]
        public void UI_ParsesRichText()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "richText", "true" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(ui.richText);
        }

        [Test]
        public void UI_ParsesBorder()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "border", "true" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(ui.border);
        }

        [Test]
        public void UI_ParsesTextAspect()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "textaspect", "0.75" }
            };

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(0.75f, ui.aspect, 0.001f);
        }

        [Test]
        public void UI_DefaultImageNameIsEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("", ui.imageName);
        }

        [Test]
        public void UI_DefaultSizeIs1()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(1f, ui.size, 0.001f);
        }

        [Test]
        public void UI_DefaultTextColorIsWhite()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("white", ui.textColor);
        }

        [Test]
        public void UI_DefaultTextBackgroundColorIsTransparent()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("transparent", ui.textBackgroundColor);
        }

        [Test]
        public void UI_DefaultHAlignIs0()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(0, ui.hAlign);
        }

        [Test]
        public void UI_DefaultVAlignIs0()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(0, ui.vAlign);
        }

        [Test]
        public void UI_DefaultTextAlignmentIsCenter()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(TextAlignment.CENTER, ui.textAlignment);
        }

        [Test]
        public void UI_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var ui = new QuestData.UI("UI1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("UI", ui.typeDynamic);
        }

        [Test]
        public void UI_GenKeyFormatsCorrectly()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var ui = new QuestData.UI("UITest", data, null, "test.ini");

            // Act & Assert
            Assert.AreEqual("UITest.uitext", ui.uitext_key);
        }

        #endregion

        #region Spawn Tests (extends Event)

        [Test]
        public void Spawn_ParsesMonsterTypes()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "monster", "Goblin Zombie Dragon" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(3, spawn.mTypes.Length);
            Assert.AreEqual("Goblin", spawn.mTypes[0]);
            Assert.AreEqual("Zombie", spawn.mTypes[1]);
            Assert.AreEqual("Dragon", spawn.mTypes[2]);
        }

        [Test]
        public void Spawn_ParsesTraitsRequired()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "traits", "undead humanoid" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(2, spawn.mTraitsRequired.Length);
            Assert.AreEqual("undead", spawn.mTraitsRequired[0]);
            Assert.AreEqual("humanoid", spawn.mTraitsRequired[1]);
        }

        [Test]
        public void Spawn_ParsesTraitsPool()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "traitpool", "flying large" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(2, spawn.mTraitsPool.Length);
            Assert.AreEqual("flying", spawn.mTraitsPool[0]);
            Assert.AreEqual("large", spawn.mTraitsPool[1]);
        }

        [Test]
        public void Spawn_ParsesUnique()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "unique", "true" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(spawn.unique);
        }

        [Test]
        public void Spawn_ParsesActivated()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "activated", "true" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.IsTrue(spawn.activated);
        }

        [Test]
        public void Spawn_ParsesUniqueHealthBase()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "uniquehealth", "50" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(50f, spawn.uniqueHealthBase, 0.001f);
        }

        [Test]
        public void Spawn_ParsesUniqueHealthHero()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "uniquehealthhero", "10" }
            };

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(10f, spawn.uniqueHealthHero, 0.001f);
        }

        [Test]
        public void Spawn_DefaultMonsterTypesIsEmpty()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual(0, spawn.mTypes.Length);
        }

        [Test]
        public void Spawn_DefaultUniqueIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.IsFalse(spawn.unique);
        }

        [Test]
        public void Spawn_DefaultActivatedIsFalse()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.IsFalse(spawn.activated);
        }

        [Test]
        public void Spawn_SetsDynamicType()
        {
            // Arrange
            var data = new Dictionary<string, string>();

            // Act
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Assert
            Assert.AreEqual("Spawn", spawn.typeDynamic);
        }

        [Test]
        public void Spawn_GenKeyFormatsCorrectly()
        {
            // Arrange
            var data = new Dictionary<string, string>();
            var spawn = new QuestData.Spawn("SpawnTest", data, null, "test.ini");

            // Act & Assert
            Assert.AreEqual("SpawnTest.uniquetitle", spawn.uniquetitle_key);
            Assert.AreEqual("SpawnTest.uniquetext", spawn.uniquetext_key);
        }

        [Test]
        public void Spawn_ChangeReference_UpdatesMonsterTypes()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "monster", "OldMonster NewMonster" }
            };
            var spawn = new QuestData.Spawn("Spawn1", data, null, "test.ini");

            // Act - Note: only renames if oldName doesn't start with "Monster"
            spawn.ChangeReference("OldMonster", "RenamedMonster");

            // Assert
            Assert.AreEqual("RenamedMonster", spawn.mTypes[0]);
        }

        #endregion
    }
}
