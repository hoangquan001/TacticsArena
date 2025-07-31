using UnityEngine;
using System.Collections.Generic;
using TacticsArena.Battle;
using TacticsArena.Core;

namespace TacticsArena.Champions
{
    [System.Serializable]
    public class ChampionSpawnData
    {
        public ChampionData championData;
        public GameObject modelPrefab;
        public RuntimeAnimatorController animatorController;
        public Vector3 spawnPosition;
        public int level = 1;
        public int stars = 1;
        public int teamId = 0;
    }

    public class ChampionFactory : MonoBehaviour
    {
        public static ChampionFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        [Header("Champion Prefabs")]
        public GameObject defaultChampionPrefab;
        public List<GameObject> championModelPrefabs = new List<GameObject>();

        [Header("Default Components")]
        public RuntimeAnimatorController defaultAnimatorController;
        public Material defaultChampionMaterial;

        [Header("Spawn Settings")]
        public LayerMask championLayer = 64;
        public string championTag = "Champion";

        [Header("Factory Settings")]
        public Transform championParent; // Parent object để organize champions
        public bool enablePooling = true;
        public int poolSize = 20;

        private Dictionary<string, Queue<GameObject>> championPool = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> championPrefabMap = new Dictionary<string, GameObject>();

        private void Start()
        {
            InitializeFactory();
        }

        private void InitializeFactory()
        {
            // Setup champion parent if not assigned
            if (championParent == null)
            {
                GameObject parent = new GameObject("Champions");
                championParent = parent.transform;
            }

            // Build prefab map for quick lookup
            BuildPrefabMap();

            // Initialize object pooling if enabled
            if (enablePooling)
            {
                InitializePool();
            }

            Debug.Log($"ChampionFactory initialized with {championModelPrefabs.Count} prefabs");
        }

        private void BuildPrefabMap()
        {
            championPrefabMap.Clear();

            foreach (GameObject prefab in championModelPrefabs)
            {
                if (prefab != null)
                {
                    championPrefabMap[prefab.name] = prefab;
                }
            }
        }

        private void InitializePool()
        {
            foreach (var kvp in championPrefabMap)
            {
                string prefabName = kvp.Key;
                GameObject prefab = kvp.Value;

                Queue<GameObject> pool = new Queue<GameObject>();

                for (int i = 0; i < poolSize; i++)
                {
                    GameObject pooledObject = CreateChampionObject(prefab, Vector3.zero, false);
                    pooledObject.SetActive(false);
                    pool.Enqueue(pooledObject);
                }

                championPool[prefabName] = pool;
            }
        }

        #region Public Factory Methods

        /// <summary>
        /// Tạo champion với ChampionData và model prefab
        /// </summary>
        public Champion CreateChampion(ChampionData data, GameObject modelPrefab = null, Vector3 position = default, int level = 1, int stars = 1, int teamId = 0)
        {
            if (data == null)
            {
                Debug.LogError("ChampionData is null!");
                return null;
            }

            // Use provided prefab or find by name or use default
            GameObject prefabToUse = modelPrefab ?? GetPrefabForChampion(data) ?? defaultChampionPrefab;

            if (prefabToUse == null)
            {
                Debug.LogError($"No prefab found for champion: {data.championName}");
                return null;
            }

            ChampionSpawnData spawnData = new ChampionSpawnData
            {
                championData = data,
                modelPrefab = prefabToUse,
                spawnPosition = position,
                level = level,
                stars = stars,
                teamId = teamId
            };

            return CreateChampion(spawnData);
        }

        private Color GetRandomChampionColor()
        {
            Color[] colors = {
                Color.red, Color.blue, Color.green, Color.yellow,
                Color.magenta, Color.cyan, Color.white, new Color(1f, 0.5f, 0f) // orange
            };

            return colors[Random.Range(0, colors.Length)];
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

            championObject.layer = GetLayer(championLayer);
            championObject.tag = championTag;
            return championObject;
        }
        private int GetLayer(LayerMask layerMask)
        {
            int layer = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((layerMask.value & (1 << i)) != 0)
                {
                    layer = i;
                    break;
                }
            }
            return layer;
        }


        /// <summary>
        /// Tạo champion với ChampionSpawnData đầy đủ
        /// </summary>
        public Champion CreateChampion(ChampionSpawnData spawnData)
        {
            if (spawnData?.championData == null)
            {
                Debug.LogError("Invalid spawn data!");
                return null;
            }

            // Get champion object (from pool or create new)
            GameObject championObject = null;
            if (spawnData.modelPrefab != null)
            {
                championObject = GetChampionObject(spawnData.modelPrefab, spawnData.spawnPosition);
            }
            else
            {
                championObject = CreateBasicChampionObject(spawnData.championData.championName);
            }

            if (championObject == null)
            {
                Debug.LogError("Failed to create champion object!");
                return null;
            }

            // Setup champion component
            Champion champion = SetupChampionComponent(championObject, spawnData);

            if (champion == null)
            {
                if (enablePooling)
                    ReturnToPool(championObject);
                else
                    Destroy(championObject);

                return null;
            }

            // Apply champion data and configuration
            ConfigureChampion(champion, spawnData);

            Debug.Log($"Created champion: {spawnData.championData.championName} at {spawnData.spawnPosition}");

            return champion;
        }

        /// <summary>
        /// Tạo champion cho specific team
        /// </summary>
        public Champion CreateChampionForTeam(ChampionData data, int teamId, Vector3 position, PlayerArea playerArea = null)
        {
            Champion champion = CreateChampion(data, null, position, 1, 1, teamId);

            if (champion != null && playerArea != null)
            {
                champion.ownerArea = playerArea;

                // Auto add to player area based on team
                if (teamId == 0) // Player team
                {
                    playerArea.TryPlaceChampionOnBench(champion);
                }
            }

            return champion;
        }

        #endregion

        #region Private Factory Methods

        private GameObject GetChampionObject(GameObject prefab, Vector3 position)
        {
            if (enablePooling && championPool.ContainsKey(prefab.name))
            {
                return GetFromPool(prefab.name, position);
            }
            else
            {
                return CreateChampionObject(prefab, position, true);
            }
        }

        private GameObject GetFromPool(string prefabName, Vector3 position)
        {
            if (!championPool.ContainsKey(prefabName) || championPool[prefabName].Count == 0)
            {
                // Pool empty, create new object
                GameObject prefab = championPrefabMap[prefabName];
                return CreateChampionObject(prefab, position, true);
            }

            GameObject pooledObject = championPool[prefabName].Dequeue();
            pooledObject.transform.position = position;
            pooledObject.SetActive(true);

            return pooledObject;
        }

        private GameObject CreateChampionObject(GameObject prefab, Vector3 position, bool setActive = true)
        {
            GameObject championObject = Instantiate(prefab, position, Quaternion.identity, championParent);
            championObject.layer = GetLayer(championLayer);
            championObject.tag = championTag;
            championObject.SetActive(setActive);

            return championObject;
        }

        private Champion SetupChampionComponent(GameObject championObject, ChampionSpawnData spawnData)
        {
            Champion champion = championObject.GetComponent<Champion>();

            if (champion == null)
            {
                champion = championObject.AddComponent<Champion>();
            }

            // Setup animator if not present
            Animator animator = championObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = championObject.AddComponent<Animator>();
            }

            // Set animator controller
            RuntimeAnimatorController controllerToUse = spawnData.animatorController ?? defaultAnimatorController;
            if (controllerToUse != null)
            {
                animator.runtimeAnimatorController = controllerToUse;
            }

            // Setup collider for interactions
            if (championObject.GetComponent<Collider>() == null)
            {
                CapsuleCollider collider = championObject.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
            }

            // Setup rigidbody for physics
            if (championObject.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = championObject.AddComponent<Rigidbody>();
                rb.freezeRotation = true; // Prevent unwanted rotation
                rb.linearDamping = 5f; // Add some drag for smoother movement
            }

            return champion;
        }

        private void ConfigureChampion(Champion champion, ChampionSpawnData spawnData)
        {
            // Set basic data
            champion.data = spawnData.championData;
            champion.level = spawnData.level;
            champion.stars = spawnData.stars;
            champion.teamId = spawnData.teamId;

            // Set position
            champion.transform.position = spawnData.spawnPosition;

            // Initialize champion stats
            champion.InitializeChampion();

            // Apply visual customizations
            ApplyVisualCustomizations(champion, spawnData);

            // Set name for debugging
            champion.gameObject.name = $"{spawnData.championData.championName}_Lv{spawnData.level}_Star{spawnData.stars}";
        }

        private void ApplyVisualCustomizations(Champion champion, ChampionSpawnData spawnData)
        {
            // Apply team colors
            ApplyTeamColors(champion, spawnData.teamId);

            // Apply star effects (visual indicators for star level)
            ApplyStarEffects(champion, spawnData.stars);

            // Apply level scaling if needed
            ApplyLevelScaling(champion, spawnData.level);
        }

        private void ApplyTeamColors(Champion champion, int teamId)
        {
            Renderer[] renderers = champion.GetComponentsInChildren<Renderer>();

            Color teamColor = teamId == 0 ? Color.blue : Color.red;

            foreach (Renderer renderer in renderers)
            {
                if (renderer.material != null)
                {
                    // Tint the material with team color
                    renderer.material.color = Color.Lerp(renderer.material.color, teamColor, 0.3f);
                }
            }
        }

        private void ApplyStarEffects(Champion champion, int stars)
        {
            // Add visual effects based on star level
            Transform effectParent = champion.transform.Find("StarEffects");
            if (effectParent == null)
            {
                GameObject effectObject = new GameObject("StarEffects");
                effectObject.transform.SetParent(champion.transform);
                effectObject.transform.localPosition = Vector3.up * 2.5f;
                effectParent = effectObject.transform;
            }

            // Create star particle effects or UI indicators
            for (int i = 0; i < stars; i++)
            {
                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Cube);
                star.transform.SetParent(effectParent);
                star.transform.localPosition = new Vector3((i - 1) * 0.3f, 0, 0);
                star.transform.localScale = Vector3.one * 0.1f;

                Renderer starRenderer = star.GetComponent<Renderer>();
                starRenderer.material.color = Color.yellow;
                starRenderer.material.SetFloat("_Metallic", 0.8f);
            }
        }

        private void ApplyLevelScaling(Champion champion, int level)
        {
            // Subtle scaling based on level
            float scaleMultiplier = 1f + (level - 1) * 0.05f; // 5% increase per level
            champion.transform.localScale = Vector3.one * scaleMultiplier;
        }

        private GameObject GetPrefabForChampion(ChampionData data)
        {
            // Try to find prefab by champion name
            string prefabName = data.championName.Replace(" ", ""); // Remove spaces

            if (championPrefabMap.ContainsKey(prefabName))
            {
                return championPrefabMap[prefabName];
            }

            // Try alternative naming conventions
            string[] namingVariations = {
                data.championName,
                data.championName + "Prefab",
                data.championName + "_Prefab",
                "Champion_" + data.championName
            };

            foreach (string variation in namingVariations)
            {
                if (championPrefabMap.ContainsKey(variation))
                {
                    return championPrefabMap[variation];
                }
            }

            return null;
        }

        #endregion

        #region Pool Management

        public void ReturnToPool(GameObject championObject)
        {
            if (!enablePooling) return;

            Champion champion = championObject.GetComponent<Champion>();
            if (champion != null)
            {
                champion.StopBattle();
                champion.currentHealth = 0;
                champion.currentMana = 0;
                champion.isAlive = false;
            }

            championObject.SetActive(false);

            string prefabName = championObject.name.Split('_')[0]; // Get original prefab name
            if (championPool.ContainsKey(prefabName))
            {
                championPool[prefabName].Enqueue(championObject);
            }
        }

        public void ClearPool()
        {
            foreach (var pool in championPool.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                        Destroy(obj);
                }
            }
            championPool.Clear();
        }

        #endregion

        #region Utility Methods

        [ContextMenu("Test Create Champion")]
        public void TestCreateChampion()
        {
            if (championModelPrefabs.Count > 0)
            {
                // Create a test champion at random position
                Vector3 randomPos = new Vector3(
                    Random.Range(-5f, 5f),
                    0f,
                    Random.Range(-5f, 5f)
                );

                GameObject testPrefab = championModelPrefabs[0];

                // You would normally pass real ChampionData here
                // For testing, we'll create a basic setup
                GameObject testObj = CreateChampionObject(testPrefab, randomPos, true);
                Champion champion = SetupChampionComponent(testObj, new ChampionSpawnData());

                Debug.Log($"Test champion created at {randomPos}");
            }
        }

        public int GetActiveChampionCount()
        {
            if (championParent == null) return 0;

            int count = 0;
            for (int i = 0; i < championParent.childCount; i++)
            {
                if (championParent.GetChild(i).gameObject.activeInHierarchy)
                    count++;
            }
            return count;
        }

        public List<Champion> GetAllActiveChampions()
        {
            List<Champion> champions = new List<Champion>();

            if (championParent != null)
            {
                Champion[] allChampions = championParent.GetComponentsInChildren<Champion>();
                foreach (Champion champion in allChampions)
                {
                    if (champion.gameObject.activeInHierarchy)
                        champions.Add(champion);
                }
            }

            return champions;
        }

        public List<Champion> GetChampionsByTeam(int teamId)
        {
            List<Champion> teamChampions = new List<Champion>();
            List<Champion> allChampions = GetAllActiveChampions();

            foreach (Champion champion in allChampions)
            {
                if (champion.teamId == teamId)
                    teamChampions.Add(champion);
            }

            return teamChampions;
        }

        #endregion

        #region Debug & Editor

        private void OnValidate()
        {
            // Validate settings in editor
            if (poolSize < 1) poolSize = 1;
            if (championLayer < 0) championLayer = 8;
        }

        [ContextMenu("Debug Pool Status")]
        public void DebugPoolStatus()
        {
            Debug.Log("=== Champion Pool Status ===");
            foreach (var kvp in championPool)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value.Count} available");
            }
            Debug.Log($"Total active champions: {GetActiveChampionCount()}");
        }

        #endregion
    }
}
