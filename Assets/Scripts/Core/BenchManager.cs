using UnityEngine;
using TacticsArena.Champions;
using TacticsArena.Battle;
using TacticsArena.Shop;

namespace TacticsArena.Core
{
    public class BenchManager : MonoBehaviour
    {
        [Header("References")]
        public Bench playerBench;
        public ShopManager shopManager;
        
        private void Start()
        {
            SubscribeToEvents();
            
            // Auto-find references if not assigned
            if (playerBench == null)
                playerBench = FindFirstObjectByType<Bench>();
                
            if (shopManager == null)
                shopManager = FindFirstObjectByType<ShopManager>();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            ShopManager.ChampionPurchasedEvent += OnChampionPurchased;
        }
        
        private void UnsubscribeFromEvents()
        {
            ShopManager.ChampionPurchasedEvent -= OnChampionPurchased;
        }
        
        private void OnChampionPurchased(ChampionData championData)
        {
            if (playerBench == null)
            {
                Debug.LogWarning("No bench found to add champion!");
                return;
            }
            
            if (playerBench.IsFull())
            {
                Debug.Log("Bench is full! Cannot add more champions.");
                return;
            }


            CreateAndAddChampionToBench(championData);
        }
        
        private void CreateAndAddChampionToBench(ChampionData championData)
        {
            Champion champion = ChampionFactory.Instance.CreateChampion(new ChampionSpawnData
            {
                championData = championData,
                modelPrefab = championData.championPrefab,
                spawnPosition = Vector3.zero,
                level = 1,
                stars = 1,
                teamId = 0
            });
            // Add to bench
            if (playerBench.AddChampion(champion))
            {
                Debug.Log($"Added {championData.championName} to bench");
            }
            else
            {
                Debug.LogWarning($"Failed to add {championData.championName} to bench");
            }
        }
        

    }
}
