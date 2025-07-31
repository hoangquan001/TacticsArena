using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using TacticsArena.Battle;

namespace TacticsArena.UI
{
    /// <summary>
    /// FloatingTextManager quản lý hiệu ứng text bay lên khi champion nhận damage/heal.
    /// Sử dụng object pooling cho performance tốt.
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        [Header("Settings")]
        public Camera worldCamera;
        public UIDocument uiDocument;
        
        [Header("Animation Settings")]
        public float floatDistance = 100f;
        public float floatDuration = 1.5f;
        public float fadeOutDuration = 0.5f;
        public AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Text Settings")]
        public int damageTextSize = 24;
        public int healTextSize = 20;
        public Color damageColor = Color.red;
        public Color healColor = Color.green;
        public Color criticalColor = Color.yellow;
        public Color missColor = Color.gray;
        
        [Header("Performance")]
        public int maxActiveTexts = 10;
        public int poolSize = 20;
        
        // Internal
        private List<FloatingText> textPool = new List<FloatingText>();
        private List<FloatingText> activeTexts = new List<FloatingText>();
        private VisualElement rootElement;
        
        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
                
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }
        
        private void Start()
        {
            InitializeUI();
            CreateTextPool();
        }
        
        private void InitializeUI()
        {
            if (uiDocument != null)
            {
                rootElement = uiDocument.rootVisualElement;
            }
            else
            {
                Debug.LogError("FloatingTextManager: UIDocument not found!");
            }
        }
        
        private void CreateTextPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                FloatingText floatingText = CreateFloatingText();
                floatingText.gameObject.SetActive(false);
                textPool.Add(floatingText);
            }
        }
        
        private FloatingText CreateFloatingText()
        {
            GameObject textObject = new GameObject("FloatingText");
            textObject.transform.SetParent(transform);
            
            FloatingText floatingText = textObject.AddComponent<FloatingText>();
            floatingText.Initialize(this);
            
            return floatingText;
        }
        
        public void ShowDamageText(Vector3 worldPosition, float damage, bool isCritical = false)
        {
            string text = Mathf.RoundToInt(damage).ToString();
            Color color = isCritical ? criticalColor : damageColor;
            int fontSize = isCritical ? damageTextSize + 4 : damageTextSize;
            
            ShowFloatingText(worldPosition, text, color, fontSize);
        }
        
        public void ShowHealText(Vector3 worldPosition, float heal)
        {
            string text = "+" + Mathf.RoundToInt(heal).ToString();
            ShowFloatingText(worldPosition, text, healColor, healTextSize);
        }
        
        public void ShowMissText(Vector3 worldPosition)
        {
            ShowFloatingText(worldPosition, "MISS", missColor, damageTextSize);
        }
        
        public void ShowCustomText(Vector3 worldPosition, string text, Color color, int fontSize = 20)
        {
            ShowFloatingText(worldPosition, text, color, fontSize);
        }
        
        private void ShowFloatingText(Vector3 worldPosition, string text, Color color, int fontSize)
        {
            if (activeTexts.Count >= maxActiveTexts)
            {
                // Remove oldest text if limit reached
                ReturnTextToPool(activeTexts[0]);
            }
            
            FloatingText floatingText = GetTextFromPool();
            if (floatingText != null)
            {
                floatingText.gameObject.SetActive(true);
                floatingText.Show(worldPosition, text, color, fontSize);
                activeTexts.Add(floatingText);
            }
        }
        
        private FloatingText GetTextFromPool()
        {
            for (int i = 0; i < textPool.Count; i++)
            {
                if (!textPool[i].gameObject.activeInHierarchy)
                {
                    FloatingText text = textPool[i];
                    textPool.RemoveAt(i);
                    return text;
                }
            }
            
            // Create new if pool empty
            return CreateFloatingText();
        }
        
        public void ReturnTextToPool(FloatingText floatingText)
        {
            if (floatingText == null) return;
            
            activeTexts.Remove(floatingText);
            floatingText.gameObject.SetActive(false);
            
            if (!textPool.Contains(floatingText))
            {
                textPool.Add(floatingText);
            }
        }
        
        // Helper method to convert world position to UI position
        public Vector2 WorldToUIPosition(Vector3 worldPosition)
        {
            if (worldCamera == null || rootElement == null)
                return Vector2.zero;
            
            Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPosition);
            
            if (screenPos.z < 0) // Behind camera
                return Vector2.zero;
            
            Vector2 uiPos = RuntimePanelUtils.ScreenToPanel(
                rootElement.panel,
                new Vector2(screenPos.x, Screen.height - screenPos.y)
            );
            
            return uiPos;
        }
        
        // Static methods for easy access
        private static FloatingTextManager instance;
        public static FloatingTextManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<FloatingTextManager>();
                return instance;
            }
        }
        
        public static void ShowDamage(Vector3 worldPosition, float damage, bool isCritical = false)
        {
            if (Instance != null)
                Instance.ShowDamageText(worldPosition, damage, isCritical);
        }
        
        public static void ShowHeal(Vector3 worldPosition, float heal)
        {
            if (Instance != null)
                Instance.ShowHealText(worldPosition, heal);
        }
        
        public static void ShowMiss(Vector3 worldPosition)
        {
            if (Instance != null)
                Instance.ShowMissText(worldPosition);
        }
        
        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
    
    /// <summary>
    /// Individual floating text component
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        private FloatingTextManager manager;
        private Label textLabel;
        private Coroutine animationCoroutine;
        private Vector3 startWorldPosition;
        
        public void Initialize(FloatingTextManager textManager)
        {
            manager = textManager;
        }
        
        public void Show(Vector3 worldPosition, string text, Color color, int fontSize)
        {
            startWorldPosition = worldPosition;
            
            // Create or update UI label
            
            // Start animation
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            
            animationCoroutine = StartCoroutine(AnimateText());
        }
        
    
        private IEnumerator AnimateText()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < manager.floatDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / manager.floatDuration;
                
                // Update position
                Vector3 currentWorldPos = startWorldPosition + Vector3.up * (manager.floatCurve.Evaluate(progress) * manager.floatDistance * 0.01f);
                Vector2 uiPos = manager.WorldToUIPosition(currentWorldPos);
                
                if (textLabel != null)
                {
                    textLabel.style.left = uiPos.x;
                    textLabel.style.top = uiPos.y;
                    
                    // Fade out in the last portion
                    if (progress > (1f - manager.fadeOutDuration / manager.floatDuration))
                    {
                        float fadeProgress = (progress - (1f - manager.fadeOutDuration / manager.floatDuration)) / (manager.fadeOutDuration / manager.floatDuration);
                        float alpha = 1f - fadeProgress;
                        
                        Color currentColor = textLabel.style.color.value;
                        currentColor.a = alpha;
                        textLabel.style.color = currentColor;
                    }
                }
                
                yield return null;
            }
            
            // Hide and return to pool
            if (textLabel != null)
            {
                textLabel.style.display = DisplayStyle.None;
                textLabel.RemoveFromHierarchy();
                textLabel = null;
            }
            
            manager.ReturnTextToPool(this);
        }
        
        private void OnDestroy()
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
                
            if (textLabel != null)
            {
                textLabel.RemoveFromHierarchy();
            }
        }
    }
}
