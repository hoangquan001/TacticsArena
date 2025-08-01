using UnityEngine;
using System.Collections.Generic;
using TacticsArena.Champions;
using TacticsArena.Core;
using UnityEditor;
using System.Linq;

namespace TacticsArena.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Shop Settings")]
        public int shopSize = 5;
        public int refreshCost = 2;
        
        [Header("Champion Pool")]
        public List<ChampionData> allChampions = new List<ChampionData>();
        
        [Header("Current Shop")]
        public List<ChampionData> currentShop = new List<ChampionData>();
        
        [Header("Probability Tables")]
        [Range(0, 100)] public int tier1Probability = 100;
        [Range(0, 100)] public int tier2Probability = 0;
        [Range(0, 100)] public int tier3Probability = 0;
        [Range(0, 100)] public int tier4Probability = 0;
        [Range(0, 100)] public int tier5Probability = 0;
        
        private Dictionary<int, int[]> levelProbabilities = new Dictionary<int, int[]>
        {
            // Level -> [Tier1%, Tier2%, Tier3%, Tier4%, Tier5%]
            {1, new int[] {100, 0, 0, 0, 0}},
            {2, new int[] {70, 30, 0, 0, 0}},
            {3, new int[] {60, 35, 5, 0, 0}},
            {4, new int[] {50, 35, 15, 0, 0}},
            {5, new int[] {45, 33, 20, 2, 0}},
            {6, new int[] {25, 40, 30, 5, 0}},
            {7, new int[] {19, 30, 35, 15, 1}},
            {8, new int[] {18, 25, 32, 22, 3}},
            {9, new int[] {10, 15, 25, 40, 10}},
            {10, new int[] {5, 10, 15, 30, 40}}
        };
        
        private void Start()
        {
            RefreshShop();
        }
        
        public void RefreshShop()
        {
            currentShop.Clear();
            
            for (int i = 0; i < shopSize; i++)
            {
                ChampionData champion = GetRandomChampion();
                if (champion != null)
                    currentShop.Add(champion);
            }
            
            // Trigger UI update
            ShopRefreshedEvent?.Invoke(currentShop);
        }
        
        private ChampionData GetRandomChampion()
        {
            // Lấy player level để xác định probability
            int playerLevel = 1; // TODO: Get from current player
            
            if (!levelProbabilities.ContainsKey(playerLevel))
                playerLevel = 9; // Max level
            
            int[] probabilities = levelProbabilities[playerLevel];
            
            // Random tier dựa trên probability
            int randomValue = Random.Range(0, 100);
            int tier = 1;
            int currentProbability = 0;
            
            for (int i = 0; i < probabilities.Length; i++)
            {
                currentProbability += probabilities[i];
                if (randomValue < currentProbability)
                {
                    tier = i + 1;
                    break;
                }
            }
            
            // Lấy champion random từ tier đó
            List<ChampionData> availableChampions = allChampions.FindAll(c => c.tier == tier);
            
            if (availableChampions.Count > 0)
            {
                return availableChampions[Random.Range(0, availableChampions.Count)];
            }
            
            return null;
        }
        
        public bool BuyChampion(ChampionData championData, Player player)
        {
            if (!currentShop.Contains(championData))
                return false;
            
            if (!player.SpendGold(championData.cost))
                return false;
            
            // Remove from shop
            int index = currentShop.IndexOf(championData);
            currentShop[index] = null; // Mark as null to remove later
            
            // Add to player's bench via event
            ChampionPurchasedEvent?.Invoke(championData);
            
            Debug.Log($"Player bought {championData.championName} for {championData.cost} gold");
            
            return true;
        }
        
        public bool RefreshShopWithCost(Player player)
        {
            if (!player.SpendGold(refreshCost))
                return false;
            
            RefreshShop();
            return true;
        }

        [ContextMenu("Load Champion Pool")]
        public void LoadChampionPool()
        {
            allChampions.Clear();
            var champions = AssetDatabase.FindAssets("t:ChampionData", new[] { "Assets/Data/Champions" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<ChampionData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(data => data != null)
                .OfType<ChampionData>()
                .ToList();
            allChampions.AddRange(champions);
        }

        // Events for UI
        public static System.Action<List<ChampionData>> ShopRefreshedEvent;
        public static System.Action<ChampionData> ChampionPurchasedEvent;
    }
}
