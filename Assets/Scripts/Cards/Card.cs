using UnityEngine;
using System;
using YuGiOh.Managers;

namespace YuGiOh.Cards
{
    public enum CardType
    {
        Monster,
        Spell,
        Trap
    }

    [Serializable]
    public class CardData
    {
        public string cardName;
        public string description;
        public int cost;
        public Sprite cardImage;
        public float cooldown;
    }

    public abstract class Card : MonoBehaviour
    {
        [SerializeField] protected CardData cardData;
        [SerializeField] protected CardType cardType;
        
        protected float currentCooldown;
        protected bool isActive;
        protected bool isActivated;

        public event Action<Card> OnCardActivated;
        public event Action<Card> OnCardDestroyed;

        protected virtual void Start()
        {
            currentCooldown = 0f;
            isActive = true;
            isActivated = false;
            
            // Register with GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterCard(this);
            }
            else
            {
                Debug.LogWarning("GameManager instance not found!");
            }
        }

        protected virtual void Update()
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
            }
        }

        public virtual bool CanActivate()
        {
            return isActive && !isActivated && currentCooldown <= 0 && 
                   GameManager.Instance.CanAffordMonster(cardData.cost);
        }

        public virtual void Activate()
        {
            if (!CanActivate())
            {
                return;
            }

            isActivated = true;
            currentCooldown = cardData.cooldown;
            GameManager.Instance.SpendDuelPoints(cardData.cost);
            
            ApplyEffect();
            
            OnCardActivated?.Invoke(this);
        }

        protected abstract void ApplyEffect();

        public virtual void Deactivate()
        {
            isActivated = false;
        }

        public virtual void Destroy()
        {
            isActive = false;
            OnCardDestroyed?.Invoke(this);
            GameManager.Instance.UnregisterCard(this);
            Destroy(gameObject);
        }

        public CardData GetCardData()
        {
            return cardData;
        }

        public CardType GetCardType()
        {
            return cardType;
        }

        public float GetCooldownProgress()
        {
            return currentCooldown / cardData.cooldown;
        }

        public bool IsActivated()
        {
            return isActivated;
        }
        
        protected virtual void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterCard(this);
            }
        }
    }
} 