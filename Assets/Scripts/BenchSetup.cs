using UnityEngine;

namespace TacticsArena
{
    /// <summary>
    /// Script để setup bench system sau khi compile
    /// </summary>
    public class BenchSetup : MonoBehaviour
    {
        [Header("Bench Settings")]
        public GameObject benchSlotPrefab;
        public int benchSize = 9;
        public float slotSpacing = 1.5f;
        
        [Header("Auto Setup")]
        public bool setupOnStart = true;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupBenchSystem();
            }
        }
        
        [ContextMenu("Setup Bench System")]
        public void SetupBenchSystem()
        {
            // Find player
            var player = FindFirstObjectByType<Core.Player>();
            if (player == null)
            {
                Debug.LogWarning("No player found to setup bench for");
                return;
            }
            
            // Setup bench if doesn't exist
            if (player.playerBench == null)
            {
                GameObject benchObject = new GameObject("PlayerBench");
                benchObject.transform.SetParent(player.transform);
                benchObject.transform.localPosition = new Vector3(0, 0, -5);
                
                // Will add Bench component when available
                player.playerBench = benchObject.AddComponent<MonoBehaviour>();
            }
            
            CreateBenchSlots();
        }
        
        private void CreateBenchSlots()
        {
            if (benchSlotPrefab == null)
            {
                CreateDefaultBenchSlots();
            }
        }
        
        private void CreateDefaultBenchSlots()
        {
            var player = FindFirstObjectByType<Core.Player>();
            if (player?.playerBench == null) return;
            
            Transform benchParent = player.playerBench.transform;
            
            for (int i = 0; i < benchSize; i++)
            {
                GameObject slot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slot.name = $"BenchSlot_{i}";
                slot.transform.SetParent(benchParent);
                
                // Position slots horizontally
                float x = (i - (benchSize - 1) / 2f) * slotSpacing;
                slot.transform.localPosition = new Vector3(x, 0, 0);
                slot.transform.localScale = Vector3.one * 0.8f;
                
                // Color the slots
                Renderer renderer = slot.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.gray;
                }
                
                Debug.Log($"Created bench slot {i}");
            }
            
            Debug.Log("Basic bench slots created. Add Bench and BenchSlot scripts when available.");
        }
    }
}
