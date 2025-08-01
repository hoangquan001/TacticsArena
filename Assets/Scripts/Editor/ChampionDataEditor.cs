using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using TacticsArena.Champions;

namespace TacticsArena.Editor
{
    /// <summary>
    /// ChampionDataEditor - Tool mạnh mẽ để setup và generate champion data
    /// Cung cấp GUI editor với nhiều chức năng tự động hóa
    /// </summary>
    public class ChampionDataEditor : EditorWindow
    {
        [MenuItem("TacticsArena/Champion Data Editor")]
        public static void ShowWindow()
        {
            ChampionDataEditor window = GetWindow<ChampionDataEditor>("Champion Data Editor");
            window.minSize = new Vector2(500, 700);
            window.Show();
        }

        #region Fields
        // Current editing data
        private ChampionData currentChampionData;
        private SerializedObject serializedObject;
        
        // Generation settings
        private bool showGenerationSettings = true;
        private bool showBatchOperations = true;
        private bool showPresetTemplates = true;
        private bool showUtilities = true;
        
        // Generation parameters
        private int generateCount = 5;
        private string baseName = "Champion";
        private ChampionClass selectedClass = ChampionClass.Warrior;
        private ChampionOrigin selectedOrigin = ChampionOrigin.Human;
        private int tierRange = 3;
        private Vector2 healthRange = new Vector2(80, 200);
        private Vector2 damageRange = new Vector2(15, 45);
        private Vector2 armorRange = new Vector2(5, 25);
        private Vector2 speedRange = new Vector2(0.8f, 1.5f);
        private Vector2 manaRange = new Vector2(50, 150);
        
        // Batch operations
        private List<ChampionData> selectedChampions = new List<ChampionData>();
        private float batchHealthMultiplier = 1.2f;
        private float batchDamageMultiplier = 1.1f;
        private int batchTierAdjustment = 0;
        
        // Presets
        private ChampionPreset[] championPresets;
        private int selectedPresetIndex = 0;
        
        // Utilities
        private string exportPath = "Assets/Data/Champions/Generated/";
        private bool autoAssignIcons = true;
        private bool autoAssignPrefabs = true;
        private Texture2D[] availableIcons;
        private GameObject[] availablePrefabs;
        
        // GUI
        private Vector2 scrollPosition;
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            InitializeStyles();
            LoadPresets();
            LoadAvailableAssets();
        }

        private void OnGUI()
        {
            if (headerStyle == null) InitializeStyles();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawCurrentChampionEditor();
            DrawPresetTemplates();
            DrawGenerationSettings();
            // DrawBatchOperations();
            // DrawUtilities();
            
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region GUI Drawing
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Champion Data Editor", headerStyle);
            EditorGUILayout.Space(10);
            
            // Quick actions toolbar
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New Champion", GUILayout.Height(30)))
            {
                CreateNewChampionData();
            }
            if (GUILayout.Button("Load Champion", GUILayout.Height(30)))
            {
                LoadChampionData();
            }
            if (GUILayout.Button("Save Current", GUILayout.Height(30)))
            {
                SaveCurrentChampionData();
            }
            if (GUILayout.Button("Refresh Assets", GUILayout.Height(30)))
            {
                LoadAvailableAssets();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }

        private void DrawCurrentChampionEditor()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Current Champion", EditorStyles.boldLabel);
            
            if (currentChampionData != null)
            {
                if (serializedObject == null || serializedObject.targetObject != currentChampionData)
                {
                    serializedObject = new SerializedObject(currentChampionData);
                }
                
                serializedObject.Update();
                
                // Basic info
                EditorGUILayout.LabelField("Basic Information", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("championName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cost"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tier"));
                
                EditorGUILayout.Space(5);
                
                // Classes and Origins
                EditorGUILayout.LabelField("Classification", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("classes"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("origins"));
                
                EditorGUILayout.Space(5);
                
                // Stats with quick adjust buttons
                EditorGUILayout.LabelField("Base Attributes", EditorStyles.miniBoldLabel);
                DrawAttributeField("health", "Health");
                DrawAttributeField("attackDamage", "Attack Damage");
                DrawAttributeField("attackSpeed", "Attack Speed");
                DrawAttributeField("armor", "Armor");
                DrawAttributeField("magicResist", "Magic Resist");
                DrawAttributeField("maxMana", "Max Mana");
                DrawAttributeField("range", "Range");


                EditorGUILayout.Space(5);
                
                // Assets
                EditorGUILayout.LabelField("Assets", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("championIcon"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("championPrefab"));
                
                // Quick assign buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Auto Assign Icon"))
                {
                    AutoAssignIcon();
                }
                if (GUILayout.Button("Auto Assign Prefab"))
                {
                    AutoAssignPrefab();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Ability
                EditorGUILayout.LabelField("Ability", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityData"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("animations"));


                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.HelpBox("No champion data selected. Create or load a champion to edit.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAttributeField(string propertyName, string displayName)
        {
            var baseAttrProperty = serializedObject.FindProperty("baseAttr");
            var property = baseAttrProperty.FindPropertyRelative(propertyName);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, new GUIContent(displayName));
            
            if (GUILayout.Button("+10%", GUILayout.Width(50)))
            {
                property.floatValue *= 1.1f;
            }
            if (GUILayout.Button("-10%", GUILayout.Width(50)))
            {
                property.floatValue *= 0.9f;
            }
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                property.floatValue = GetDefaultAttributeValue(propertyName);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGenerationSettings()
        {
            showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings, "Generation Settings", true);
            if (!showGenerationSettings) return;
            
            EditorGUILayout.BeginVertical(boxStyle);
            
            EditorGUILayout.LabelField("Batch Generation", EditorStyles.boldLabel);
            
            generateCount = EditorGUILayout.IntSlider("Count", generateCount, 1, 20);
            baseName = EditorGUILayout.TextField("Base Name", baseName);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Classification", EditorStyles.miniBoldLabel);
            selectedClass = (ChampionClass)EditorGUILayout.EnumPopup("Default Class", selectedClass);
            selectedOrigin = (ChampionOrigin)EditorGUILayout.EnumPopup("Default Origin", selectedOrigin);
            tierRange = EditorGUILayout.IntSlider("Max Tier", tierRange, 1, 5);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Attribute Ranges", EditorStyles.miniBoldLabel);
            healthRange = EditorGUILayout.Vector2Field("Health Range", healthRange);
            damageRange = EditorGUILayout.Vector2Field("Damage Range", damageRange);
            armorRange = EditorGUILayout.Vector2Field("Armor Range", armorRange);
            speedRange = EditorGUILayout.Vector2Field("Speed Range", speedRange);
            manaRange = EditorGUILayout.Vector2Field("Mana Range", manaRange);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Random Champions", GUILayout.Height(35)))
            {
                GenerateRandomChampions();
            }
            if (GUILayout.Button("Generate Balanced Set", GUILayout.Height(35)))
            {
                GenerateBalancedChampionSet();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawBatchOperations()
        {
            showBatchOperations = EditorGUILayout.Foldout(showBatchOperations, "Batch Operations", true);
            if (!showBatchOperations) return;
            
            EditorGUILayout.BeginVertical(boxStyle);
            
            EditorGUILayout.LabelField("Batch Modifications", EditorStyles.boldLabel);
            
            // Champion selection
            EditorGUILayout.LabelField("Selected Champions:", EditorStyles.miniBoldLabel);
            if (selectedChampions.Count == 0)
            {
                EditorGUILayout.HelpBox("No champions selected. Use the buttons below to select champions.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < selectedChampions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i + 1}. {selectedChampions[i]?.championName ?? "Null"}");
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        selectedChampions.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All Champions"))
            {
                SelectAllChampions();
            }
            if (GUILayout.Button("Select by Tier"))
            {
                SelectChampionsByTier();
            }
            if (GUILayout.Button("Clear Selection"))
            {
                selectedChampions.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Batch modification settings
            EditorGUILayout.LabelField("Modification Settings", EditorStyles.miniBoldLabel);
            batchHealthMultiplier = EditorGUILayout.Slider("Health Multiplier", batchHealthMultiplier, 0.5f, 2.0f);
            batchDamageMultiplier = EditorGUILayout.Slider("Damage Multiplier", batchDamageMultiplier, 0.5f, 2.0f);
            batchTierAdjustment = EditorGUILayout.IntSlider("Tier Adjustment", batchTierAdjustment, -2, 2);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Multipliers") && selectedChampions.Count > 0)
            {
                ApplyBatchMultipliers();
            }
            if (GUILayout.Button("Normalize Stats") && selectedChampions.Count > 0)
            {
                NormalizeBatchStats();
            }
            if (GUILayout.Button("Auto Balance") && selectedChampions.Count > 0)
            {
                AutoBalanceBatch();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPresetTemplates()
        {
            showPresetTemplates = EditorGUILayout.Foldout(showPresetTemplates, "Preset Templates", true);
            if (!showPresetTemplates) return;
            
            EditorGUILayout.BeginVertical(boxStyle);
            
            EditorGUILayout.LabelField("Champion Presets", EditorStyles.boldLabel);
            
            if (championPresets != null && championPresets.Length > 0)
            {
                string[] presetNames = new string[championPresets.Length];
                for (int i = 0; i < championPresets.Length; i++)
                {
                    presetNames[i] = championPresets[i].name;
                }
                
                selectedPresetIndex = EditorGUILayout.Popup("Select Preset", selectedPresetIndex, presetNames);
                
                if (selectedPresetIndex >= 0 && selectedPresetIndex < championPresets.Length)
                {
                    var preset = championPresets[selectedPresetIndex];
                    EditorGUILayout.LabelField($"Class: {preset.championClass}");
                    EditorGUILayout.LabelField($"Origin: {preset.championOrigin}");
                    EditorGUILayout.LabelField($"Tier: {preset.tier}");
                    EditorGUILayout.LabelField($"Health: {preset.baseHealth}");
                    EditorGUILayout.LabelField($"Damage: {preset.baseDamage}");
                }
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Preset to Current"))
                {
                    ApplyPresetToCurrent();
                }
                if (GUILayout.Button("Create from Preset"))
                {
                    CreateChampionFromPreset();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No presets found. Create some preset files in the project.", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Tank"))
            {
                ApplyQuickPreset("Tank");
            }
            if (GUILayout.Button("DPS"))
            {
                ApplyQuickPreset("DPS");
            }
            if (GUILayout.Button("Support"))
            {
                ApplyQuickPreset("Support");
            }
            if (GUILayout.Button("Assassin"))
            {
                ApplyQuickPreset("Assassin");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawUtilities()
        {
            showUtilities = EditorGUILayout.Foldout(showUtilities, "Utilities", true);
            if (!showUtilities) return;
            
            EditorGUILayout.BeginVertical(boxStyle);
            
            EditorGUILayout.LabelField("Export/Import", EditorStyles.boldLabel);
            
            exportPath = EditorGUILayout.TextField("Export Path", exportPath);
            autoAssignIcons = EditorGUILayout.Toggle("Auto Assign Icons", autoAssignIcons);
            autoAssignPrefabs = EditorGUILayout.Toggle("Auto Assign Prefabs", autoAssignPrefabs);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export All Champions"))
            {
                ExportAllChampions();
            }
            if (GUILayout.Button("Import from CSV"))
            {
                ImportFromCSV();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate All Champions"))
            {
                ValidateAllChampions();
            }
            if (GUILayout.Button("Fix Missing References"))
            {
                FixMissingReferences();
            }
            if (GUILayout.Button("Update Asset References"))
            {
                UpdateAssetReferences();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            if (GUILayout.Button("Show Champion Statistics"))
            {
                ShowChampionStatistics();
            }
            
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Core Functions
        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
        }

        private void CreateNewChampionData()
        {
            currentChampionData = CreateInstance<ChampionData>();
            currentChampionData.championName = "New Champion";
            currentChampionData.cost = 1;
            currentChampionData.tier = 1;
            currentChampionData.baseAttr = new AttributeData
            {
                health = 100,
                attackDamage = 25,
                attackSpeed = 1.0f,
                armor = 10,
                magicResist = 10,
                maxMana = 100
            };
            currentChampionData.classes = new List<ChampionClass> { ChampionClass.Warrior };
            currentChampionData.origins = new List<ChampionOrigin> { ChampionOrigin.Human };
            
            serializedObject = new SerializedObject(currentChampionData);
        }

        private void LoadChampionData()
        {
            string path = EditorUtility.OpenFilePanel("Load Champion Data", "Assets/Data/Champions", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);
                currentChampionData = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                serializedObject = new SerializedObject(currentChampionData);
            }
        }

        private void SaveCurrentChampionData()
        {
            if (currentChampionData == null) return;
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Champion Data", 
                currentChampionData.championName, 
                "asset",
                "Save champion data as asset file",
                "Assets/Data/Champions/"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(currentChampionData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", $"Champion data saved to {path}", "OK");
            }
        }
        #endregion

        #region Generation Functions
        private void GenerateRandomChampions()
        {
            if (!AssetDatabase.IsValidFolder(exportPath))
            {
                Directory.CreateDirectory(exportPath);
                AssetDatabase.Refresh();
            }
            
            for (int i = 0; i < generateCount; i++)
            {
                ChampionData newChampion = CreateInstance<ChampionData>();
                
                // Basic info
                newChampion.championName = $"{baseName}_{i + 1:D2}";
                newChampion.tier = Random.Range(1, tierRange + 1);
                newChampion.cost = newChampion.tier;
                
                // Classification
                newChampion.classes = new List<ChampionClass> { GetRandomClass() };
                newChampion.origins = new List<ChampionOrigin> { GetRandomOrigin() };
                
                // Random stats
                newChampion.baseAttr = new AttributeData
                {
                    health = Random.Range(healthRange.x, healthRange.y),
                    attackDamage = Random.Range(damageRange.x, damageRange.y),
                    attackSpeed = Random.Range(speedRange.x, speedRange.y),
                    armor = Random.Range(armorRange.x, armorRange.y),
                    magicResist = Random.Range(armorRange.x, armorRange.y),
                    maxMana = Random.Range(manaRange.x, manaRange.y)
                };
                
                // Scale by tier
                float tierMultiplier = 1f + (newChampion.tier - 1) * 0.3f;
                newChampion.baseAttr.health *= tierMultiplier;
                newChampion.baseAttr.attackDamage *= tierMultiplier;
                
                // Auto assign assets
                if (autoAssignIcons) AutoAssignIconToChampion(newChampion);
                if (autoAssignPrefabs) AutoAssignPrefabToChampion(newChampion);
                
                // Save
                string assetPath = $"{exportPath}{newChampion.championName}.asset";
                AssetDatabase.CreateAsset(newChampion, assetPath);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", $"Generated {generateCount} random champions!", "OK");
        }

        private void GenerateBalancedChampionSet()
        {
            if (!AssetDatabase.IsValidFolder(exportPath))
            {
                Directory.CreateDirectory(exportPath);
                AssetDatabase.Refresh();
            }
            
            var classes = System.Enum.GetValues(typeof(ChampionClass));
            int championsPerClass = Mathf.Max(1, generateCount / classes.Length);
            
            int championIndex = 0;
            
            foreach (ChampionClass championClass in classes)
            {
                if (championIndex >= generateCount) break;
                
                for (int i = 0; i < championsPerClass && championIndex < generateCount; i++)
                {
                    ChampionData newChampion = CreateInstance<ChampionData>();
                    newChampion.AssignDefaultData();

                    // Basic info
                    newChampion.championName = $"{championClass}_{i + 1:D2}";
                    newChampion.tier = Random.Range(1, tierRange + 1);
                    newChampion.cost = newChampion.tier;
                    
                    // Classification
                    newChampion.classes = new List<ChampionClass> { championClass };
                    newChampion.origins = new List<ChampionOrigin> { GetRandomOrigin() };
                    
                    // Balanced stats based on class
                    newChampion.baseAttr = GetBalancedStatsForClass(championClass, newChampion.tier);
                    
                    // Auto assign assets
                    if (autoAssignIcons) AutoAssignIconToChampion(newChampion);
                    if (autoAssignPrefabs) AutoAssignPrefabToChampion(newChampion);
                    // Save
                    string assetPath = $"{exportPath}{newChampion.championName}.asset";
                    AssetDatabase.CreateAsset(newChampion, assetPath);
                    
                    championIndex++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", $"Generated {championIndex} balanced champions!", "OK");
        }
        #endregion

        #region Helper Functions
        private ChampionClass GetRandomClass()
        {
            var classes = System.Enum.GetValues(typeof(ChampionClass));
            return (ChampionClass)classes.GetValue(Random.Range(0, classes.Length));
        }

        private ChampionOrigin GetRandomOrigin()
        {
            var origins = System.Enum.GetValues(typeof(ChampionOrigin));
            return (ChampionOrigin)origins.GetValue(Random.Range(0, origins.Length));
        }

        private AttributeData GetBalancedStatsForClass(ChampionClass championClass, int tier)
        {
            float tierMultiplier = 1f + (tier - 1) * 0.3f;
            
            switch (championClass)
            {
                case ChampionClass.Tank:
                    return new AttributeData
                    {
                        health = Random.Range(150, 250) * tierMultiplier,
                        attackDamage = Random.Range(15, 25) * tierMultiplier,
                        attackSpeed = Random.Range(0.6f, 0.9f),
                        armor = Random.Range(20, 35) * tierMultiplier,
                        magicResist = Random.Range(15, 25) * tierMultiplier,
                        maxMana = Random.Range(80, 120)
                    };
                    
                case ChampionClass.Assassin:
                    return new AttributeData
                    {
                        health = Random.Range(70, 120) * tierMultiplier,
                        attackDamage = Random.Range(35, 55) * tierMultiplier,
                        attackSpeed = Random.Range(1.2f, 1.8f),
                        armor = Random.Range(5, 15) * tierMultiplier,
                        magicResist = Random.Range(5, 15) * tierMultiplier,
                        maxMana = Random.Range(60, 100)
                    };
                    
                case ChampionClass.Mage:
                    return new AttributeData
                    {
                        health = Random.Range(80, 130) * tierMultiplier,
                        attackDamage = Random.Range(20, 35) * tierMultiplier,
                        attackSpeed = Random.Range(0.8f, 1.2f),
                        armor = Random.Range(5, 15) * tierMultiplier,
                        magicResist = Random.Range(15, 30) * tierMultiplier,
                        maxMana = Random.Range(120, 200)
                    };
                    
                case ChampionClass.Archer:
                    return new AttributeData
                    {
                        health = Random.Range(90, 140) * tierMultiplier,
                        attackDamage = Random.Range(30, 45) * tierMultiplier,
                        attackSpeed = Random.Range(1.0f, 1.4f),
                        armor = Random.Range(8, 18) * tierMultiplier,
                        magicResist = Random.Range(8, 18) * tierMultiplier,
                        maxMana = Random.Range(70, 110)
                    };
                    
                case ChampionClass.Support:
                    return new AttributeData
                    {
                        health = Random.Range(100, 160) * tierMultiplier,
                        attackDamage = Random.Range(18, 30) * tierMultiplier,
                        attackSpeed = Random.Range(0.9f, 1.3f),
                        armor = Random.Range(12, 22) * tierMultiplier,
                        magicResist = Random.Range(18, 30) * tierMultiplier,
                        maxMana = Random.Range(100, 180)
                    };
                    
                default: // Warrior
                    return new AttributeData
                    {
                        health = Random.Range(120, 180) * tierMultiplier,
                        attackDamage = Random.Range(25, 40) * tierMultiplier,
                        attackSpeed = Random.Range(0.9f, 1.3f),
                        armor = Random.Range(15, 25) * tierMultiplier,
                        magicResist = Random.Range(10, 20) * tierMultiplier,
                        maxMana = Random.Range(80, 130)
                    };
            }
        }

        private float GetDefaultAttributeValue(string attributeName)
        {
            switch (attributeName)
            {
                case "health": return 100f;
                case "attackDamage": return 25f;
                case "attackSpeed": return 1.0f;
                case "armor": return 10f;
                case "magicResist": return 10f;
                case "maxMana": return 100f;
                default: return 0f;
            }
        }

        private void LoadPresets()
        {
            // Load preset data from project
            string[] presetGuids = AssetDatabase.FindAssets("t:ChampionPreset");
            championPresets = new ChampionPreset[presetGuids.Length];
            
            for (int i = 0; i < presetGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(presetGuids[i]);
                championPresets[i] = AssetDatabase.LoadAssetAtPath<ChampionPreset>(path);
            }
        }

        private void LoadAvailableAssets()
        {
            // Load available icons
            string[] iconGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Icons" });
            availableIcons = new Texture2D[iconGuids.Length];
            for (int i = 0; i < iconGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(iconGuids[i]);
                availableIcons[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            
            // Load available prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs/Champions" });
            availablePrefabs = new GameObject[prefabGuids.Length];
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                availablePrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        private void AutoAssignIcon()
        {
            if (currentChampionData != null && availableIcons != null && availableIcons.Length > 0)
            {
                AutoAssignIconToChampion(currentChampionData);
                serializedObject.Update();
            }
        }

        private void AutoAssignIconToChampion(ChampionData champion)
        {
            if (availableIcons == null || availableIcons.Length == 0) return;
            
            // Try to find icon by name first
            foreach (var icon in availableIcons)
            {
                if (icon.name.ToLower().Contains(champion.championName.ToLower()) ||
                    champion.championName.ToLower().Contains(icon.name.ToLower()))
                {
                    champion.championIcon = CreateSpriteFromTexture(icon);
                    return;
                }
            }
            
            // Random assignment if no match found
            var randomIcon = availableIcons[Random.Range(0, availableIcons.Length)];
            champion.championIcon = CreateSpriteFromTexture(randomIcon);
        }

        private void AutoAssignPrefab()
        {
            if (currentChampionData != null && availablePrefabs != null && availablePrefabs.Length > 0)
            {
                AutoAssignPrefabToChampion(currentChampionData);
                serializedObject.Update();
            }
        }

        private void AutoAssignPrefabToChampion(ChampionData champion)
        {
            if (availablePrefabs == null || availablePrefabs.Length == 0) return;
            
            // Try to find prefab by name first
            foreach (var prefab in availablePrefabs)
            {
                if (prefab.name.ToLower().Contains(champion.championName.ToLower()) ||
                    champion.championName.ToLower().Contains(prefab.name.ToLower()))
                {
                    champion.championPrefab = prefab;
                    return;
                }
            }
            
            // Random assignment if no match found
            champion.championPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Length)];
        }

        private Sprite CreateSpriteFromTexture(Texture2D texture)
        {
            if (texture == null) return null;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        // Batch operations
        private void SelectAllChampions()
        {
            selectedChampions.Clear();
            string[] championGuids = AssetDatabase.FindAssets("t:ChampionData");
            
            foreach (string guid in championGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChampionData champion = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                if (champion != null)
                {
                    selectedChampions.Add(champion);
                }
            }
        }

        private void SelectChampionsByTier()
        {
            int tier = EditorGUILayout.IntField("Select Tier", 1);
            selectedChampions.Clear();
            
            string[] championGuids = AssetDatabase.FindAssets("t:ChampionData");
            
            foreach (string guid in championGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChampionData champion = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                if (champion != null && champion.tier == tier)
                {
                    selectedChampions.Add(champion);
                }
            }
        }

        private void ApplyBatchMultipliers()
        {
            foreach (var champion in selectedChampions)
            {
                if (champion != null)
                {
                    champion.baseAttr.health *= batchHealthMultiplier;
                    champion.baseAttr.attackDamage *= batchDamageMultiplier;
                    champion.tier = Mathf.Clamp(champion.tier + batchTierAdjustment, 1, 5);
                    
                    EditorUtility.SetDirty(champion);
                }
            }
            
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Applied multipliers to {selectedChampions.Count} champions!", "OK");
        }

        private void NormalizeBatchStats()
        {
            // Implementation for normalizing stats across selected champions
            EditorUtility.DisplayDialog("Info", "Normalize stats feature - to be implemented based on your balance requirements", "OK");
        }

        private void AutoBalanceBatch()
        {
            // Implementation for auto-balancing selected champions
            EditorUtility.DisplayDialog("Info", "Auto balance feature - to be implemented based on your game balance system", "OK");
        }

        private void ApplyPresetToCurrent()
        {
            if (currentChampionData != null && championPresets != null && selectedPresetIndex < championPresets.Length)
            {
                var preset = championPresets[selectedPresetIndex];
                ApplyPresetToChampion(currentChampionData, preset);
                serializedObject.Update();
            }
        }

        private void CreateChampionFromPreset()
        {
            if (championPresets != null && selectedPresetIndex < championPresets.Length)
            {
                CreateNewChampionData();
                var preset = championPresets[selectedPresetIndex];
                ApplyPresetToChampion(currentChampionData, preset);
                serializedObject.Update();
            }
        }

        private void ApplyPresetToChampion(ChampionData champion, ChampionPreset preset)
        {
            champion.championName = preset.name;
            champion.tier = preset.tier;
            champion.cost = preset.tier;
            champion.classes = new List<ChampionClass> { preset.championClass };
            champion.origins = new List<ChampionOrigin> { preset.championOrigin };
            
            champion.baseAttr.health = preset.baseHealth;
            champion.baseAttr.attackDamage = preset.baseDamage;
            // Apply other preset values as needed
        }

        private void ApplyQuickPreset(string presetType)
        {
            if (currentChampionData == null) CreateNewChampionData();
            
            switch (presetType)
            {
                case "Tank":
                    currentChampionData.championName = "Tank Champion";
                    currentChampionData.classes = new List<ChampionClass> { ChampionClass.Tank };
                    currentChampionData.baseAttr = GetBalancedStatsForClass(ChampionClass.Tank, 2);
                    break;
                case "DPS":
                    currentChampionData.championName = "DPS Champion";
                    currentChampionData.classes = new List<ChampionClass> { ChampionClass.Assassin };
                    currentChampionData.baseAttr = GetBalancedStatsForClass(ChampionClass.Assassin, 2);
                    break;
                case "Support":
                    currentChampionData.championName = "Support Champion";
                    currentChampionData.classes = new List<ChampionClass> { ChampionClass.Support };
                    currentChampionData.baseAttr = GetBalancedStatsForClass(ChampionClass.Support, 2);
                    break;
                case "Assassin":
                    currentChampionData.championName = "Assassin Champion";
                    currentChampionData.classes = new List<ChampionClass> { ChampionClass.Assassin };
                    currentChampionData.baseAttr = GetBalancedStatsForClass(ChampionClass.Assassin, 2);
                    break;
            }
            
            serializedObject.Update();
        }

        // Utility functions
        private void ExportAllChampions()
        {
            string path = EditorUtility.SaveFilePanel("Export Champions to CSV", "", "champions_export", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                ChampionCSVUtility.ExportToCSV(path);
            }
        }

        private void ImportFromCSV()
        {
            string csvPath = EditorUtility.OpenFilePanel("Import Champions from CSV", "", "csv");
            if (!string.IsNullOrEmpty(csvPath))
            {
                if (!AssetDatabase.IsValidFolder(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                    AssetDatabase.Refresh();
                }
                ChampionCSVUtility.ImportFromCSV(csvPath, exportPath);
            }
        }

        private void ValidateAllChampions()
        {
            string[] championGuids = AssetDatabase.FindAssets("t:ChampionData");
            int issues = 0;
            
            foreach (string guid in championGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChampionData champion = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                
                if (champion != null)
                {
                    if (string.IsNullOrEmpty(champion.championName)) issues++;
                    if (champion.championIcon == null) issues++;
                    if (champion.championPrefab == null) issues++;
                    if (champion.baseAttr.health <= 0) issues++;
                }
            }
            
            EditorUtility.DisplayDialog("Validation Complete", 
                $"Found {issues} issues across {championGuids.Length} champions.", "OK");
        }

        private void FixMissingReferences()
        {
            EditorUtility.DisplayDialog("Info", "Fix missing references - feature to be implemented", "OK");
        }

        private void UpdateAssetReferences()
        {
            LoadAvailableAssets();
            EditorUtility.DisplayDialog("Success", "Asset references updated!", "OK");
        }

        private void ShowChampionStatistics()
        {
            ChampionStatisticsWindow.ShowWindow();
        }
        #endregion
    }

    #region Data Classes
    [System.Serializable]
    public class ChampionPreset : ScriptableObject
    {
        public ChampionClass championClass;
        public ChampionOrigin championOrigin;
        public int tier = 1;
        public float baseHealth = 100;
        public float baseDamage = 25;
        public float baseArmor = 10;
        public float baseMana = 100;
    }
    #endregion
}
