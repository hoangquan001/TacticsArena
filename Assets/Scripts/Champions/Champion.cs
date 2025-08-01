using UnityEngine;
using System.Collections;
using TacticsArena.Champions;
using System;
using System.Collections.Generic;
using TacticsArena.Core;
using TacticsArena.UI;
using UnityEditor;

namespace TacticsArena.Battle
{
    public enum ChampionState
    {
        Idle,
        Moving,
        NormalAttacking,
        SkillAttacking,
        Dead
    }
    [CustomEditor(typeof(Champion))]
    public class ChampionInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Champion champion = (Champion)target;

            if (GUILayout.Button("Reset Stats"))
            {
                champion.SetAnimatorTrigger("Attack");
            }
        }
    }

    public class Champion : MonoBehaviour
    {
        public int teamId = 0; // 0 = player, 1 = enemy
        public PlayerArea ownerArea;
        public ChampionData data;
        public int level = 1;
        public int stars = 1;
        public float currentHealth;
        public float currentMana;
        public bool isAlive = true;
        public Champion currentTarget;
        public ChampionState currentState = ChampionState.Idle;

        [Header("Config")]
        public float attackRange = 1.5f;
        public float moveSpeed = 2f;
        public float attackCooldown = 1f;
        public float skillCooldown = 5f;

        private Animator _animator;
        private Coroutine attackCoroutine;
        private Coroutine moveCoroutine;
        private Coroutine stateCoroutine;
        private float lastAttackTime;
        private float lastSkillTime;
        private Vector3 homePosition;
        private string idleAnimName = "Idle";
        private string moveAnimName = "Move";
        private string attackAnimName = "Attack";
        private string skillAnimName = "Skill";
        private string deathAnimName = "Death";
        private AttributeManager attributes;

        private void Start()
        {
            attributes = GetComponent<AttributeManager>();
            attributes?.InitChampion(this);
            _animator = GetComponent<Animator>();
            InitializeChampion();
            homePosition = transform.position;
            ChangeState(ChampionState.Idle);
        }

        public void InitializeChampion()
        {
            if (data == null) return;

            // Initialize stats from data
            currentHealth = data.baseAttr.health;
            currentMana = 0f;
            isAlive = true;

            // Setup animator parameters
            SetupAnimatorParameters();
            SetupAnimatorOverrides();
        }

        private void SetupAnimatorParameters()
        {
            if (_animator == null) return;

            // Ensure animator has required parameters
            // These should match your Animator Controller parameters
            if (!HasParameter("State"))
                Debug.LogWarning($"Animator missing 'State' parameter for {gameObject.name}");
            if (!HasParameter("IsMoving"))
                Debug.LogWarning($"Animator missing 'IsMoving' parameter for {gameObject.name}");
            if (!HasParameter("Attack"))
                Debug.LogWarning($"Animator missing 'Attack' trigger for {gameObject.name}");
            if (!HasParameter("Skill"))
                Debug.LogWarning($"Animator missing 'Skill' trigger for {gameObject.name}");
            if (!HasParameter("Death"))
                Debug.LogWarning($"Animator missing 'Death' trigger for {gameObject.name}");
        }

        private bool HasParameter(string paramName)
        {
            if (_animator == null) return false;

            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        public void ChangeState(ChampionState newState)
        {
            if (currentState == newState) return;

            // Exit current state
            ExitState(currentState);

            // Change state
            ChampionState previousState = currentState;
            currentState = newState;

            // Enter new state
            EnterState(newState, previousState);

            Debug.Log(message: $"{data?.championName ?? gameObject.name} changed state from {previousState} to {newState}");
        }

        private void EnterState(ChampionState state, ChampionState previousState)
        {
            switch (state)
            {
                case ChampionState.Idle:
                    EnterIdleState();
                    break;
                case ChampionState.Moving:
                    EnterMovingState();
                    break;
                case ChampionState.NormalAttacking:
                    EnterNormalAttackState();
                    break;
                case ChampionState.SkillAttacking:
                    EnterSkillAttackState();
                    break;
                case ChampionState.Dead:
                    EnterDeadState();
                    break;
            }
        }

        private void ExitState(ChampionState state)
        {
            // Stop any running coroutines for the current state
            if (stateCoroutine != null)
            {
                StopCoroutine(stateCoroutine);
                stateCoroutine = null;
            }

            switch (state)
            {
                case ChampionState.Moving:
                    ExitMovingState();
                    break;
                case ChampionState.NormalAttacking:
                    ExitNormalAttackState();
                    break;
                case ChampionState.SkillAttacking:
                    ExitSkillAttackState();
                    break;
            }
        }

        #region State Handlers

        private void EnterIdleState()
        {
            PlayAnimation(idleAnimName);
            SetAnimatorBool("IsMoving", false);
            SetAnimatorInt("State", 0); // Idle = 0
        }

        private void EnterMovingState()
        {
            PlayAnimation(moveAnimName);
            SetAnimatorBool("IsMoving", true);
            SetAnimatorInt("State", 1); // Moving = 1
        }

        private void ExitMovingState()
        {
            SetAnimatorBool("IsMoving", false);
        }

        private void EnterNormalAttackState()
        {
            SetAnimatorInt("State", 2); // Attack = 2
            SetAnimatorTrigger("Attack");
            stateCoroutine = StartCoroutine(NormalAttackCoroutine());
        }

        private void ExitNormalAttackState()
        {
            // Attack exit is handled by coroutine completion
        }

        private void EnterSkillAttackState()
        {
            SetAnimatorInt("State", 3); // Skill = 3
            SetAnimatorTrigger("Skill");
            stateCoroutine = StartCoroutine(SkillAttackCoroutine());
        }

        private void ExitSkillAttackState()
        {
            // Skill exit is handled by coroutine completion
        }

        private void EnterDeadState()
        {
            SetAnimatorInt("State", 4); // Dead = 4
            SetAnimatorTrigger("Death");
            PlayAnimation(deathAnimName);

            // Stop all other coroutines
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
        }

        #endregion

        #region Animation Helpers
        private void SetupAnimatorOverrides()
        {
            if (_animator == null || data == null) return;
            // Override animations based on ChampionData
            // OverrideAnimation("Idle", data.animations.idleAnimation);
            // OverrideAnimation("Move", data.animations.moveAnimation);
            // OverrideAnimation("Attack", data.animations.attackAnimation);
            // OverrideAnimation("Skill", data.animations.abilityAnimation);
            // OverrideAnimation("Death", data.animations.deathAnimation);
            OverrideAnimations();
        }
        private void OverrideAnimations()
        {
            var overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);

            foreach (var clip in overrideController.runtimeAnimatorController.animationClips)
            {
                if (clip.name == idleAnimName && data.animations.idleAnimation != null)
                    overrideController[clip] = data.animations.idleAnimation;
                else if (clip.name == moveAnimName && data.animations.moveAnimation != null)
                    overrideController[clip] = data.animations.moveAnimation;
                else if (clip.name == attackAnimName && data.animations.attackAnimation != null)
                    overrideController[clip] = data.animations.attackAnimation;
                else if (clip.name == skillAnimName && data.animations.abilityAnimation != null)
                    overrideController[clip] = data.animations.abilityAnimation;
                else if (clip.name == deathAnimName && data.animations.deathAnimation != null)
                    overrideController[clip] = data.animations.deathAnimation;
            }

            _animator.runtimeAnimatorController = overrideController;
        }

        public void PlayAnimation(string animationName)
        {
            if (_animator != null && !string.IsNullOrEmpty(animationName))
            {
                _animator.Play(animationName);
            }
        }

        private void SetAnimatorInt(string paramName, int value)
        {
            if (_animator != null && HasParameter(paramName))
            {
                _animator.SetInteger(paramName, value);
            }
        }

        private void SetAnimatorBool(string paramName, bool value)
        {
            if (_animator != null && HasParameter(paramName))
            {
                _animator.SetBool(paramName, value);
            }
        }

        public void SetAnimatorTrigger(string paramName)
        {
            if (_animator != null && HasParameter(paramName))
            {
                _animator.SetTrigger(paramName);
            }
        }

        #endregion

        #region Attack Coroutines

        private IEnumerator NormalAttackCoroutine()
        {
            if (currentTarget == null || !currentTarget.isAlive)
            {
                ChangeState(ChampionState.Idle);
                yield break;
            }

            // Wait for animation to reach attack point (usually mid-animation)
            yield return new WaitForSeconds(0.3f);

            // Perform the actual attack
            PerformNormalAttack(currentTarget);

            // Wait for attack animation to complete
            yield return new WaitForSeconds(0.7f);

            // Update last attack time
            lastAttackTime = Time.time;

            // Return to idle or continue combat
            if (currentTarget != null && currentTarget.isAlive)
            {
                ChangeState(ChampionState.Idle);
            }
            else
            {
                ChangeState(ChampionState.Idle);
            }
        }

        private IEnumerator SkillAttackCoroutine()
        {
            if (currentTarget == null || !currentTarget.isAlive)
            {
                ChangeState(ChampionState.Idle);
                yield break;
            }

            Debug.Log($"{data?.championName} is casting skill!");

            // Wait for skill animation to reach cast point
            yield return new WaitForSeconds(0.5f);

            // Perform the skill attack
            PerformSkillAttack(currentTarget);

            // Wait for skill animation to complete
            yield return new WaitForSeconds(1f);

            // Update last skill time
            lastSkillTime = Time.time;

            // Return to idle
            ChangeState(ChampionState.Idle);
        }

        #endregion

        private float GetStarMultiplier()
        {
            switch (stars)
            {
                case 1: return 1f;
                case 2: return 1.8f;
                case 3: return 3.24f;
                default: return 1f;
            }
        }

        public void StartBattle()
        {
            if (!isAlive) return;

            attackCoroutine = StartCoroutine(BattleLoop());
        }

        private IEnumerator BattleLoop()
        {
            while (isAlive && currentHealth > 0)
            {
                // Wait if currently in an action state
                if (currentState == ChampionState.NormalAttacking ||
                    currentState == ChampionState.SkillAttacking ||
                    currentState == ChampionState.Dead)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                // Find target
                currentTarget = FindNearestEnemy();

                if (currentTarget != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

                    // Move to target if too far
                    if (distanceToTarget > attackRange)
                    {
                        if (currentState != ChampionState.Moving)
                        {
                            ChangeState(ChampionState.Moving);
                        }
                        yield return StartCoroutine(MoveToTarget(currentTarget));
                    }
                    else
                    {
                        // In attack range - decide between normal attack or skill
                        if (CanUseSkill())
                        {
                            ChangeState(ChampionState.SkillAttacking);
                            // Wait for skill to complete
                            while (currentState == ChampionState.SkillAttacking)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                        }
                        else if (CanAttack())
                        {
                            ChangeState(ChampionState.NormalAttacking);
                            // Wait for attack to complete
                            while (currentState == ChampionState.NormalAttacking)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                        }
                        else
                        {
                            // Can't attack yet, wait in idle
                            if (currentState != ChampionState.Idle)
                            {
                                ChangeState(ChampionState.Idle);
                            }
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else
                {
                    // No target found, go idle
                    if (currentState != ChampionState.Idle)
                    {
                        ChangeState(ChampionState.Idle);
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        #region Combat Helpers

        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        private bool CanUseSkill()
        {
            return currentMana >= data.baseAttr.maxMana &&
                   Time.time >= lastSkillTime + skillCooldown;
        }

        private void PerformNormalAttack(Champion target)
        {
            if (target == null || !target.isAlive) return;

            float damage = CalculateDamage(data.baseAttr.attackDamage, target.data.baseAttr.armor);
            target.TakeDamage(damage);

            // Add mana for attacking
            AddManaAmount(10f);

            Debug.Log($"{data.championName} attacks {target.data.championName} for {damage} damage");
        }

        private void PerformSkillAttack(Champion target)
        {
            if (target == null || !target.isAlive || data.abilityData == null) return;

            float skillDamage = data.abilityData.damage;
            target.TakeDamage(skillDamage);

            // Reset mana after using skill
            currentMana = 0f;

            Debug.Log($"{data.championName} uses skill on {target.data.championName} for {skillDamage} damage!");
        }

        #endregion

        private Champion FindNearestEnemy()
        {
            // Tìm enemy gần nhất
            Champion[] allChampions = FindObjectsByType<Champion>(FindObjectsSortMode.None);
            Champion nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Champion champion in allChampions)
            {
                if (champion != this && champion.isAlive && IsEnemy(champion))
                {
                    float distance = Vector3.Distance(transform.position, champion.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearest = champion;
                        nearestDistance = distance;
                    }
                }
            }

            return nearest;
        }

        private bool IsEnemy(Champion other)
        {
            // Kiểm tra team ID
            return other.teamId != this.teamId;
        }

        private IEnumerator MoveToTarget(Champion target)
        {
            while (target != null && target.isAlive &&
                   Vector3.Distance(transform.position, target.transform.position) > attackRange)
            {
                // Stop moving if we're not in moving state anymore
                if (currentState != ChampionState.Moving)
                {
                    break;
                }

                Vector3 direction = (target.transform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                // Face the target
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                yield return null;
            }

            // Finished moving, go to idle if still in moving state
            if (currentState == ChampionState.Moving)
            {
                ChangeState(ChampionState.Idle);
            }
        }

        private void PerformAttack(Champion target)
        {
            if (target == null || !target.isAlive) return;

            float damage = CalculateDamage(data.baseAttr.attackDamage, target.data.baseAttr.armor);
            target.TakeDamage(damage);

            Debug.Log($"{data.championName} attacks {target.data.championName} for {damage} damage");
        }

        private float CalculateDamage(float baseDamage, float targetArmor)
        {
            // Công thức damage reduction
            float damageReduction = targetArmor / (targetArmor + 100);
            return baseDamage * (1 - damageReduction);
        }

        public void TakeDamage(float damage, bool isCritical = false)
        {
            if (!isAlive || currentState == ChampionState.Dead) return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }

            // Trigger HUD update event
            OnHealthChanged?.Invoke(currentHealth);

            // Debug log for now (can be replaced with floating text later)
            Debug.Log($"{data?.championName ?? gameObject.name} took {damage} damage{(isCritical ? " (CRITICAL)" : "")}");
        }

        public void Heal(float healAmount)
        {
            if (!isAlive || currentState == ChampionState.Dead) return;

            float maxHealth = data != null ? data.baseAttr.health : 100f;
            float actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);

            if (actualHeal > 0)
            {
                currentHealth += actualHeal;

                // Trigger HUD update event
                OnHealthChanged?.Invoke(currentHealth);

                // Debug log for now (can be replaced with floating text later)
                Debug.Log($"{data?.championName ?? gameObject.name} healed {actualHeal} HP");
            }
        }

        public void AddManaAmount(float manaAmount)
        {
            if (!isAlive || currentState == ChampionState.Dead) return;

            float maxMana = data != null ? data.baseAttr.maxMana : 100f;
            currentMana = Mathf.Min(currentMana + manaAmount, maxMana);

            // Trigger HUD update event
            OnManaChanged?.Invoke(currentMana);
        }

        public void SpendMana(float manaAmount)
        {
            if (!isAlive || currentState == ChampionState.Dead) return;

            currentMana = Mathf.Max(0, currentMana - manaAmount);

            // Trigger HUD update event
            OnManaChanged?.Invoke(currentMana);
        }

        // Events for HUD updates
        public System.Action<float> OnHealthChanged;
        public System.Action<float> OnManaChanged;

        private void Die()
        {
            if (!isAlive) return; // Prevent multiple death calls

            isAlive = false;
            ChangeState(ChampionState.Dead);

            Debug.Log($"{data.championName} has died!");

            // Stop all coroutines
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            // Disable after death animation (optional)
            StartCoroutine(DisableAfterDeath());
        }

        private IEnumerator DisableAfterDeath()
        {
            // Wait for death animation to play
            yield return new WaitForSeconds(2f);

            // Optionally disable the gameobject or just keep it inactive
            // gameObject.SetActive(false);
        }

        private void AddMana(float amount)
        {
            if (!isAlive || currentState == ChampionState.Dead) return;

            currentMana += amount;

            // Clamp mana to max value
            if (currentMana > data.baseAttr.maxMana)
            {
                currentMana = data.baseAttr.maxMana;
            }
        }

        #region Public Interface Methods

        public void ForceIdle()
        {
            if (isAlive && currentState != ChampionState.Dead)
            {
                ChangeState(ChampionState.Idle);
            }
        }

        public void StopBattle()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            ForceIdle();
        }

        public bool IsInCombat()
        {
            return currentState == ChampionState.NormalAttacking ||
                   currentState == ChampionState.SkillAttacking ||
                   currentState == ChampionState.Moving;
        }

        public float GetHealthPercentage()
        {
            if (data == null) return 0f;
            return currentHealth / data.baseAttr.health;
        }

        public float GetManaPercentage()
        {
            if (data == null) return 0f;
            return currentMana / data.baseAttr.maxMana;
        }

        #endregion

        internal void EndBattle()
        {
            StopBattle();

            // Reset position to home if needed
            if (homePosition != Vector3.zero)
            {
                transform.position = homePosition;
            }

            // Reset some combat stats
            currentTarget = null;

            Debug.Log($"{data?.championName ?? gameObject.name} ended battle");
        }

        public AttributeData GetBaseAttribute()
        {
            return data.baseAttr;
        }
    }
}
