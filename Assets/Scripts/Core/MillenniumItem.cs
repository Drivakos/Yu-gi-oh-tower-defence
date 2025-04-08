using UnityEngine;
using System.Collections.Generic;
using System;

namespace YuGiOhTowerDefense.Core
{
    public enum MillenniumItemType
    {
        Ring,
        Puzzle,
        Eye,
        Rod,
        Key,
        Scale,
        Necklace
    }

    [CreateAssetMenu(fileName = "New Millennium Item", menuName = "YuGiOh/Millennium Item")]
    public class MillenniumItem : ScriptableObject
    {
        [Header("Item Information")]
        [SerializeField] private string itemName;
        [SerializeField] private MillenniumItemType itemType;
        [SerializeField] private Sprite itemIcon;
        [SerializeField] [TextArea(3, 5)] private string description;
        [SerializeField] private bool isCollected;

        [Header("Item Effects")]
        [SerializeField] private float cooldown = 60f;
        [SerializeField] private float effectDuration = 10f;
        [SerializeField] private bool requiresActivation = true;

        [Header("Special Abilities")]
        [SerializeField] private bool canSeeInvisible = false;
        [SerializeField] private bool canControlMonsters = false;
        [SerializeField] private bool canManipulateTime = false;
        [SerializeField] private bool canReadMinds = false;
        [SerializeField] private bool canProtectFromShadow = false;
        [SerializeField] private bool canBanishToShadow = false;
        [SerializeField] private bool canResurrectMonsters = false;

        [Header("Effect Modifiers")]
        [SerializeField] private float damageMultiplier = 1.5f;
        [SerializeField] private float defenseMultiplier = 1.5f;
        [SerializeField] private float recruitmentBonus = 0.2f;
        [SerializeField] private int extraCardDraw = 1;

        private float lastActivationTime;
        private bool isActive;

        public string ItemName => itemName;
        public MillenniumItemType ItemType => itemType;
        public Sprite ItemIcon => itemIcon;
        public string Description => description;
        public bool IsCollected => isCollected;
        public bool IsActive => isActive;
        public float Cooldown => cooldown;
        public float EffectDuration => effectDuration;
        public bool RequiresActivation => requiresActivation;
        public float RemainingCooldown => Mathf.Max(0, cooldown - (Time.time - lastActivationTime));

        public void Collect()
        {
            isCollected = true;
            Debug.Log($"Collected {itemName}!");
        }

        public bool CanActivate()
        {
            if (!isCollected) return false;
            if (!requiresActivation) return true;
            return Time.time - lastActivationTime >= cooldown;
        }

        public void Activate()
        {
            if (!CanActivate()) return;

            isActive = true;
            lastActivationTime = Time.time;
            Debug.Log($"Activated {itemName}!");

            // Schedule deactivation
            if (effectDuration > 0)
            {
                Invoke(nameof(Deactivate), effectDuration);
            }
        }

        public void Deactivate()
        {
            if (!isActive) return;

            isActive = false;
            Debug.Log($"Deactivated {itemName}!");
        }

        public float GetDamageMultiplier()
        {
            return isActive ? damageMultiplier : 1f;
        }

        public float GetDefenseMultiplier()
        {
            return isActive ? defenseMultiplier : 1f;
        }

        public float GetRecruitmentBonus()
        {
            return isActive ? recruitmentBonus : 0f;
        }

        public int GetExtraCardDraw()
        {
            return isActive ? extraCardDraw : 0;
        }

        public bool CanSeeInvisible()
        {
            return isActive && canSeeInvisible;
        }

        public bool CanControlMonsters()
        {
            return isActive && canControlMonsters;
        }

        public bool CanManipulateTime()
        {
            return isActive && canManipulateTime;
        }

        public bool CanReadMinds()
        {
            return isActive && canReadMinds;
        }

        public bool CanProtectFromShadow()
        {
            return isActive && canProtectFromShadow;
        }

        public bool CanBanishToShadow()
        {
            return isActive && canBanishToShadow;
        }

        public bool CanResurrectMonsters()
        {
            return isActive && canResurrectMonsters;
        }
    }
} 