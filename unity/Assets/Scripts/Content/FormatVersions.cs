using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Assets.Scripts.Content
{
    public class QuestFormat
    {
        public const int CURRENT_VERSION = (int) Versions.SPLIT_BASE_MOM_AND_CONVERSION_KIT;

        public readonly static HashSet<string> SCENARIOS_THAT_REQUIRE_CONVERSION_KIT = new HashSet<string>
        {
            "Artefatos_Roubados",
            "BelieveorDie1",
            "BlackWoodsSecrets",
            "DemoniosEntreLosWilson",
            "EditorCenario8",
            "EditorScenario3",
            "Escape",
            "HolyMansion",
            "Horror_Haunts_Merinda",
            "InTheDark",
            "La_Follia_di_Arkham",
            "Main_Street_Market_Mayham",
            "OMalqueNuncaDorme",
            "Saviors",
            "StrainOnReality",
            "Stressandstrain",
            "TheLairofRlimShaikorth",
            "TheRitualScenario",
            "TheRobberyOfTheKadakianIdol",
            "wiltshire"
        }.Select(t => t.ToLower(CultureInfo.InvariantCulture)).ToSet();

        public enum Versions
        {
            RICH_TEXT = 16,
            SPLIT_BASE_MOM_AND_CONVERSION_KIT = 17,
        }
    }
}