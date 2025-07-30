using UnityEngine;
using UnityEngine.UIElements;

namespace TacticsArena.UI
{
    /// <summary>
    /// Component để setup Shop UI cho TFT game
    /// Attach vào GameObject với UIDocument component
    /// </summary>
    public class ShopUISetup : MonoBehaviour
    {
        [Header("UI Assets")]
        public VisualTreeAsset shopUITemplate;
        public StyleSheet shopStyleSheet;
        
        [Header("Auto Setup")]
        public bool autoSetupOnStart = true;
        
        private UIDocument uiDocument;
        private ShopUIController shopController;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupShopUI();
            }
        }
        
        [ContextMenu("Setup Shop UI")]
        public void SetupShopUI()
        {
            // Get or create UIDocument
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }
            
            // Load UXML template
            if (shopUITemplate != null)
            {
                uiDocument.visualTreeAsset = shopUITemplate;
            }
            else
            {
                Debug.LogWarning("Shop UI Template not assigned! Please assign the UXML file.");
                return;
            }
            
            // Add stylesheet
            if (shopStyleSheet != null)
            {
                if (uiDocument.rootVisualElement != null)
                {
                    uiDocument.rootVisualElement.styleSheets.Add(shopStyleSheet);
                }
            }
            else
            {
                Debug.LogWarning("Shop StyleSheet not assigned! Please assign the USS file.");
            }
            
            // Get or create ShopUIController
            shopController = GetComponent<ShopUIController>();
            if (shopController == null)
            {
                shopController = gameObject.AddComponent<ShopUIController>();
            }
            
            // Assign references
            shopController.uiDocument = uiDocument;
            shopController.shopUITemplate = shopUITemplate;
            
            Debug.Log("Shop UI setup completed!");
        }
        
        [ContextMenu("Load UI Assets")]
        public void LoadUIAssets()
        {
            // Try to find UI assets automatically
            if (shopUITemplate == null)
            {
                shopUITemplate = Resources.Load<VisualTreeAsset>("UI/UXML/ShopUI");
                if (shopUITemplate == null)
                {
                    Debug.LogWarning("Could not find ShopUI.uxml in Resources/UI/UXML/");
                }
            }
            
            if (shopStyleSheet == null)
            {
                shopStyleSheet = Resources.Load<StyleSheet>("UI/Styles/ShopUI");
                if (shopStyleSheet == null)
                {
                    Debug.LogWarning("Could not find ShopUI.uss in Resources/UI/Styles/");
                }
            }
            
            if (shopUITemplate != null && shopStyleSheet != null)
            {
                Debug.Log("UI Assets loaded successfully!");
                SetupShopUI();
            }
        }
        
        private void OnValidate()
        {
            // Auto-assign if in editor and assets are available
            #if UNITY_EDITOR
            if (shopUITemplate == null || shopStyleSheet == null)
            {
                // Try to find assets in the project
                string[] uiGUIDs = UnityEditor.AssetDatabase.FindAssets("ShopUI t:VisualTreeAsset");
                if (uiGUIDs.Length > 0)
                {
                    string uiPath = UnityEditor.AssetDatabase.GUIDToAssetPath(uiGUIDs[0]);
                    shopUITemplate = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uiPath);
                }
                
                string[] cssGUIDs = UnityEditor.AssetDatabase.FindAssets("ShopUI t:StyleSheet");
                if (cssGUIDs.Length > 0)
                {
                    string cssPath = UnityEditor.AssetDatabase.GUIDToAssetPath(cssGUIDs[0]);
                    shopStyleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(cssPath);
                }
            }
            #endif
        }
    }
}
