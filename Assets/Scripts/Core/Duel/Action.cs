using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Duel
{
    public class Action
    {
        public enum ActionType
        {
            Summon,
            SpellActivation,
            TrapActivation,
            Attack,
            EffectActivation
        }
        
        public ActionType Type { get; private set; }
        public YuGiOhCard Card { get; private set; }
        public GameObject Target { get; private set; }
        public float EffectValue { get; private set; }
        
        private bool isNegated = false;
        private bool isDestroyed = false;
        
        public Action(ActionType type, YuGiOhCard card, GameObject target = null, float effectValue = 0f)
        {
            Type = type;
            Card = card;
            Target = target;
            EffectValue = effectValue;
        }
        
        public void Negate()
        {
            if (isNegated || isDestroyed) return;
            
            isNegated = true;
            
            // Handle negation based on action type
            switch (Type)
            {
                case ActionType.Summon:
                    if (Target != null)
                    {
                        Object.Destroy(Target);
                    }
                    break;
                case ActionType.SpellActivation:
                case ActionType.TrapActivation:
                    if (Card != null)
                    {
                        // Return card to hand or send to graveyard
                        // This will be handled by the DuelManager
                    }
                    break;
                case ActionType.Attack:
                    // Cancel the attack
                    break;
                case ActionType.EffectActivation:
                    // Cancel the effect
                    break;
            }
        }
        
        public void Destroy()
        {
            if (isDestroyed) return;
            
            isDestroyed = true;
            
            // Handle destruction based on action type
            switch (Type)
            {
                case ActionType.Summon:
                    if (Target != null)
                    {
                        Object.Destroy(Target);
                    }
                    break;
                case ActionType.SpellActivation:
                case ActionType.TrapActivation:
                    if (Card != null)
                    {
                        // Send card to graveyard
                        // This will be handled by the DuelManager
                    }
                    break;
                case ActionType.Attack:
                    // Cancel the attack
                    break;
                case ActionType.EffectActivation:
                    // Cancel the effect
                    break;
            }
        }
        
        public bool IsValid()
        {
            return !isNegated && !isDestroyed;
        }
    }
} 