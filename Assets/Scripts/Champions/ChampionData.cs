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
        
        public AttributeData baseAttr = new AttributeData();

        [Header("Ability")]
        public SkillData abilityData;

        [Header("Animations")]
        public AnimationClip idleAnimation;
        public AnimationClip moveAnimation;
        public AnimationClip attackAnimation;
        public AnimationClip abilityAnimation;
        public AnimationClip deathAnimation;
        
        public float GetStatAtLevel(float baseStat, int level)
        {
            // Công thức scale stats theo level
            return baseStat * (1 + (level - 1) * 0.8f);
        }
    }
}
