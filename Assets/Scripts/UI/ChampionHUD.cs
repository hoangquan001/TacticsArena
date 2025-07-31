using UnityEngine;
using UnityEngine.UI;
using TacticsArena.Battle;
using TMPro;

namespace TacticsArena.UI
{
    public class ChampionHUD : MonoBehaviour
    {
        [Header("References")]
        public Champion champion;
        public Canvas hudCanvas;
        public Camera worldCamera;
        
        [Header("HUD Settings")]
        public Vector3 worldOffset = new Vector3(0, 2f, 0);
        public bool followChampion = true;
        public bool hideWhenDead = true;
        public bool hideWhenFullHealth = false;
        public bool faceCamera = true;
        
        [Header("Animation Settings")]
        public float updateSmoothness = 0.1f;
        public float fadeSpeed = 2f;
        
        [Header("UI Elements")]
        public GameObject hudContainer;
        public Slider healthBar;
        public Slider manaBar;
        
        [Header("Visual Settings")]
        public Color playerHealthColor = Color.green;
        public Color enemyHealthColor = Color.red;
        public Color manaColor = Color.blue;
        public Color playerNameColor = Color.cyan;
        public Color enemyNameColor = Color.white;
        
        // Internal state
        private float targetHealthPercent;
        private float targetManaPercent;
        private float currentHealthPercent;
        private float currentManaPercent;
        private bool isVisible = true;
        private CanvasGroup canvasGroup;
        
        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
                
            if (hudCanvas == null)
                hudCanvas = GetComponentInChildren<Canvas>();
                
            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        
        private void Start()
        {
            InitializeHUD();
            if (champion != null)
            {
                SetupChampionReference(champion);
            }
        }
        
        private void Update()
        {
            if (champion == null) return;
            
            UpdateHUDPosition();
            UpdateHUDValues();
            UpdateVisibility();
            
            if (faceCamera && worldCamera != null)
            {
                UpdateHUDRotation();
            }
        }
        
        private void InitializeHUD()
        {
            // Setup canvas
            if (hudCanvas != null)
            {
                hudCanvas.renderMode = RenderMode.WorldSpace;
                hudCanvas.worldCamera = worldCamera;
            }
            
            // Initialize values
            currentHealthPercent = 1f;
            currentManaPercent = 0f;
            targetHealthPercent = 1f;
            targetManaPercent = 0f;
            
            // Setup initial styling
            SetupInitialStyling();
        }
        
        private void SetupInitialStyling()
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(true);
            }
            
            // Set initial bar values
            if (healthBar != null)
            {
                healthBar.value = 1f;
                healthBar.maxValue = 1f;
                healthBar.minValue = 0f;
            }
            
            if (manaBar != null)
            {
                manaBar.value = 0f;
                manaBar.maxValue = 1f;
                manaBar.minValue = 0f;
            }
        }
        
        public void SetupChampionReference(Champion newChampion)
        {
            champion = newChampion;
            
            if (champion == null) return;
            
            // Update champion info
            UpdateChampionInfo();
            
            // Subscribe to champion events
            SubscribeToChampionEvents();
        }
        
        private void SubscribeToChampionEvents()
        {
            if (champion == null) return;
            
            // Subscribe to health/mana change events if they exist
            champion.OnHealthChanged += OnHealthChanged;
            champion.OnManaChanged += OnManaChanged;
        }
        
        private void UnsubscribeFromChampionEvents()
        {
            if (champion == null) return;
            
            champion.OnHealthChanged -= OnHealthChanged;
            champion.OnManaChanged -= OnManaChanged;
        }
        
        private void OnHealthChanged(float newHealth)
        {
            // This will be handled in UpdateHUDValues
        }
        
        private void OnManaChanged(float newMana)
        {
            // This will be handled in UpdateHUDValues
        }
        
        private void UpdateChampionInfo()
        {
            if (champion == null || champion.data == null) return;
            
        
            
            // Update team coloring
            UpdateTeamColoring();
        }
        

        
        private void UpdateTeamColoring()
        {
            if (champion == null) return;
            
            Color nameColor = champion.teamId == 0 ? playerNameColor : enemyNameColor;
            Color healthColor = champion.teamId == 0 ? playerHealthColor : enemyHealthColor;
            
        
            
            // Update health bar color
            if (healthBar != null && healthBar.fillRect != null)
            {
                Image fillImage = healthBar.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = healthColor;
                }
            }
            
            // Update mana bar color
            if (manaBar != null && manaBar.fillRect != null)
            {
                Image fillImage = manaBar.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = manaColor;
                }
            }
        }
        
        private void UpdateHUDPosition()
        {
            if (!followChampion || champion == null || worldCamera == null)
                return;
            
            // Calculate world position
            Vector3 targetPosition = champion.transform.position + worldOffset;
            
            // Update HUD position
            transform.position = targetPosition;
        }

        [ContextMenu("Update HUD Rotation")]
        private void UpdateHUDRotation()
        {
            if (worldCamera == null) return;
            
            // Face the camera
            Vector3 directionToCamera = worldCamera.transform.position - transform.position;
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(worldCamera.transform.forward, Vector3.up);
            }
        }
        
        private void UpdateHUDValues()
        {
            if (champion == null) return;
            
            // Calculate target percentages
            float maxHealth = champion.data != null ? champion.data.baseAttr.health : 100f;
            float maxMana = champion.data != null ? champion.data.baseAttr.maxMana : 100f;
            
            targetHealthPercent = Mathf.Clamp01(champion.currentHealth / maxHealth);
            targetManaPercent = maxMana > 0 ? Mathf.Clamp01(champion.currentMana / maxMana) : 0f;
            
            // Smooth interpolation
            currentHealthPercent = Mathf.Lerp(currentHealthPercent, targetHealthPercent, updateSmoothness);
            currentManaPercent = Mathf.Lerp(currentManaPercent, targetManaPercent, updateSmoothness);
            
            // Update UI elements
            UpdateHealthBar();
            UpdateManaBar();
        }
        
        private void UpdateHealthBar()
        {
            if (healthBar == null) return;
            
            healthBar.value = currentHealthPercent;
            
            // Update health bar color based on percentage
            if (healthBar.fillRect != null)
            {
                Image fillImage = healthBar.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    Color baseColor = champion.teamId == 0 ? playerHealthColor : enemyHealthColor;
                    
                    if (currentHealthPercent > 0.6f)
                        fillImage.color = baseColor;
                    else if (currentHealthPercent > 0.3f)
                        fillImage.color = Color.Lerp(Color.red, baseColor, 0.5f);
                    else
                        fillImage.color = Color.red;
                }
            }
        }
        
        private void UpdateManaBar()
        {
            if (manaBar == null) return;
            
            manaBar.value = currentManaPercent;
        }
        
    
        
        private void UpdateVisibility()
        {
            if (champion == null) return;
            
            bool shouldBeVisible = true;
            
            // Hide when dead
            if (hideWhenDead && !champion.isAlive)
                shouldBeVisible = false;
            
            // Hide when full health
            if (hideWhenFullHealth && targetHealthPercent >= 0.99f)
                shouldBeVisible = false;
            
            // Update visibility
            SetHUDVisible(shouldBeVisible);
        }
        
        private void SetHUDVisible(bool visible)
        {
            if (isVisible == visible) return;
            
            isVisible = visible;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else if (hudContainer != null)
            {
                hudContainer.SetActive(visible);
            }
        }
        
        // Public methods for external control
        public void SetWorldOffset(Vector3 offset)
        {
            worldOffset = offset;
        }
        
        public void SetFollowMode(bool follow)
        {
            followChampion = follow;
        }
        
        public void SetFaceCamera(bool face)
        {
            faceCamera = face;
        }
        
        public void ForceUpdatePosition()
        {
            UpdateHUDPosition();
            if (faceCamera)
                UpdateHUDRotation();
        }
        
        public void ForceUpdateValues()
        {
            if (champion == null) return;
            
            // Force immediate update without smoothing
            float maxHealth = champion.data != null ? champion.data.baseAttr.health : 100f;
            float maxMana = champion.data != null ? champion.data.baseAttr.maxMana : 100f;
            
            targetHealthPercent = Mathf.Clamp01(champion.currentHealth / maxHealth);
            targetManaPercent = maxMana > 0 ? Mathf.Clamp01(champion.currentMana / maxMana) : 0f;
            
            currentHealthPercent = targetHealthPercent;
            currentManaPercent = targetManaPercent;
            
            UpdateHealthBar();
            UpdateManaBar();
        }
        
        
        // Cleanup
        private void OnDestroy()
        {
            UnsubscribeFromChampionEvents();
        }
        
        // Context menu for testing
        [ContextMenu("Test Update HUD")]
        private void TestUpdateHUD()
        {
            if (champion != null)
            {
                ForceUpdateValues();
                ForceUpdatePosition();
            }
        }
        
        [ContextMenu("Toggle Face Camera")]
        private void ToggleFaceCamera()
        {
            faceCamera = !faceCamera;
            Debug.Log($"Face Camera: {faceCamera}");
        }
    }
}
