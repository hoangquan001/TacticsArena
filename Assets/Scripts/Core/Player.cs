using TacticsArena.Battle;
using UnityEngine;

namespace TacticsArena.Core
{
    public class Player : MonoBehaviour
    {
        [Header("Player Info")]
        public string playerName = "Player";
        public int playerID;

        [Header("Resources")]
        public int health = 100;
        public int gold = 0;
        public int experience = 0;
        public int level = 1;

        [Header("Area")]
        public PlayerArea playerArea;

        [Header("Economy")]
        public int goldPerRound = 5;
        public int interestRate = 10; // Lãi suất mỗi 10 gold


        public void AddGold(int amount)
        {
            gold += amount;
        }

        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                return true;
            }
            return false;
        }

        public void AddExperience(int amount)
        {
            experience += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int requiredExp = GetRequiredExperience(level + 1);
            if (experience >= requiredExp)
            {
                level++;
                experience -= requiredExp;
                Debug.Log($"{playerName} leveled up to {level}!");
            }
        }

        private int GetRequiredExperience(int targetLevel)
        {
            // TFT experience formula
            return targetLevel * 2;
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                health = 0;
                OnPlayerEliminated();
            }
        }

        private void OnPlayerEliminated()
        {
            Debug.Log($"{playerName} has been eliminated!");
            // Handle player elimination
        }

        public int CalculateInterest()
        {
            return gold / interestRate;
        }
    }
}
