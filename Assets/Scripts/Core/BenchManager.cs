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
        
        [Header("Champion Creation")]
        public GameObject defaultChampionPrefab;
        
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
            GameObject championObject;
            
            if (championData.championPrefab != null)
            {
                // Use the specified prefab
                championObject = Instantiate(championData.championPrefab);
            }
            else if (defaultChampionPrefab != null)
            {
                // Use default prefab
                championObject = Instantiate(defaultChampionPrefab);
            }
            else
            {
                // Create basic champion GameObject
                championObject = CreateBasicChampionObject(championData.championName);
            }
            
            // Ensure champion component exists
            Champion champion = championObject.GetComponent<Champion>();
            if (champion == null)
                champion = championObject.AddComponent<Champion>();
            
            // Set champion data
            champion.championData = championData;
            
            // Add to bench
            if (playerBench.AddChampion(champion))
            {
                Debug.Log($"Added {championData.championName} to bench");
            }
            else
            {
                Debug.LogWarning($"Failed to add {championData.championName} to bench");
                Destroy(championObject);
            }
        }
        
        private GameObject CreateBasicChampionObject(string championName)
        {
            GameObject championObject = new GameObject(championName);
            
            // Add visual representation
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(championObject.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 0.8f;
            
            // Add different colors for different champions
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = GetRandomChampionColor();
            }
            
            // Add collider for interactions
            if (championObject.GetComponent<Collider>() == null)
            {
                BoxCollider collider = championObject.AddComponent<BoxCollider>();
                collider.size = Vector3.one * 0.8f;
            }
            
            return championObject;
        }
        
        private Color GetRandomChampionColor()
        {
            Color[] colors = {
                Color.red, Color.blue, Color.green, Color.yellow,
                Color.magenta, Color.cyan, Color.white, new Color(1f, 0.5f, 0f) // orange
            };
            
            return colors[Random.Range(0, colors.Length)];
        }
        
        [ContextMenu("Test Add Random Champion")]
        public void TestAddRandomChampion()
        {
            if (shopManager != null && shopManager.allChampions.Count > 0)
            {
                ChampionData randomChampion = shopManager.allChampions[Random.Range(0, shopManager.allChampions.Count)];
                CreateAndAddChampionToBench(randomChampion);
            }
        }
    }
}
