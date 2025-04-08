using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    [CreateAssetMenu(fileName = "MillenniumItemTutorialContent", menuName = "YuGiOh/Tutorial/Millennium Item Tutorial Content")]
    public class MillenniumItemTutorialContent : ScriptableObject
    {
        [System.Serializable]
        public class ItemTutorialContent
        {
            public MillenniumItemType itemType;
            public List<TutorialStep> steps = new List<TutorialStep>();
        }

        [SerializeField] private List<ItemTutorialContent> itemTutorials = new List<ItemTutorialContent>();

        // Introduction steps shown to all players
        [SerializeField] private List<TutorialStep> introductionSteps = new List<TutorialStep>
        {
            new TutorialStep
            {
                title = "Welcome to the Shadow Realm",
                description = "You have been drawn into the Shadow Realm by a mysterious force. The power of the Millennium Items may be your only way to survive and uncover the truth.",
                requiresItemCollection = false,
                highlightTarget = "MillenniumItemPanel"
            },
            new TutorialStep
            {
                title = "The Power of Millennium Items",
                description = "Each Millennium Item grants unique abilities. You'll need to master them all to overcome the challenges ahead.",
                requiresItemCollection = false,
                highlightTarget = "ItemInventory"
            }
        };

        // Default tutorial steps for each Millennium Item
        private void OnEnable()
        {
            // Millennium Ring
            AddItemTutorial(MillenniumItemType.Ring, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Ring",
                    description = "The Millennium Ring allows you to detect and locate hidden enemies and secrets. Its dark powers can also be used to banish creatures to the Shadow Realm.",
                    requiresItemCollection = true,
                    highlightTarget = "RingSlot"
                },
                new TutorialStep
                {
                    title = "Ring Detection",
                    description = "When activated, the Ring's pointers will guide you to nearby threats and hidden treasures. Watch for the golden glow indicating a discovery.",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "RingActivateButton"
                }
            });

            // Millennium Puzzle
            AddItemTutorial(MillenniumItemType.Puzzle, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Puzzle",
                    description = "The Millennium Puzzle holds the power to turn the tide of battle. It can strengthen your allies and confuse your enemies.",
                    requiresItemCollection = true,
                    highlightTarget = "PuzzleSlot"
                },
                new TutorialStep
                {
                    title = "Mind Shuffle",
                    description = "Activate the Puzzle to temporarily boost your allies' power and disorient nearby enemies, giving you a tactical advantage.",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "PuzzleActivateButton"
                }
            });

            // Millennium Eye
            AddItemTutorial(MillenniumItemType.Eye, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Eye",
                    description = "The Millennium Eye grants the power to see through deception and predict enemy movements. Use it to plan your strategy.",
                    requiresItemCollection = true,
                    highlightTarget = "EyeSlot"
                },
                new TutorialStep
                {
                    title = "Future Sight",
                    description = "When active, the Eye reveals enemy attack patterns and hidden weaknesses. Time your defenses accordingly.",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "EyeActivateButton"
                }
            });

            // Millennium Rod
            AddItemTutorial(MillenniumItemType.Rod, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Rod",
                    description = "The Millennium Rod allows you to take control of defeated enemies. Only rare or stronger monsters can resist its influence.",
                    requiresItemCollection = true,
                    highlightTarget = "RodSlot"
                },
                new TutorialStep
                {
                    title = "Mind Control",
                    description = "After defeating a worthy opponent, use the Rod to attempt recruitment. Higher rarity monsters require more attempts to control.",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "RodActivateButton"
                }
            });

            // Millennium Key
            AddItemTutorial(MillenniumItemType.Key, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Key",
                    description = "The Millennium Key can unlock hidden paths and reveal the true nature of your surroundings. It also protects against shadow magic.",
                    requiresItemCollection = true,
                    highlightTarget = "KeySlot"
                },
                new TutorialStep
                {
                    title = "Soul Protection",
                    description = "Activate the Key to shield your forces from shadow effects and reveal hidden passages in the environment.",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "KeyActivateButton"
                }
            });

            // Millennium Scale
            AddItemTutorial(MillenniumItemType.Scale, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Scale",
                    description = "The Millennium Scale judges the worth of your enemies and allies. Use it to balance your forces and identify powerful threats.",
                    requiresItemCollection = true,
                    highlightTarget = "ScaleSlot"
                },
                new TutorialStep
                {
                    title = "Power Judgment",
                    description = "The Scale reveals the true strength of all creatures on the field. Plan your strategy based on these revelations.",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "ScaleActivateButton"
                }
            });

            // Millennium Necklace
            AddItemTutorial(MillenniumItemType.Necklace, new List<TutorialStep>
            {
                new TutorialStep
                {
                    title = "The Millennium Necklace",
                    description = "The Millennium Necklace shows visions of possible futures. Use this knowledge to prepare for upcoming waves of enemies.",
                    requiresItemCollection = true,
                    highlightTarget = "NecklaceSlot"
                },
                new TutorialStep
                {
                    title = "Future Vision",
                    description = "Activate the Necklace to preview the next enemy wave and their spawn locations. Knowledge is power!",
                    requiresItemCollection = true,
                    requiresItemActivation = true,
                    highlightTarget = "NecklaceActivateButton"
                }
            });
        }

        private void AddItemTutorial(MillenniumItemType itemType, List<TutorialStep> steps)
        {
            var tutorial = new ItemTutorialContent
            {
                itemType = itemType,
                steps = steps
            };
            itemTutorials.Add(tutorial);
        }

        public List<TutorialStep> GetTutorialSteps(MillenniumItemType itemType)
        {
            var itemTutorial = itemTutorials.Find(t => t.itemType == itemType);
            return itemTutorial?.steps ?? new List<TutorialStep>();
        }

        public List<TutorialStep> GetIntroductionSteps()
        {
            return introductionSteps;
        }
    }
} 