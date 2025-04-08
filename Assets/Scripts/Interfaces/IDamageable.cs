using UnityEngine;

namespace YuGiOh.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
        bool IsAlive();
        void OnDeath();
    }
} 