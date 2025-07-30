using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TacticsArena.UI
{
    /// <summary>
    /// Setup script cho toàn bộ TFT UI System (1920x1080)
    /// Tự động cấu hình và kết nối tất cả UI components
    /// </summary>
    public class TFTUISetup : MonoBehaviour
    {
        [Header("UI Assets")]
        public VisualTreeAsset mainUITemplate;
        public StyleSheet mainUIStyleSheet;
        
        [Header("Setup Options")]
        public bool autoSetupOnStart = true;
        public bool enableTooltips = true;
        public bool enableAnimations = true;
        public bool enableResponsiveDesign = true;
        
        [Header("Resolution Settings")]
        public Vector2Int targetResolution = new Vector2Int(1920, 1080);
        public bool forceResolution = false;
        
        private UIDocument uiDocument;
        private TFTMainUIController mainController;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTFTUI();
            }
        }
        
        [ContextMenu("Setup TFT UI System")]
        public void SetupTFTUI()
        {
            SetupResolution();
            SetupUIDocument();
            SetupMainController();
            ApplyCustomizations();
            
            Debug.Log("✅ TFT UI System setup completed successfully!");
        }
        
        private void SetupResolution()
        {
            if (forceResolution)
            {
                Screen.SetResolution(targetResolution.x, targetResolution.y, Screen.fullScreen);
                Debug.Log($"🖥️ Resolution set to {targetResolution.x}x{targetResolution.y}");
            }
        }
        
        private void SetupUIDocument()
        {
            // Get or create UIDocument
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }
            
            // Load UI template
            if (mainUITemplate != null)
            {
                uiDocument.visualTreeAsset = mainUITemplate;
                Debug.Log("📄 Main UI template loaded");
            }
            else
            {
                // Try to auto-find template
                mainUITemplate = Resources.Load<VisualTreeAsset>("UI/UXML/TFTMainUI");
                if (mainUITemplate != null)
                {
                    uiDocument.visualTreeAsset = mainUITemplate;
                    Debug.Log("📄 Auto-found and loaded main UI template");
                }
                else
                {
                    Debug.LogError("❌ Main UI template not found! Please assign TFTMainUI.uxml");
                    return;
                }
            }
            
            // Apply stylesheets
            if (mainUIStyleSheet != null)
            {
                if (uiDocument.rootVisualElement != null)
                {
                    uiDocument.rootVisualElement.styleSheets.Add(mainUIStyleSheet);
                    Debug.Log("🎨 Main UI stylesheet applied");
                }
            }
            else
            {
                // Try to auto-find stylesheet
                mainUIStyleSheet = Resources.Load<StyleSheet>("UI/Styles/TFTMainUI");
                if (mainUIStyleSheet != null)
                {
                    uiDocument.rootVisualElement.styleSheets.Add(mainUIStyleSheet);
                    Debug.Log("🎨 Auto-found and applied main UI stylesheet");
                }
                else
                {
                    Debug.LogWarning("⚠️ Main UI stylesheet not found! UI may not display correctly");
                }
            }
        }
        
        private void SetupMainController()
        {
            // Get or create main controller
            mainController = GetComponent<TFTMainUIController>();
            if (mainController == null)
            {
                mainController = gameObject.AddComponent<TFTMainUIController>();
            }
            
            // Configure controller settings
            mainController.mainUIDocument = uiDocument;
            mainController.mainUITemplate = mainUITemplate;
            mainController.enableTooltips = enableTooltips;
            mainController.enableAnimations = enableAnimations;
            
            Debug.Log("🎮 Main UI controller configured");
        }
        
        private void ApplyCustomizations()
        {
            if (uiDocument?.rootVisualElement == null) return;
            
            var root = uiDocument.rootVisualElement;
            
            // Apply responsive design if enabled
            if (enableResponsiveDesign)
            {
                SetupResponsiveDesign(root);
            }
            
            // Add custom visual enhancements
            AddVisualEnhancements(root);
            
            // Setup dynamic backgrounds
            SetupDynamicBackgrounds(root);
        }
        
        private void SetupResponsiveDesign(VisualElement root)
        {
            // Add responsive classes based on screen size
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            if (screenWidth <= 1366)
            {
                root.AddToClassList("small-screen");
            }
            else if (screenWidth <= 1600)
            {
                root.AddToClassList("medium-screen");
            }
            else
            {
                root.AddToClassList("large-screen");
            }
            
            // Add aspect ratio classes
            float aspectRatio = screenWidth / screenHeight;
            if (aspectRatio > 1.8f)
            {
                root.AddToClassList("ultrawide");
            }
            else if (aspectRatio < 1.5f)
            {
                root.AddToClassList("square");
            }
            
            Debug.Log($"📱 Responsive design applied for {screenWidth}x{screenHeight}");
        }
        
        private void AddVisualEnhancements(VisualElement root)
        {
            // Add glow effects to high-tier shop slots
            var shopSlots = root.Query<VisualElement>(className: "shop-slot").ToList();
            foreach (var slot in shopSlots)
            {
                // Add hover enhancement
                slot.RegisterCallback<MouseEnterEvent>(evt => 
                {
                    if (enableAnimations)
                    {
                        slot.AddToClassList("hover-enhanced");
                    }
                });
                
                slot.RegisterCallback<MouseLeaveEvent>(evt => 
                {
                    slot.RemoveFromClassList("hover-enhanced");
                });
            }
            
            // Add particle effects containers
            var mainContainer = root.Q<VisualElement>("tft-main-container");
            if (mainContainer != null)
            {
                var particleContainer = new VisualElement();
                particleContainer.name = "particle-effects";
                particleContainer.AddToClassList("particle-container");
                particleContainer.style.position = Position.Absolute;
                particleContainer.style.top = 0;
                particleContainer.style.left = 0;
                particleContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                particleContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                // particleContainer.style.pointerEvents = new StyleEnum<PointerEvents>(PointerEvents.None);
                
                mainContainer.Add(particleContainer);
            }
            
            Debug.Log("✨ Visual enhancements applied");
        }
        
        private void SetupDynamicBackgrounds(VisualElement root)
        {
            // Setup animated background for battle board
            var battleBoard = root.Q<VisualElement>("battle-board");
            if (battleBoard != null && enableAnimations)
            {
                // Add subtle breathing animation
                battleBoard.schedule.Execute(() => {
                    battleBoard.AddToClassList("board-breathe");
                }).ExecuteLater(1000);
                
                battleBoard.schedule.Execute(() => {
                    battleBoard.RemoveFromClassList("board-breathe");
                }).Every(4000).ExecuteLater(3000);
            }
            
            // Setup dynamic timer colors
            var timerValue = root.Q<Label>("timer-value");
            if (timerValue != null)
            {
                timerValue.schedule.Execute(() => {
                    UpdateTimerColors(timerValue);
                }).Every(1000);
            }
            
            Debug.Log("🌊 Dynamic backgrounds setup");
        }
        
        private void UpdateTimerColors(Label timerLabel)
        {
            // This would integrate with actual game timer
            // For now, just cycle through colors for demonstration
            var colors = new Color[]
            {
                new Color(1f, 0.84f, 0.39f), // Gold
                new Color(1f, 0.6f, 0.2f),  // Orange  
                new Color(1f, 0.3f, 0.3f)   // Red
            };
            
            int colorIndex = (int)(Time.time) % colors.Length;
            timerLabel.style.color = new StyleColor(colors[colorIndex]);
        }
        
        [ContextMenu("Test UI Animations")]
        public void TestAnimations()
        {
            if (uiDocument?.rootVisualElement == null)
            {
                Debug.LogWarning("UI not setup yet!");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            
            // Test shop slot animations
            var shopSlots = root.Query<VisualElement>(className: "shop-slot").ToList();
            foreach (var slot in shopSlots)
            {
                slot.AddToClassList("highlight");
                slot.schedule.Execute(() => {
                    slot.RemoveFromClassList("highlight");
                }).ExecuteLater(1000);
            }
            
            // Test timer animation
            var timerFill = root.Q<VisualElement>("timer-fill");
            if (timerFill != null)
            {
                timerFill.style.width = new StyleLength(new Length(10, LengthUnit.Percent));
                timerFill.schedule.Execute(() => {
                    timerFill.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                }).ExecuteLater(500);
            }
            
            Debug.Log("🎭 Animation test completed");
        }
        
        [ContextMenu("Auto-Find UI Assets")]
        public void AutoFindAssets()
        {
            // Try to find UXML template
            if (mainUITemplate == null)
            {
                #if UNITY_EDITOR
                string[] uiGUIDs = UnityEditor.AssetDatabase.FindAssets("TFTMainUI t:VisualTreeAsset");
                if (uiGUIDs.Length > 0)
                {
                    string uiPath = UnityEditor.AssetDatabase.GUIDToAssetPath(uiGUIDs[0]);
                    mainUITemplate = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uiPath);
                    Debug.Log($"📄 Found UI template: {uiPath}");
                }
                #endif
            }
            
            // Try to find USS stylesheet
            if (mainUIStyleSheet == null)
            {
                #if UNITY_EDITOR
                string[] cssGUIDs = UnityEditor.AssetDatabase.FindAssets("TFTMainUI t:StyleSheet");
                if (cssGUIDs.Length > 0)
                {
                    string cssPath = UnityEditor.AssetDatabase.GUIDToAssetPath(cssGUIDs[0]);
                    mainUIStyleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(cssPath);
                    Debug.Log($"🎨 Found UI stylesheet: {cssPath}");
                }
                #endif
            }
            
            if (mainUITemplate != null && mainUIStyleSheet != null)
            {
                Debug.Log("✅ All UI assets found successfully!");
            }
            else
            {
                Debug.LogWarning("⚠️ Some UI assets not found. Check file names and locations.");
            }
        }
        
        [ContextMenu("Apply Vietnamese Localization")]
        public void ApplyVietnameseLocalization()
        {
            if (uiDocument?.rootVisualElement == null)
            {
                Debug.LogWarning("UI not setup yet!");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            
            // Localize button texts
            var buttons = new Dictionary<string, string>
            {
                { "refresh-btn", "🔄 Làm Mới (2💰)" },
                { "lock-btn", "🔒 Khóa" },
                { "all-in-btn", "🎰 All-In" },
                { "buy-exp-btn", "🎓 Mua KN (4💰)" },
                { "settings-btn", "⚙️" },
                { "surrender-btn", "🏳️" },
                { "exit-btn", "❌" }
            };
            
            foreach (var kvp in buttons)
            {
                var button = root.Q<Button>(kvp.Key);
                if (button != null)
                {
                    button.text = kvp.Value;
                }
            }
            
            // Localize labels
            var labels = new Dictionary<string, string>
            {
                { "round-label", "Hiệp 1-1" },
                { "phase-label", "Chuẩn Bị" },
                { "player-name", "Người Chơi" },
                { "shop-level", "Cấp 1" }
            };
            
            foreach (var kvp in labels)
            {
                var label = root.Q<Label>(kvp.Key);
                if (label != null)
                {
                    label.text = kvp.Value;
                }
            }
            
            Debug.Log("🇻🇳 Vietnamese localization applied");
        }
        
        [ContextMenu("Validate UI Setup")]
        public void ValidateSetup()
        {
            bool isValid = true;
            
            // Check UIDocument
            if (uiDocument == null)
            {
                Debug.LogError("❌ UIDocument component missing");
                isValid = false;
            }
            
            // Check UI template
            if (mainUITemplate == null)
            {
                Debug.LogError("❌ Main UI template not assigned");
                isValid = false;
            }
            
            // Check stylesheet
            if (mainUIStyleSheet == null)
            {
                Debug.LogWarning("⚠️ Main UI stylesheet not assigned");
            }
            
            // Check main controller
            if (mainController == null)
            {
                Debug.LogWarning("⚠️ Main UI controller not found");
            }
            
            // Check resolution
            if (Screen.width != targetResolution.x || Screen.height != targetResolution.y)
            {
                Debug.LogWarning($"⚠️ Current resolution ({Screen.width}x{Screen.height}) differs from target ({targetResolution.x}x{targetResolution.y})");
            }
            
            if (isValid)
            {
                Debug.Log("✅ UI setup validation passed!");
            }
            else
            {
                Debug.LogError("❌ UI setup validation failed!");
            }
        }
        
        private void OnValidate()
        {
            // Auto-find assets when values change in inspector
            if (mainUITemplate == null || mainUIStyleSheet == null)
            {
                AutoFindAssets();
            }
        }
        
        #region Public API
        
        public void EnableTooltips(bool enable)
        {
            enableTooltips = enable;
            if (mainController != null)
            {
                mainController.enableTooltips = enable;
            }
        }
        
        public void EnableAnimations(bool enable)
        {
            enableAnimations = enable;
            if (mainController != null)
            {
                mainController.enableAnimations = enable;
            }
        }
        
        public void SetResolution(int width, int height)
        {
            targetResolution = new Vector2Int(width, height);
            if (forceResolution)
            {
                Screen.SetResolution(width, height, Screen.fullScreen);
            }
        }
        
        #endregion
    }
}
