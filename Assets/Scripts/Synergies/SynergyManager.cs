using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TacticsArena.Champions;
using TacticsArena.Battle;

namespace TacticsArena.Synergies
{
    [System.Serializable]
    public class SynergyBonus
    {
        public int requiredCount;
        public string bonusDescription;
        public float bonusValue;
    }
    
    [CreateAssetMenu(fileName = "New Synergy", menuName = "TacticsArena/Synergy")]
    public class SynergyData : ScriptableObject
    {
        [Header("Synergy Info")]
        public string synergyName;
        public ChampionClass championClass;
        public ChampionOrigin championOrigin;
        public Sprite synergyIcon;
        
        [Header("Synergy Bonuses")]
        public List<SynergyBonus> bonuses = new List<SynergyBonus>();
        
        public SynergyBonus GetActiveBonus(int championCount)
        {
            SynergyBonus activeBonus = null;
            
            foreach (var bonus in bonuses)
            {
                if (championCount >= bonus.requiredCount)
                    activeBonus = bonus;
            }
            
            return activeBonus;
        }
    }
    
    public class SynergyManager : MonoBehaviour
    {
        [Header("Synergy Data")]
        public List<SynergyData> allSynergies = new List<SynergyData>();
        
        private Dictionary<ChampionClass, int> classCounts = new Dictionary<ChampionClass, int>();
        private Dictionary<ChampionOrigin, int> originCounts = new Dictionary<ChampionOrigin, int>();
        private List<SynergyBonus> activeBonuses = new List<SynergyBonus>();
        
        public void CalculateSynergies(List<Champion> champions)
        {
            // Reset counts
            classCounts.Clear();
            originCounts.Clear();
            activeBonuses.Clear();
            
            // Count champions by class and origin
            foreach (Champion champion in champions)
            {
                if (champion.data == null) continue;
                
                // Count classes
                foreach (ChampionClass championClass in champion.data.classes)
                {
                    if (!classCounts.ContainsKey(championClass))
                        classCounts[championClass] = 0;
                    classCounts[championClass]++;
                }
                
                // Count origins
                foreach (ChampionOrigin origin in champion.data.origins)
                {
                    if (!originCounts.ContainsKey(origin))
                        originCounts[origin] = 0;
                    originCounts[origin]++;
                }
            }
            
            // Calculate active synergies
            foreach (SynergyData synergy in allSynergies)
            {
                int count = 0;
                
                // Check class synergies
                if (classCounts.ContainsKey(synergy.championClass))
                    count = classCounts[synergy.championClass];
                
                // Check origin synergies  
                if (originCounts.ContainsKey(synergy.championOrigin))
                    count = originCounts[synergy.championOrigin];
                
                SynergyBonus activeBonus = synergy.GetActiveBonus(count);
                if (activeBonus != null)
                {
                    activeBonuses.Add(activeBonus);
                    Debug.Log($"Active synergy: {synergy.synergyName} - {activeBonus.bonusDescription}");
                }
            }
            
            // Apply synergy bonuses to champions
            ApplySynergyBonuses(champions);
            
            // Trigger UI update
            SynergiesUpdatedEvent?.Invoke(GetActiveSynergiesInfo());
        }
        
        private void ApplySynergyBonuses(List<Champion> champions)
        {
            // Reset all champions to base stats first
            foreach (Champion champion in champions)
            {
                champion.InitializeChampion();
            }
            
            // Apply synergy bonuses
            foreach (SynergyBonus bonus in activeBonuses)
            {
                ApplyBonusToChampions(bonus, champions);
            }
        }
        
        private void ApplyBonusToChampions(SynergyBonus bonus, List<Champion> champions)
        {
            // Apply bonus based on bonus description/type
            // This is a simplified implementation
            foreach (Champion champion in champions)
            {
                if (bonus.bonusDescription.Contains("Attack"))
                {
                    champion.data.baseAttr.attackDamage *= (1 + bonus.bonusValue);
                }
              
                else if (bonus.bonusDescription.Contains("Speed"))
                {
                    champion.data.baseAttr.attackSpeed *= (1 + bonus.bonusValue);
                }
            }
        }
        
        public List<string> GetActiveSynergiesInfo()
        {
            List<string> synergyInfo = new List<string>();
            
            foreach (var kvp in classCounts)
            {
                if (kvp.Value > 0)
                {
                    SynergyData synergy = allSynergies.FirstOrDefault(s => s.championClass == kvp.Key);
                    if (synergy != null)
                    {
                        SynergyBonus activeBonus = synergy.GetActiveBonus(kvp.Value);
                        if (activeBonus != null)
                        {
                            synergyInfo.Add($"{synergy.synergyName} ({kvp.Value}): {activeBonus.bonusDescription}");
                        }
                        else
                        {
                            synergyInfo.Add($"{synergy.synergyName} ({kvp.Value})");
                        }
                    }
                }
            }
            
            foreach (var kvp in originCounts)
            {
                if (kvp.Value > 0)
                {
                    SynergyData synergy = allSynergies.FirstOrDefault(s => s.championOrigin == kvp.Key);
                    if (synergy != null)
                    {
                        SynergyBonus activeBonus = synergy.GetActiveBonus(kvp.Value);
                        if (activeBonus != null)
                        {
                            synergyInfo.Add($"{synergy.synergyName} ({kvp.Value}): {activeBonus.bonusDescription}");
                        }
                        else
                        {
                            synergyInfo.Add($"{synergy.synergyName} ({kvp.Value})");
                        }
                    }
                }
            }
            
            return synergyInfo;
        }
        
        // Events for UI
        public static System.Action<List<string>> SynergiesUpdatedEvent;
    }
}
