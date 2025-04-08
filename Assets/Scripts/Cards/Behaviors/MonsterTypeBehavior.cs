using UnityEngine;
using System.Collections;
using YuGiOh.Cards;

namespace YuGiOh.Cards.Behaviors
{
    public abstract class MonsterTypeBehavior : MonoBehaviour
    {
        [Header("Base Monster Properties")]
        [SerializeField] protected MonsterCard monsterCard;
        [SerializeField] protected float specialAbilityCooldown = 5f;
        [SerializeField] protected float specialAbilityRange = 10f;
        [SerializeField] protected GameObject specialEffectPrefab;
        [SerializeField] protected AudioClip specialSound;
        
        protected bool isSpecialAbilityReady = true;
        protected float lastSpecialAbilityTime;
        protected bool isSpecialAbilityActive = false;
        
        protected virtual void Start()
        {
            if (monsterCard == null)
            {
                monsterCard = GetComponent<MonsterCard>();
                if (monsterCard == null)
                {
                    Debug.LogError("MonsterTypeBehavior requires a MonsterCard component!");
                    enabled = false;
                    return;
                }
            }
            
            lastSpecialAbilityTime = -specialAbilityCooldown;
        }
        
        protected virtual void Update()
        {
            if (Time.time - lastSpecialAbilityTime >= specialAbilityCooldown)
            {
                isSpecialAbilityReady = true;
            }
            
            if (isSpecialAbilityReady && monsterCard.GetCurrentTarget() != null)
            {
                ApplySpecialAbilityEffects();
                lastSpecialAbilityTime = Time.time;
                isSpecialAbilityReady = false;
            }
        }
        
        protected abstract void ApplySpecialAbilityEffects();
        
        protected abstract void RemoveSpecialAbilityEffects();
        
        public virtual void OnMonsterSpawned()
        {
            // Base implementation - can be overridden by specific behaviors
        }
        
        public virtual void OnMonsterDefeated()
        {
            RemoveSpecialAbilityEffects();
        }
        
        protected bool IsTargetInRange(Transform target)
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.position) <= specialAbilityRange;
        }
        
        protected void PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, rotation, parent);
                Destroy(effect, 2f);
            }
        }
        
        protected void PlaySound(AudioClip sound, Vector3 position)
        {
            if (sound != null)
            {
                AudioSource.PlayClipAtPoint(sound, position);
            }
        }
        
        protected void ApplyDamageToTarget(Transform target, int damage)
        {
            if (target != null)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
        
        protected void ApplyDamageInRadius(Vector3 center, float radius, int damage, LayerMask targetLayer)
        {
            Collider[] colliders = Physics.OverlapSphere(center, radius, targetLayer);
            foreach (Collider collider in colliders)
            {
                ApplyDamageToTarget(collider.transform, damage);
            }
        }
        
        protected void ApplyBuffToTarget(Transform target, float duration, float healthMultiplier, float speedMultiplier)
        {
            if (target != null)
            {
                MonsterCard monster = target.GetComponent<MonsterCard>();
                if (monster != null)
                {
                    monster.ModifyStats(healthMultiplier, speedMultiplier);
                    StartCoroutine(RemoveBuffAfterDelay(monster, duration, 1f/healthMultiplier, 1f/speedMultiplier));
                }
            }
        }
        
        protected IEnumerator RemoveBuffAfterDelay(MonsterCard monster, float delay, float healthMultiplier, float speedMultiplier)
        {
            yield return new WaitForSeconds(delay);
            if (monster != null)
            {
                monster.ModifyStats(healthMultiplier, speedMultiplier);
            }
        }
    }
} 