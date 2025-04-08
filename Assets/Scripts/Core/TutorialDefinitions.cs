using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core
{
    public class TutorialDefinitions : MonoBehaviour
    {
        [SerializeField] private TutorialManager tutorialManager;
        
        [Header("Tutorial Icons")]
        [SerializeField] private Sprite monsterIcon;
        [SerializeField] private Sprite cardIcon;
        [SerializeField] private Sprite waveIcon;
        [SerializeField] private Sprite upgradeIcon;
        [SerializeField] private Sprite victoryIcon;
        
        private void Awake()
        {
            if (tutorialManager == null)
            {
                tutorialManager = FindObjectOfType<TutorialManager>();
            }
            
            DefineTutorialSteps();
        }
        
        private void DefineTutorialSteps()
        {
            List<TutorialManager.TutorialStep> steps = new List<TutorialManager.TutorialStep>
            {
                // Welcome
                new TutorialManager.TutorialStep
                {
                    stepId = "welcome",
                    title = "Welcome to Yu-Gi-Oh! Tower Defense",
                    description = "Learn how to defend your territory using powerful monsters and strategic cards. Let's go through the basics!",
                    icon = null,
                    requiredAction = TutorialManager.TutorialAction.None
                },
                
                // Monster Placement
                new TutorialManager.TutorialStep
                {
                    stepId = "monster_placement",
                    title = "Place Your Monsters",
                    description = "Select a monster from your hand and place it on the grid. Monsters will automatically attack enemies that come within range.",
                    icon = monsterIcon,
                    requiredAction = TutorialManager.TutorialAction.PlaceMonster
                },
                
                // Card Usage
                new TutorialManager.TutorialStep
                {
                    stepId = "card_usage",
                    title = "Use Your Cards",
                    description = "Cards can provide powerful effects. Select a card from your hand and use it strategically to support your monsters.",
                    icon = cardIcon,
                    requiredAction = TutorialManager.TutorialAction.UseCard
                },
                
                // Wave System
                new TutorialManager.TutorialStep
                {
                    stepId = "wave_system",
                    title = "Defend Against Waves",
                    description = "Enemies will come in waves. Click the 'Start Wave' button when you're ready to face them.",
                    icon = waveIcon,
                    requiredAction = TutorialManager.TutorialAction.StartWave
                },
                
                // Wave Completion
                new TutorialManager.TutorialStep
                {
                    stepId = "wave_completion",
                    title = "Complete the Wave",
                    description = "Defeat all enemies in the wave to earn Duel Points. Use these points to place more monsters or upgrade existing ones.",
                    icon = waveIcon,
                    requiredAction = TutorialManager.TutorialAction.CompleteWave
                },
                
                // Monster Upgrades
                new TutorialManager.TutorialStep
                {
                    stepId = "monster_upgrades",
                    title = "Upgrade Your Monsters",
                    description = "Select a placed monster and click the upgrade button to make it stronger. Upgraded monsters deal more damage and have better stats.",
                    icon = upgradeIcon,
                    requiredAction = TutorialManager.TutorialAction.UpgradeMonster
                },
                
                // Victory
                new TutorialManager.TutorialStep
                {
                    stepId = "victory",
                    title = "Victory!",
                    description = "Congratulations! You've completed the tutorial. You're now ready to face real challenges in the game.",
                    icon = victoryIcon,
                    requiredAction = TutorialManager.TutorialAction.None
                }
            };
            
            // Set the tutorial steps in the TutorialManager
            var field = typeof(TutorialManager).GetField("tutorialSteps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(tutorialManager, steps);
            }
            else
            {
                Debug.LogError("Could not find 'tutorialSteps' field in TutorialManager");
            }
        }
    }
} 