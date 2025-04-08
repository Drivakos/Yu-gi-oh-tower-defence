using UnityEngine;

namespace YuGiOhTowerDefense.Core.CardPacks
{
    [CreateAssetMenu(fileName = "Trap Pack", menuName = "YuGiOh/Card Packs/Trap Pack")]
    public class TrapPack : CardPack
    {
        private void OnValidate()
        {
            // Set default values for trap pack
            if (string.IsNullOrEmpty(PackName))
            {
                PackName = "Trap Pack";
            }

            if (string.IsNullOrEmpty(Description))
            {
                Description = "A pack containing powerful trap cards to defend your tower.";
            }

            if (Cost <= 0)
            {
                Cost = 1000; // Default cost for trap pack
            }

            // Set default rarity chances for trap pack
            if (GetRarityChance(CardRarity.Common) == 0)
            {
                // Traps are generally more rare than monsters
                commonChance = 50f;
                rareChance = 30f;
                superRareChance = 15f;
                ultraRareChance = 5f;
            }
        }
    }
} 