using UnityEngine;
using System.Collections;
using TacticsArena.Champions;
using System;

namespace TacticsArena.Battle
{
    public class Champion : MonoBehaviour
    {
        [Header("Champion Data")]
        public ChampionData championData;
        public int level = 1;
        public int stars = 1;

        [Header("Current Stats")]
        public float currentHealth;
        public float maxHealth;
        public float attackDamage;
        public float attackSpeed;
        public float armor;
        public float magicResist;
        public float currentMana;
        public float maxMana;

        [Header("Battle State")]
        public bool isAlive = true;
        public Champion currentTarget;
        public Vector3 homePosition;

        [Header("Combat")]
        public float attackRange = 1.5f;
        public float moveSpeed = 2f;

        private Cell currentCell; // Ô hiện tại của champion, sẽ được cập nhật khi đặt champion vào ô
        public Cell  CurCell
        {
            get { return currentCell; }
            set
            {
                currentCell = value;
                if (currentCell != null)
                {
                    transform.position = currentCell.transform.position; // Đặt champion lên ô
                }
            }
        }

        public bool IsPlaced()
        {
            return currentCell != null;
        }

        private Coroutine attackCoroutine;
        private Coroutine moveCoroutine;

        private void Start()
        {
            InitializeChampion();
        }

        public void InitializeChampion()
        {
            if (championData == null) return;

            // Tính stats dựa trên level và stars
            maxHealth = championData.GetStatAtLevel(championData.baseHealth, level) * GetStarMultiplier();
            currentHealth = maxHealth;
            attackDamage = championData.GetStatAtLevel(championData.baseAttackDamage, level) * GetStarMultiplier();
            attackSpeed = championData.baseAttackSpeed;
            armor = championData.baseArmor;
            magicResist = championData.baseMagicResist;
            maxMana = championData.maxMana;
            currentMana = 0f;

            homePosition = transform.position;
        }

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
                // Tìm target
                currentTarget = FindNearestEnemy();

                if (currentTarget != null)
                {
                    // Di chuyển đến target nếu cần
                    if (Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
                    {
                        yield return StartCoroutine(MoveToTarget(currentTarget));
                    }

                    // Tấn công
                    if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRange)
                    {
                        PerformAttack(currentTarget);
                        AddMana(10f); // Mỗi lần attack được mana

                        // Chờ attack speed
                        yield return new WaitForSeconds(1f / attackSpeed);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private Champion FindNearestEnemy()
        {
            // Tìm enemy gần nhất (cần implement logic team)
            Champion[] allChampions = FindObjectsOfType<Champion>();
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
            // Logic xác định enemy (tạm thời return true)
            return true;
        }

        private IEnumerator MoveToTarget(Champion target)
        {
            while (target != null && Vector3.Distance(transform.position, target.transform.position) > attackRange)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                yield return null;
            }
        }

        private void PerformAttack(Champion target)
        {
            if (target == null || !target.isAlive) return;

            float damage = CalculateDamage(attackDamage, target.armor);
            target.TakeDamage(damage);

            Debug.Log($"{championData.championName} attacks {target.championData.championName} for {damage} damage");
        }

        private float CalculateDamage(float baseDamage, float targetArmor)
        {
            // Công thức damage reduction
            float damageReduction = targetArmor / (targetArmor + 100);
            return baseDamage * (1 - damageReduction);
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            isAlive = false;

            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);

            Debug.Log($"{championData.championName} has died!");

            // Visual effects, disable gameobject, etc.
            gameObject.SetActive(false);
        }

        private void AddMana(float amount)
        {
            currentMana += amount;
            if (currentMana >= maxMana)
            {
                currentMana = 0;
                CastAbility();
            }
        }

        private void CastAbility()
        {
            if (championData == null) return;

            Debug.Log($"{championData.championName} casts {championData.abilityName}!");

            // Implement ability logic
            if (currentTarget != null)
            {
                float abilityDamage = championData.abilityDamage;
                currentTarget.TakeDamage(abilityDamage);
            }
        }

        internal void EndBattle()
        {
            throw new NotImplementedException();
        }
    }
}
