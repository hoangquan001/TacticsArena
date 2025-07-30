using UnityEngine;
using TacticsArena.Data;
using TacticsArena.Champions;
using TacticsArena.Synergies;

namespace TacticsArena
{
    /// <summary>
    /// Component để setup sample data cho testing
    /// Attach vào MainGameController để tự động tạo sample data
    /// </summary>
    public class SampleDataSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        public bool createSampleDataOnStart = true;
        
        [Header("References")]
        public Shop.ShopManager shopManager;
        public SynergyManager synergyManager;
        
        private void Start()
        {
            if (createSampleDataOnStart)
            {
                SetupSampleData();
            }
        }
        
        [ContextMenu("Setup Sample Data")]
        public void SetupSampleData()
        {
            SetupChampionData();
            SetupSynergyData();
            Debug.Log("Sample data created successfully!");
        }
        
        private void SetupChampionData()
        {
            if (shopManager == null)
                shopManager = FindObjectOfType<Shop.ShopManager>();
            
            if (shopManager != null)
            {
                // Tạo sample champion pool
                ChampionData[] sampleChampions = SampleDataFactory.CreateSampleChampionPool();
                
                // Add vào shop manager
                shopManager.allChampions.Clear();
                shopManager.allChampions.AddRange(sampleChampions);
                
                Debug.Log($"Added {sampleChampions.Length} sample champions to shop");
            }
        }
        
        private void SetupSynergyData()
        {
            if (synergyManager == null)
                synergyManager = FindObjectOfType<SynergyManager>();
            
            if (synergyManager != null)
            {
                synergyManager.allSynergies.Clear();
                
                // Create class synergies
                synergyManager.allSynergies.Add(CreateWarriorSynergy());
                synergyManager.allSynergies.Add(CreateMageSynergy());
                synergyManager.allSynergies.Add(CreateArcherSynergy());
                synergyManager.allSynergies.Add(CreateAssassinSynergy());
                synergyManager.allSynergies.Add(CreateTankSynergy());
                
                // Create origin synergies
                synergyManager.allSynergies.Add(CreateHumanSynergy());
                synergyManager.allSynergies.Add(CreateElfSynergy());
                synergyManager.allSynergies.Add(CreateOrcSynergy());
                synergyManager.allSynergies.Add(CreateDragonSynergy());
                synergyManager.allSynergies.Add(CreateUndeadSynergy());
                
                Debug.Log($"Created {synergyManager.allSynergies.Count} synergies");
            }
        }
        
        private SynergyData CreateWarriorSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Warrior";
            synergy.championClass = ChampionClass.Warrior;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+20% Attack Damage",
                bonusValue = 0.2f
            });
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 4,
                bonusDescription = "+40% Attack Damage",
                bonusValue = 0.4f
            });
            
            return synergy;
        }
        
        private SynergyData CreateMageSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Mage";
            synergy.championClass = ChampionClass.Mage;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+30% Spell Damage",
                bonusValue = 0.3f
            });
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 4,
                bonusDescription = "+60% Spell Damage",
                bonusValue = 0.6f
            });
            
            return synergy;
        }
        
        private SynergyData CreateArcherSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Archer";
            synergy.championClass = ChampionClass.Archer;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+25% Attack Speed",
                bonusValue = 0.25f
            });
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 4,
                bonusDescription = "+50% Attack Speed",
                bonusValue = 0.5f
            });
            
            return synergy;
        }
        
        private SynergyData CreateAssassinSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Assassin";
            synergy.championClass = ChampionClass.Assassin;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+15% Critical Chance",
                bonusValue = 0.15f
            });
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 4,
                bonusDescription = "+30% Critical Chance",
                bonusValue = 0.3f
            });
            
            return synergy;
        }
        
        private SynergyData CreateTankSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Tank";
            synergy.championClass = ChampionClass.Tank;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+30% Health",
                bonusValue = 0.3f
            });
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 4,
                bonusDescription = "+60% Health",
                bonusValue = 0.6f
            });
            
            return synergy;
        }
        
        private SynergyData CreateHumanSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Human";
            synergy.championOrigin = ChampionOrigin.Human;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+20% All Stats",
                bonusValue = 0.2f
            });
            
            return synergy;
        }
        
        private SynergyData CreateElfSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Elf";
            synergy.championOrigin = ChampionOrigin.Elf;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+30% Attack Speed",
                bonusValue = 0.3f
            });
            
            return synergy;
        }
        
        private SynergyData CreateOrcSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Orc";
            synergy.championOrigin = ChampionOrigin.Orc;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+40% Health",
                bonusValue = 0.4f
            });
            
            return synergy;
        }
        
        private SynergyData CreateDragonSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Dragon";
            synergy.championOrigin = ChampionOrigin.Dragon;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "+100% Spell Damage",
                bonusValue = 1.0f
            });
            
            return synergy;
        }
        
        private SynergyData CreateUndeadSynergy()
        {
            var synergy = ScriptableObject.CreateInstance<SynergyData>();
            synergy.synergyName = "Undead";
            synergy.championOrigin = ChampionOrigin.Undead;
            
            synergy.bonuses.Add(new SynergyBonus
            {
                requiredCount = 2,
                bonusDescription = "Immune to Death",
                bonusValue = 1.0f
            });
            
            return synergy;
        }
    }
}
