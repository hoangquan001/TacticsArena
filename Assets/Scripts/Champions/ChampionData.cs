using UnityEngine;
using System.Collections.Generic;

namespace TacticsArena.Champions
{
    [System.Serializable]
    public enum ChampionClass
    {
        Assassin,
        Mage,
        Warrior,
        Archer,
        Support,
        Tank
    }
    
    [System.Serializable]
    public enum ChampionOrigin
    {
        Human,
        Elf,
        Orc,
        Undead,
        Dragon,
        Beast
    }
    
    [CreateAssetMenu(fileName = "New Champion Data", menuName = "TacticsArena/Champion Data")]
    public class ChampionData : ScriptableObject
    {
        [Header("Basic Info")]
        public string championName;
        public Sprite championIcon;
        public GameObject championPrefab;
        public int cost = 1;
        public int tier = 1;
        
        [Header("Classes & Origins")]
        public List<ChampionClass> classes = new List<ChampionClass>();
        public List<ChampionOrigin> origins = new List<ChampionOrigin>();
        
        [Header("Base Stats")]
        public float baseHealth = 100f;
        public float baseAttackDamage = 50f;
        public float baseAttackSpeed = 1f;
        public float baseArmor = 10f;
        public float baseMagicResist = 10f;
        public float baseMana = 100f;
        public float maxMana = 100f;
        
        [Header("Ability")]
        public string abilityName;
        public string abilityDescription;
        public float abilityDamage = 100f;
        public float abilityCooldown = 3f;
        
        public float GetStatAtLevel(float baseStat, int level)
        {
            // Công thức scale stats theo level
            return baseStat * (1 + (level - 1) * 0.8f);
        }
    }
}
