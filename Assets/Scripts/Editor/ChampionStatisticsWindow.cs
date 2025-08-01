using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TacticsArena.Champions;

namespace TacticsArena.Editor
{
    /// <summary>
    /// ChampionStatisticsWindow - Hiển thị thống kê chi tiết về champions
    /// </summary>
    public class ChampionStatisticsWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            ChampionStatisticsWindow window = GetWindow<ChampionStatisticsWindow>("Champion Statistics");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }

        private Vector2 scrollPosition;
        private List<ChampionData> allChampions = new List<ChampionData>();
        private Dictionary<ChampionClass, int> classDistribution = new Dictionary<ChampionClass, int>();
        private Dictionary<ChampionOrigin, int> originDistribution = new Dictionary<ChampionOrigin, int>();
        private Dictionary<int, int> tierDistribution = new Dictionary<int, int>();
        
        // Statistics
        private float avgHealth, avgDamage, avgArmor, avgMana;
        private float minHealth, maxHealth, minDamage, maxDamage;
        private int totalChampions;
        
        private bool showDistribution = true;
        private bool showAverages = true;
        private bool showRanges = true;
        private bool showBalance = true;

        private void OnEnable()
        {
            RefreshStatistics();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Champion Statistics Overview", EditorStyles.largeLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Data"))
            {
                RefreshStatistics();
            }
            if (GUILayout.Button("Export Statistics"))
            {
                ExportStatistics();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawGeneralInfo();
            DrawDistributionStats();
            DrawAverageStats();
            DrawRangeStats();
            DrawBalanceAnalysis();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawGeneralInfo()
        {
            EditorGUILayout.LabelField("General Information", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Total Champions: {totalChampions}");
            EditorGUILayout.LabelField($"Unique Classes: {classDistribution.Count}");
            EditorGUILayout.LabelField($"Unique Origins: {originDistribution.Count}");
            EditorGUILayout.LabelField($"Tier Range: {(tierDistribution.Count > 0 ? tierDistribution.Keys.Min() : 0)} - {(tierDistribution.Count > 0 ? tierDistribution.Keys.Max() : 0)}");
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawDistributionStats()
        {
            showDistribution = EditorGUILayout.Foldout(showDistribution, "Distribution Statistics", true);
            if (!showDistribution) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Class Distribution
            EditorGUILayout.LabelField("Class Distribution", EditorStyles.miniBoldLabel);
            foreach (var kvp in classDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / totalChampions * 100f;
                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            
            EditorGUILayout.Space(5);
            
            // Origin Distribution
            EditorGUILayout.LabelField("Origin Distribution", EditorStyles.miniBoldLabel);
            foreach (var kvp in originDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / totalChampions * 100f;
                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            
            EditorGUILayout.Space(5);
            
            // Tier Distribution
            EditorGUILayout.LabelField("Tier Distribution", EditorStyles.miniBoldLabel);
            foreach (var kvp in tierDistribution.OrderBy(x => x.Key))
            {
                float percentage = (float)kvp.Value / totalChampions * 100f;
                EditorGUILayout.LabelField($"Tier {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawAverageStats()
        {
            showAverages = EditorGUILayout.Foldout(showAverages, "Average Statistics", true);
            if (!showAverages) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Average Health: {avgHealth:F1}");
            EditorGUILayout.LabelField($"Average Attack Damage: {avgDamage:F1}");
            EditorGUILayout.LabelField($"Average Armor: {avgArmor:F1}");
            EditorGUILayout.LabelField($"Average Max Mana: {avgMana:F1}");
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawRangeStats()
        {
            showRanges = EditorGUILayout.Foldout(showRanges, "Range Statistics", true);
            if (!showRanges) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Health Range: {minHealth:F1} - {maxHealth:F1}");
            EditorGUILayout.LabelField($"Damage Range: {minDamage:F1} - {maxDamage:F1}");
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawBalanceAnalysis()
        {
            showBalance = EditorGUILayout.Foldout(showBalance, "Balance Analysis", true);
            if (!showBalance) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Analyze balance issues
            var balanceIssues = AnalyzeBalance();
            
            if (balanceIssues.Count == 0)
            {
                EditorGUILayout.HelpBox("No major balance issues detected!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Found {balanceIssues.Count} potential balance issues:", MessageType.Warning);
                foreach (string issue in balanceIssues)
                {
                    EditorGUILayout.LabelField($"• {issue}");
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void RefreshStatistics()
        {
            allChampions.Clear();
            classDistribution.Clear();
            originDistribution.Clear();
            tierDistribution.Clear();
            
            // Load all champions
            string[] championGuids = AssetDatabase.FindAssets("t:ChampionData");
            
            foreach (string guid in championGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChampionData champion = AssetDatabase.LoadAssetAtPath<ChampionData>(path);
                if (champion != null)
                {
                    allChampions.Add(champion);
                }
            }
            
            totalChampions = allChampions.Count;
            
            if (totalChampions == 0) return;
            
            // Calculate distributions
            foreach (var champion in allChampions)
            {
                // Class distribution
                if (champion.classes != null)
                {
                    foreach (var championClass in champion.classes)
                    {
                        if (classDistribution.ContainsKey(championClass))
                            classDistribution[championClass]++;
                        else
                            classDistribution[championClass] = 1;
                    }
                }
                
                // Origin distribution
                if (champion.origins != null)
                {
                    foreach (var origin in champion.origins)
                    {
                        if (originDistribution.ContainsKey(origin))
                            originDistribution[origin]++;
                        else
                            originDistribution[origin] = 1;
                    }
                }
                
                // Tier distribution
                if (tierDistribution.ContainsKey(champion.tier))
                    tierDistribution[champion.tier]++;
                else
                    tierDistribution[champion.tier] = 1;
            }
            
            // Calculate averages
            avgHealth = allChampions.Average(c => c.baseAttr.health);
            avgDamage = allChampions.Average(c => c.baseAttr.attackDamage);
            avgArmor = allChampions.Average(c => c.baseAttr.armor);
            avgMana = allChampions.Average(c => c.baseAttr.maxMana);
            
            // Calculate ranges
            minHealth = allChampions.Min(c => c.baseAttr.health);
            maxHealth = allChampions.Max(c => c.baseAttr.health);
            minDamage = allChampions.Min(c => c.baseAttr.attackDamage);
            maxDamage = allChampions.Max(c => c.baseAttr.attackDamage);
        }

        private List<string> AnalyzeBalance()
        {
            var issues = new List<string>();
            
            if (totalChampions == 0) return issues;
            
            // Check class balance
            var mostCommonClass = classDistribution.OrderByDescending(x => x.Value).First();
            var leastCommonClass = classDistribution.OrderBy(x => x.Value).First();
            
            if (mostCommonClass.Value > leastCommonClass.Value * 3)
            {
                issues.Add($"Class imbalance: {mostCommonClass.Key} ({mostCommonClass.Value}) vs {leastCommonClass.Key} ({leastCommonClass.Value})");
            }
            
            // Check tier distribution
            if (tierDistribution.ContainsKey(1) && tierDistribution.ContainsKey(5))
            {
                int tier1Count = tierDistribution[1];
                int tier5Count = tierDistribution[5];
                
                if (tier1Count < tier5Count)
                {
                    issues.Add("More high-tier champions than low-tier ones - may affect game progression");
                }
            }
            
            // Check stat ranges
            float healthRatio = maxHealth / minHealth;
            if (healthRatio > 4.0f)
            {
                issues.Add($"Health range too wide: {healthRatio:F1}x difference between min and max");
            }
            
            float damageRatio = maxDamage / minDamage;
            if (damageRatio > 4.0f)
            {
                issues.Add($"Damage range too wide: {damageRatio:F1}x difference between min and max");
            }
            
            return issues;
        }

        private void ExportStatistics()
        {
            string path = EditorUtility.SaveFilePanel("Export Champion Statistics", "", "champion_stats", "txt");
            if (string.IsNullOrEmpty(path)) return;
            
            var content = new System.Text.StringBuilder();
            content.AppendLine("CHAMPION STATISTICS REPORT");
            content.AppendLine("=========================");
            content.AppendLine();
            
            content.AppendLine($"Total Champions: {totalChampions}");
            content.AppendLine();
            
            content.AppendLine("CLASS DISTRIBUTION:");
            foreach (var kvp in classDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / totalChampions * 100f;
                content.AppendLine($"  {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            content.AppendLine();
            
            content.AppendLine("ORIGIN DISTRIBUTION:");
            foreach (var kvp in originDistribution.OrderByDescending(x => x.Value))
            {
                float percentage = (float)kvp.Value / totalChampions * 100f;
                content.AppendLine($"  {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
            }
            content.AppendLine();
            
            content.AppendLine("AVERAGE STATS:");
            content.AppendLine($"  Health: {avgHealth:F1}");
            content.AppendLine($"  Attack Damage: {avgDamage:F1}");
            content.AppendLine($"  Armor: {avgArmor:F1}");
            content.AppendLine($"  Max Mana: {avgMana:F1}");
            content.AppendLine();
            
            var balanceIssues = AnalyzeBalance();
            if (balanceIssues.Count > 0)
            {
                content.AppendLine("BALANCE ISSUES:");
                foreach (string issue in balanceIssues)
                {
                    content.AppendLine($"  • {issue}");
                }
            }
            
            System.IO.File.WriteAllText(path, content.ToString());
            
            EditorUtility.DisplayDialog("Export Complete", $"Statistics exported to {path}", "OK");
        }
    }
}
