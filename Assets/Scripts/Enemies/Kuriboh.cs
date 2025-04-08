using UnityEngine;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Enemies
{
    public class Kuriboh : Enemy
    {
        [Header("Kuriboh Settings")]
        [SerializeField] private float splitHealthThreshold = 0.5f;
        [SerializeField] private GameObject kuribohPrefab;
        [SerializeField] private int maxSplits = 2;
        [SerializeField] private float splitSpeedMultiplier = 1.2f;
        
        private int currentSplits = 0;

        protected override void Start()
        {
            base.Start();
            stats.type = EnemyType.Normal;
        }

        protected override void Die()
        {
            if (currentHealth <= stats.health * splitHealthThreshold && currentSplits < maxSplits)
            {
                Split();
            }
            else
            {
                base.Die();
            }
        }

        private void Split()
        {
            // Spawn two smaller Kuribohs
            for (int i = 0; i < 2; i++)
            {
                Vector3 spawnOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                );
                
                GameObject kuribohObj = Instantiate(
                    kuribohPrefab,
                    transform.position + spawnOffset,
                    Quaternion.identity,
                    transform.parent
                );
                
                Kuriboh newKuriboh = kuribohObj.GetComponent<Kuriboh>();
                if (newKuriboh != null)
                {
                    // Initialize the new Kuriboh with reduced stats
                    EnemyStats newStats = new EnemyStats
                    {
                        health = stats.health * 0.5f,
                        speed = stats.speed * splitSpeedMultiplier,
                        damage = stats.damage * 0.5f,
                        rewardPoints = stats.rewardPoints / 2,
                        type = stats.type
                    };
                    
                    newKuriboh.Initialize(newStats, currentSplits + 1);
                    newKuriboh.SetWaypoints(waypoints);
                }
            }
            
            // Destroy the original Kuriboh
            base.Die();
        }

        public void Initialize(EnemyStats newStats, int splits)
        {
            stats = newStats;
            currentSplits = splits;
            currentHealth = stats.health;
        }

        protected override void Move()
        {
            base.Move();
            
            // Add some bobbing motion
            float bobHeight = Mathf.Sin(Time.time * 5f) * 0.2f;
            transform.position += Vector3.up * bobHeight * Time.deltaTime;
        }
    }
} 