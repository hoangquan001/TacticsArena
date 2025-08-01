using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TacticsArena.Champions;

namespace TacticsArena.Editor
{
    /// <summary>
    /// ChampionCSVUtility - Import/Export champions từ/ra CSV files
    /// </summary>
    public static class ChampionCSVUtility
    {
        private const string CSV_HEADER = "Name,Tier,Cost,Class,Origin,Health,AttackDamage,AttackSpeed,Armor,MagicResist,MaxMana,IconPath,PrefabPath";
        
        /// <summary>
        /// Export all champions to CSV file
        /// </summary>
        public static void ExportToCSV(string filePath)
        {
            var champions = LoadAllChampions();
            
            if (champions.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Failed", "No champions found to export!", "OK");
                return;
            }
            
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine(CSV_HEADER);
            
            foreach (var champion in champions)
            {
                string line = FormatChampionToCSV(champion);
                csvContent.AppendLine(line);
            }
            
            try
            {
                File.WriteAllText(filePath, csvContent.ToString());
                EditorUtility.DisplayDialog("Export Successful", 
                    $"Exported {champions.Count} champions to {filePath}", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Export Failed", $"Error: {e.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Import champions from CSV file
        /// </summary>
        public static void ImportFromCSV(string filePath, string outputFolder)
        {
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Import Failed", "CSV file not found!", "OK");
                return;
            }
            
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                
                if (lines.Length < 2)
                {
                    EditorUtility.DisplayDialog("Import Failed", "CSV file is empty or has no data!", "OK");
                    return;
                }
                
                // Skip header
                int imported = 0;
                int errors = 0;
                var errorMessages = new List<string>();
                
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        ChampionData champion = ParseCSVLineToChampion(lines[i]);
                        if (champion != null)
                        {
                            string assetPath = Path.Combine(outputFolder, $"{champion.championName}.asset");
                            AssetDatabase.CreateAsset(champion, assetPath);
                            imported++;
                        }
                        else
                        {
                            errors++;
                            errorMessages.Add($"Line {i + 1}: Failed to parse champion data");
                        }
                    }
                    catch (System.Exception e)
                    {
                        errors++;
                        errorMessages.Add($"Line {i + 1}: {e.Message}");
                    }
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                string message = $"Import completed!\nImported: {imported} champions\nErrors: {errors}";
                if (errorMessages.Count > 0 && errorMessages.Count <= 5)
                {
                    message += "\n\nErrors:\n" + string.Join("\n", errorMessages.Take(5));
                }
                
                EditorUtility.DisplayDialog("Import Results", message, "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Failed", $"Error reading CSV: {e.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Update existing champions from CSV data
        /// </summary>
        public static void UpdateFromCSV(string filePath)
        {
            if (!File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Update Failed", "CSV file not found!", "OK");
                return;
            }
            
            try
            {
                var existingChampions = LoadAllChampionsDictionary();
                string[] lines = File.ReadAllLines(filePath);
                
                int updated = 0;
                int notFound = 0;
                int errors = 0;
                
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var csvData = ParseCSVLine(lines[i]);
                        string championName = csvData["Name"];
                        
                        if (existingChampions.ContainsKey(championName))
                        {
                            UpdateChampionFromCSVData(existingChampions[championName], csvData);
                            EditorUtility.SetDirty(existingChampions[championName]);
                            updated++;
                        }
                        else
                        {
                            notFound++;
                        }
                    }
                    catch (System.Exception e)
                    {
                        errors++;
                        Debug.LogError($"Line {i + 1}: {e.Message}");
                    }
                }
                
                AssetDatabase.SaveAssets();
                
                string message = $"Update completed!\nUpdated: {updated} champions\nNot found: {notFound}\nErrors: {errors}";
                EditorUtility.DisplayDialog("Update Results", message, "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Update Failed", $"Error: {e.Message}", "OK");
            }
        }
        
        #region Helper Methods
        private static List<ChampionData> LoadAllChampions()
        {
            var champions = new List<ChampionData>();
            string[] championGuids = AssetDatabase.FindAssets("t:ChampionData");
            
            foreach (string guid in championGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChampionData champion = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                if (champion != null)
                {
                    champions.Add(champion);
                }
            }
            
            return champions.OrderBy(c => c.championName).ToList();
        }
        
        private static Dictionary<string, ChampionData> LoadAllChampionsDictionary()
        {
            var champions = new Dictionary<string, ChampionData>();
            string[] championGuids = AssetDatabase.FindAssets("t:ChampionData");
            
            foreach (string guid in championGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChampionData champion = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                if (champion != null && !string.IsNullOrEmpty(champion.championName))
                {
                    champions[champion.championName] = champion;
                }
            }
            
            return champions;
        }
        
        private static string FormatChampionToCSV(ChampionData champion)
        {
            string className = champion.classes != null && champion.classes.Count > 0 ? 
                champion.classes[0].ToString() : "Warrior";
            string originName = champion.origins != null && champion.origins.Count > 0 ? 
                champion.origins[0].ToString() : "Human";
            
            string iconPath = champion.championIcon != null ? AssetDatabase.GetAssetPath(champion.championIcon) : "";
            string prefabPath = champion.championPrefab != null ? AssetDatabase.GetAssetPath(champion.championPrefab) : "";
            
            return $"{champion.championName}," +
                   $"{champion.tier}," +
                   $"{champion.cost}," +
                   $"{className}," +
                   $"{originName}," +
                   $"{champion.baseAttr.health}," +
                   $"{champion.baseAttr.attackDamage}," +
                   $"{champion.baseAttr.attackSpeed}," +
                   $"{champion.baseAttr.armor}," +
                   $"{champion.baseAttr.magicResist}," +
                   $"{champion.baseAttr.maxMana}," +
                   $"{iconPath}," +
                   $"{prefabPath}";
        }
        
        private static ChampionData ParseCSVLineToChampion(string csvLine)
        {
            var data = ParseCSVLine(csvLine);
            
            ChampionData champion = ScriptableObject.CreateInstance<ChampionData>();
            
            champion.championName = data["Name"];
            champion.tier = int.Parse(data["Tier"]);
            champion.cost = int.Parse(data["Cost"]);
            
            // Parse class
            if (System.Enum.TryParse<ChampionClass>(data["Class"], out ChampionClass championClass))
            {
                champion.classes = new List<ChampionClass> { championClass };
            }
            else
            {
                champion.classes = new List<ChampionClass> { ChampionClass.Warrior };
            }
            
            // Parse origin
            if (System.Enum.TryParse<ChampionOrigin>(data["Origin"], out ChampionOrigin championOrigin))
            {
                champion.origins = new List<ChampionOrigin> { championOrigin };
            }
            else
            {
                champion.origins = new List<ChampionOrigin> { ChampionOrigin.Human };
            }
            
            // Parse attributes
            champion.baseAttr = new AttributeData
            {
                health = float.Parse(data["Health"]),
                attackDamage = float.Parse(data["AttackDamage"]),
                attackSpeed = float.Parse(data["AttackSpeed"]),
                armor = float.Parse(data["Armor"]),
                magicResist = float.Parse(data["MagicResist"]),
                maxMana = float.Parse(data["MaxMana"])
            };
            
            // Load assets if paths are provided
            if (!string.IsNullOrEmpty(data["IconPath"]))
            {
                champion.championIcon = AssetDatabase.LoadAssetAtPath<Sprite>(data["IconPath"]);
            }
            
            if (!string.IsNullOrEmpty(data["PrefabPath"]))
            {
                champion.championPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data["PrefabPath"]);
            }
            
            return champion;
        }
        
        private static Dictionary<string, string> ParseCSVLine(string csvLine)
        {
            var values = SplitCSVLine(csvLine);
            var headers = SplitCSVLine(CSV_HEADER);
            
            if (values.Length != headers.Length)
            {
                throw new System.ArgumentException($"CSV line has {values.Length} values but expected {headers.Length}");
            }
            
            var data = new Dictionary<string, string>();
            for (int i = 0; i < headers.Length; i++)
            {
                data[headers[i]] = values[i];
            }
            
            return data;
        }
        
        private static string[] SplitCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentValue = "";
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentValue.Trim());
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }
            
            result.Add(currentValue.Trim());
            return result.ToArray();
        }
        
        private static void UpdateChampionFromCSVData(ChampionData champion, Dictionary<string, string> data)
        {
            champion.championName = data["Name"];
            champion.tier = int.Parse(data["Tier"]);
            champion.cost = int.Parse(data["Cost"]);
            
            // Update class
            if (System.Enum.TryParse<ChampionClass>(data["Class"], out ChampionClass championClass))
            {
                champion.classes = new List<ChampionClass> { championClass };
            }
            
            // Update origin
            if (System.Enum.TryParse<ChampionOrigin>(data["Origin"], out ChampionOrigin championOrigin))
            {
                champion.origins = new List<ChampionOrigin> { championOrigin };
            }
            
            // Update attributes
            champion.baseAttr.health = float.Parse(data["Health"]);
            champion.baseAttr.attackDamage = float.Parse(data["AttackDamage"]);
            champion.baseAttr.attackSpeed = float.Parse(data["AttackSpeed"]);
            champion.baseAttr.armor = float.Parse(data["Armor"]);
            champion.baseAttr.magicResist = float.Parse(data["MagicResist"]);
            champion.baseAttr.maxMana = float.Parse(data["MaxMana"]);
            
            // Update assets if paths are provided and different
            if (!string.IsNullOrEmpty(data["IconPath"]))
            {
                var newIcon = AssetDatabase.LoadAssetAtPath<Sprite>(data["IconPath"]);
                if (newIcon != null) champion.championIcon = newIcon;
            }
            
            if (!string.IsNullOrEmpty(data["PrefabPath"]))
            {
                var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data["PrefabPath"]);
                if (newPrefab != null) champion.championPrefab = newPrefab;
            }
        }
        #endregion
        
        #region Menu Items
        [MenuItem("TacticsArena/CSV Tools/Export Champions to CSV")]
        public static void ExportChampionsToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Champions to CSV", "", "champions", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                ExportToCSV(path);
            }
        }
        
        [MenuItem("TacticsArena/CSV Tools/Import Champions from CSV")]
        public static void ImportChampionsFromCSV()
        {
            string csvPath = EditorUtility.OpenFilePanel("Import Champions from CSV", "", "csv");
            if (!string.IsNullOrEmpty(csvPath))
            {
                string outputFolder = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(outputFolder))
                {
                    outputFolder = FileUtil.GetProjectRelativePath(outputFolder);
                    ImportFromCSV(csvPath, outputFolder);
                }
            }
        }
        
        [MenuItem("TacticsArena/CSV Tools/Update Champions from CSV")]
        public static void UpdateChampionsFromCSV()
        {
            string path = EditorUtility.OpenFilePanel("Update Champions from CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                UpdateFromCSV(path);
            }
        }
        #endregion
    }
}
