using UnityEngine;
using TacticsArena.Champions;

namespace TacticsArena.Data
{
    /// <summary>
    /// Factory class để tạo sample data cho testing
    /// </summary>
    public static class SampleDataFactory
    {
        public static ChampionData CreateSampleChampion(string name, ChampionClass championClass, ChampionOrigin origin, int tier, int cost)
        {
            var champion = ScriptableObject.CreateInstance<ChampionData>();
            champion.championName = name;
            champion.classes.Add(championClass);
            champion.origins.Add(origin);
            champion.tier = tier;
            champion.cost = cost;
            
            // Set base stats dựa trên tier
            switch (tier)
            {
                case 1:
                    champion.baseAttr.health = 100f;
                    champion.baseAttr.attackDamage = 50f;
                    break;
                case 2:
                    champion.baseAttr.health = 150f;
                    champion.baseAttr.attackDamage = 70f;
                    break;
                case 3:
                    champion.baseAttr.health = 200f;
                    champion.baseAttr.attackDamage = 90f;
                    break;
                case 4:
                    champion.baseAttr.health = 300f;
                    champion.baseAttr.attackDamage = 120f;
                    break;
                case 5:
                    champion.baseAttr.health = 500f;
                    champion.baseAttr.attackDamage = 180f;
                    break;
            }

            champion.baseAttr.attackSpeed = 1f;
            champion.baseAttr.armor = 10f + (tier * 5);
            champion.baseAttr.magicResist = 10f + (tier * 5);
            champion.baseAttr.maxMana = 100f;

            return champion;
        }
        
        public static ChampionData[] CreateSampleChampionPool()
        {
            return new ChampionData[]
            {
                // Tier 1 Champions
                CreateSampleChampion("Warrior", ChampionClass.Warrior, ChampionOrigin.Human, 1, 1),
                CreateSampleChampion("Archer", ChampionClass.Archer, ChampionOrigin.Elf, 1, 1),
                CreateSampleChampion("Mage", ChampionClass.Mage, ChampionOrigin.Human, 1, 1),
                CreateSampleChampion("Orc Warrior", ChampionClass.Warrior, ChampionOrigin.Orc, 1, 1),
                CreateSampleChampion("Beast", ChampionClass.Tank, ChampionOrigin.Beast, 1, 1),
                
                // Tier 2 Champions
                CreateSampleChampion("Knight", ChampionClass.Tank, ChampionOrigin.Human, 2, 2),
                CreateSampleChampion("Elf Ranger", ChampionClass.Archer, ChampionOrigin.Elf, 2, 2),
                CreateSampleChampion("Fire Mage", ChampionClass.Mage, ChampionOrigin.Human, 2, 2),
                CreateSampleChampion("Orc Shaman", ChampionClass.Mage, ChampionOrigin.Orc, 2, 2),
                CreateSampleChampion("Assassin", ChampionClass.Assassin, ChampionOrigin.Elf, 2, 2),
                
                // Tier 3 Champions
                CreateSampleChampion("Paladin", ChampionClass.Tank, ChampionOrigin.Human, 3, 3),
                CreateSampleChampion("Archmage", ChampionClass.Mage, ChampionOrigin.Elf, 3, 3),
                CreateSampleChampion("Dragon Knight", ChampionClass.Warrior, ChampionOrigin.Dragon, 3, 3),
                CreateSampleChampion("Lich", ChampionClass.Mage, ChampionOrigin.Undead, 3, 3),
                
                // Tier 4 Champions
                CreateSampleChampion("Dragon", ChampionClass.Mage, ChampionOrigin.Dragon, 4, 4),
                CreateSampleChampion("Demon Lord", ChampionClass.Assassin, ChampionOrigin.Undead, 4, 4),
                
                // Tier 5 Champions
                CreateSampleChampion("Ancient Dragon", ChampionClass.Mage, ChampionOrigin.Dragon, 5, 5)
            };
        }
    }
}
