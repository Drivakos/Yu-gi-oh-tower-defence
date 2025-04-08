using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhTowerDefense.Core
{
    public class GameStory : MonoBehaviour
    {
        [System.Serializable]
        public class StoryChapter
        {
            public string chapterName;
            [TextArea(3, 5)]
            public string description;
            public List<string> objectives;
            public List<YuGiOhCard> recruitableCards;
            public List<string> unlockedCards;
            public string milleniumItemInvolved;
            public bool isCompleted;
        }

        [Header("Story Settings")]
        [SerializeField] private List<StoryChapter> storyChapters = new List<StoryChapter>();
        [SerializeField] private YuGiOhCard celticGuardian;
        [SerializeField] private YuGiOhCard kuriboh;
        
        [Header("Recruitment Settings")]
        [SerializeField] private float baseRecruitmentChance = 0.4f;
        [SerializeField] private float rarityRecruitmentBonus = 0.1f;
        [SerializeField] private int maxRecruitmentAttempts = 3;

        private PlayerCollection playerCollection;
        private Dictionary<string, int> recruitmentAttempts = new Dictionary<string, int>();
        private int currentChapterIndex = 0;

        private void Awake()
        {
            playerCollection = PlayerCollection.Instance;
            InitializeStarterDeck();
            InitializeStoryChapters();
        }

        private void InitializeStarterDeck()
        {
            if (playerCollection == null)
            {
                Debug.LogError("PlayerCollection not found!");
                return;
            }

            // Add starter cards if player doesn't have them
            if (!playerCollection.HasCard(celticGuardian))
            {
                playerCollection.AddCard(celticGuardian);
                Debug.Log("Added Celtic Guardian to player's collection");
            }

            if (!playerCollection.HasCard(kuriboh))
            {
                playerCollection.AddCard(kuriboh);
                Debug.Log("Added Kuriboh to player's collection");
            }
        }

        private void InitializeStoryChapters()
        {
            if (storyChapters.Count == 0)
            {
                // Chapter 1: The Awakening
                storyChapters.Add(new StoryChapter
                {
                    chapterName = "The Shadow Realm's Call",
                    description = "You find yourself trapped in the Shadow Realm, drawn in by the mysterious power of a Millennium Item. " +
                                "With only Celtic Guardian and Kuriboh by your side, you must survive the darkness and discover the truth behind your imprisonment.",
                    objectives = new List<string>
                    {
                        "Survive your first wave of shadow creatures",
                        "Build your first defensive line",
                        "Recruit your first monster ally"
                    },
                    milleniumItemInvolved = "Millennium Ring",
                    recruitableCards = new List<YuGiOhCard>(),
                    unlockedCards = new List<string> { "Celtic Guardian", "Kuriboh" }
                });

                // Chapter 2: First Allies
                storyChapters.Add(new StoryChapter
                {
                    chapterName = "Gathering of Allies",
                    description = "As you delve deeper into the Shadow Realm, you discover that not all monsters are hostile. " +
                                "Some, trapped like yourself, seek alliance against a greater threat.",
                    objectives = new List<string>
                    {
                        "Recruit Dark Magician Girl",
                        "Complete 3 defensive battles",
                        "Build a deck with at least 10 cards"
                    },
                    milleniumItemInvolved = "Millennium Puzzle",
                    recruitableCards = new List<YuGiOhCard>(),
                    unlockedCards = new List<string> { "Dark Magician Girl", "Mystical Elf" }
                });
            }
        }

        public bool TryRecruitCard(YuGiOhCard card)
        {
            if (card == null || !IsCardRecruitable(card))
            {
                return false;
            }

            string cardId = card.CardId;
            
            // Check recruitment attempts
            if (!recruitmentAttempts.ContainsKey(cardId))
            {
                recruitmentAttempts[cardId] = 0;
            }

            if (recruitmentAttempts[cardId] >= maxRecruitmentAttempts)
            {
                Debug.Log($"Maximum recruitment attempts reached for {card.Name}");
                return false;
            }

            // Calculate recruitment chance
            float recruitChance = baseRecruitmentChance;
            
            // Add bonus based on rarity
            switch (card.Rarity.ToLower())
            {
                case "rare":
                    recruitChance += rarityRecruitmentBonus;
                    break;
                case "super rare":
                    recruitChance += rarityRecruitmentBonus * 2;
                    break;
                case "ultra rare":
                    recruitChance += rarityRecruitmentBonus * 3;
                    break;
            }

            // Add bonus for multiple attempts
            recruitChance += (recruitmentAttempts[cardId] * 0.1f);

            // Try recruitment
            recruitmentAttempts[cardId]++;
            bool success = Random.value <= recruitChance;

            if (success)
            {
                playerCollection.AddCard(card);
                Debug.Log($"Successfully recruited {card.Name}!");
            }

            return success;
        }

        public bool IsCardRecruitable(YuGiOhCard card)
        {
            if (card == null) return false;

            // Check if card is rare or above
            string rarity = card.Rarity.ToLower();
            return rarity == "rare" || rarity == "super rare" || rarity == "ultra rare";
        }

        public StoryChapter GetCurrentChapter()
        {
            if (currentChapterIndex < storyChapters.Count)
            {
                return storyChapters[currentChapterIndex];
            }
            return null;
        }

        public void AdvanceChapter()
        {
            if (currentChapterIndex < storyChapters.Count - 1)
            {
                storyChapters[currentChapterIndex].isCompleted = true;
                currentChapterIndex++;
                OnChapterAdvanced();
            }
        }

        private void OnChapterAdvanced()
        {
            var chapter = GetCurrentChapter();
            if (chapter != null)
            {
                // Unlock new cards for this chapter
                foreach (string cardName in chapter.unlockedCards)
                {
                    var card = FindCardByName(cardName);
                    if (card != null && !playerCollection.HasCard(card))
                    {
                        playerCollection.AddCard(card);
                    }
                }
            }
        }

        private YuGiOhCard FindCardByName(string cardName)
        {
            // This should be implemented to find a card in your card database
            return null; // Placeholder
        }

        public float GetRecruitmentChance(YuGiOhCard card)
        {
            if (!IsCardRecruitable(card)) return 0f;

            string cardId = card.CardId;
            float chance = baseRecruitmentChance;

            // Add rarity bonus
            switch (card.Rarity.ToLower())
            {
                case "rare":
                    chance += rarityRecruitmentBonus;
                    break;
                case "super rare":
                    chance += rarityRecruitmentBonus * 2;
                    break;
                case "ultra rare":
                    chance += rarityRecruitmentBonus * 3;
                    break;
            }

            // Add attempt bonus
            if (recruitmentAttempts.ContainsKey(cardId))
            {
                chance += (recruitmentAttempts[cardId] * 0.1f);
            }

            return Mathf.Clamp01(chance);
        }
    }
} 