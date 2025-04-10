using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.StateMachine
{
    public interface IMonsterState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public class MonsterStateMachine
    {
        private readonly MonsterCard monster;
        private IMonsterState currentState;
        private readonly Dictionary<MonsterState, IMonsterState> states;

        public MonsterStateMachine(MonsterCard monster)
        {
            this.monster = monster;
            states = new Dictionary<MonsterState, IMonsterState>
            {
                { MonsterState.Idle, new MonsterIdleState(monster) },
                { MonsterState.Moving, new MonsterMovingState(monster) },
                { MonsterState.Attacking, new MonsterAttackingState(monster) },
                { MonsterState.Dead, new MonsterDeadState(monster) }
            };
        }

        public void SetState(MonsterState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            currentState = states[newState];
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }

        private class MonsterIdleState : IMonsterState
        {
            private readonly MonsterCard monster;

            public MonsterIdleState(MonsterCard monster)
            {
                this.monster = monster;
            }

            public void Enter()
            {
                if (monster.Animator != null)
                {
                    monster.Animator.SetBool("IsMoving", false);
                }

                if (monster.IdleParticles != null)
                {
                    monster.IdleParticles.Play();
                }
            }

            public void Update()
            {
                monster.FindNearestEnemy();

                if (monster.Target != null)
                {
                    float distance = Vector3.Distance(monster.transform.position, monster.Target.position);

                    if (distance <= monster.AttackRange && monster.AttackTimer <= 0)
                    {
                        monster.SetState(MonsterState.Attacking);
                    }
                    else if (distance > monster.AttackRange)
                    {
                        monster.SetState(MonsterState.Moving);
                    }
                }
            }

            public void Exit()
            {
                if (monster.IdleParticles != null)
                {
                    monster.IdleParticles.Stop();
                }
            }
        }

        private class MonsterMovingState : IMonsterState
        {
            private readonly MonsterCard monster;

            public MonsterMovingState(MonsterCard monster)
            {
                this.monster = monster;
            }

            public void Enter()
            {
                if (monster.Animator != null)
                {
                    monster.Animator.SetBool("IsMoving", true);
                }
            }

            public void Update()
            {
                if (monster.Target == null)
                {
                    monster.SetState(MonsterState.Idle);
                    return;
                }

                float distance = Vector3.Distance(monster.transform.position, monster.Target.position);

                if (distance <= monster.AttackRange && monster.AttackTimer <= 0)
                {
                    monster.SetState(MonsterState.Attacking);
                }
                else if (distance > monster.AttackRange)
                {
                    monster.MoveTowardsTarget();
                }
            }

            public void Exit()
            {
                if (monster.Animator != null)
                {
                    monster.Animator.SetBool("IsMoving", false);
                }
            }
        }

        private class MonsterAttackingState : IMonsterState
        {
            private readonly MonsterCard monster;

            public MonsterAttackingState(MonsterCard monster)
            {
                this.monster = monster;
            }

            public void Enter()
            {
                monster.AttackTarget();
            }

            public void Update()
            {
                if (monster.Target == null)
                {
                    monster.SetState(MonsterState.Idle);
                    return;
                }

                float distance = Vector3.Distance(monster.transform.position, monster.Target.position);

                if (distance > monster.AttackRange)
                {
                    monster.SetState(MonsterState.Moving);
                }
                else if (monster.AttackTimer <= 0)
                {
                    monster.AttackTarget();
                }
            }

            public void Exit()
            {
                // Nothing to do here
            }
        }

        private class MonsterDeadState : IMonsterState
        {
            private readonly MonsterCard monster;

            public MonsterDeadState(MonsterCard monster)
            {
                this.monster = monster;
            }

            public void Enter()
            {
                monster.Die();
            }

            public void Update()
            {
                // Nothing to do here
            }

            public void Exit()
            {
                // Nothing to do here
            }
        }
    }
} 