using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using TacticsArena.Champions;
using TacticsArena.Core;
using TacticsArena.Shop;

namespace TacticsArena.UI
{
    public class ShopUIController : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument uiDocument;
        public VisualTreeAsset shopUITemplate;
        
        [Header("Champion Icons")]
        public Texture2D[] championIcons;
        
        [Header("References")]
        public Player player;
        public ShopManager shopManager;
        public GameManager gameManager;
        
        // UI Elements
        private VisualElement shopContainer;
        private Label healthValue;
        private Label goldValue;
        private Label levelValue;
        private Label roundValue;
        private Label interestValue;
        private Label streakValue;
        private Label expLabelRight;
        private VisualElement expBarFill;
        private Label timerLabel;
        private Label timerValue;
        private Button refreshButton;
        private Button lockButton;
        private Button buyExpButton;
        private Button allInButton;
        
        private List<VisualElement> championSlots = new List<VisualElement>();
        private List<ChampionData> currentShopData = new List<ChampionData>();
        
        private bool isLocked = false;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            UpdateUI();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeUI()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component not found!");
                return;
            }
            
            // Get references to UI elements
            var root = uiDocument.rootVisualElement;
            shopContainer = root.Q<VisualElement>("shop-container");
            
            // Player info elements
            healthValue = root.Q<Label>("health-value");
            goldValue = root.Q<Label>("gold-value");
            levelValue = root.Q<Label>("level-value");
            roundValue = root.Q<Label>("round-value");
            interestValue = root.Q<Label>("interest-value");
            streakValue = root.Q<Label>("streak-value");
            
            // Experience elements
            expLabelRight = root.Q<Label>("exp-label-right");
            expBarFill = root.Q<VisualElement>("exp-bar-fill");
            
            // Timer elements
            timerLabel = root.Q<Label>("timer-label");
            timerValue = root.Q<Label>("timer-value");
            
            // Button elements
            refreshButton = root.Q<Button>("refresh-button");
            lockButton = root.Q<Button>("lock-button");
            buyExpButton = root.Q<Button>("buy-exp-button");
            allInButton = root.Q<Button>("all-in-button");
            
            // Champion slots
            for (int i = 1; i <= 5; i++)
            {
                var slot = root.Q<VisualElement>($"champion-slot-{i}");
                if (slot != null)
                {
                    championSlots.Add(slot);
                    int slotIndex = i - 1; // Convert to 0-based index
                    slot.RegisterCallback<ClickEvent>(evt => OnChampionSlotClicked(slotIndex));
                }
            }
            
            // Setup button callbacks
            refreshButton?.RegisterCallback<ClickEvent>(OnRefreshClicked);
            lockButton?.RegisterCallback<ClickEvent>(OnLockClicked);
            buyExpButton?.RegisterCallback<ClickEvent>(OnBuyExpClicked);
            allInButton?.RegisterCallback<ClickEvent>(OnAllInClicked);
            
            Debug.Log("Shop UI initialized successfully!");
        }
        
        private void SubscribeToEvents()
        {
            if (shopManager != null)
            {
                ShopManager.ShopRefreshedEvent += UpdateShopDisplay;
                ShopManager.ChampionPurchasedEvent += OnChampionPurchased;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            ShopManager.ShopRefreshedEvent -= UpdateShopDisplay;
            ShopManager.ChampionPurchasedEvent -= OnChampionPurchased;
        }
        
        private void Update()
        {
            UpdateTimerDisplay();
            UpdatePlayerInfo();
        }
        
        private void UpdateUI()
        {
            UpdatePlayerInfo();
            UpdateShopDisplay(currentShopData);
            UpdateButtonStates();
        }
        
        private void UpdatePlayerInfo()
        {
            if (player == null) return;
            
            healthValue.text = player.health.ToString();
            goldValue.text = player.gold.ToString();
            levelValue.text = player.level.ToString();
            
            // Calculate and display interest
            int interest = Mathf.Min(player.gold / 10, 5);
            if (interestValue != null)
                interestValue.text = interest.ToString();
            
            // Display win streak (placeholder for now)
            if (streakValue != null)
                streakValue.text = "0";
            
            // Update experience bar
            int currentExp = player.experience;
            int requiredExp = GetRequiredExperience(player.level + 1);
            float expProgress = (float)currentExp / requiredExp;

            expLabelRight.text = $"{currentExp}/{requiredExp}";
            if (expBarFill != null)
            {
                expBarFill.style.width = Length.Percent(expProgress * 100);
            }
            
            // Update round info
            if (gameManager != null)
            {
                roundValue.text = $"{gameManager.currentRound}";
            }
        }
        
        private void UpdateTimerDisplay()
        {
            if (gameManager == null) return;
            
            // Convert state to Vietnamese
            string stateText = gameManager.currentState switch
            {
                GameState.Preparation => "Chuẩn bị",
                GameState.Battle => "Chiến đấu",
                GameState.PostBattle => "Kết thúc",
                _ => "Đang chờ"
            };
            
            timerLabel.text = stateText;

            // In a real implementation, you'd get the actual timer value
            timerValue.text = "30";
        }
        
        private void UpdateShopDisplay(List<ChampionData> shopChampions)
        {
            currentShopData = shopChampions ?? new List<ChampionData>();
            
            for (int i = 0; i < championSlots.Count; i++)
            {
                var slot = championSlots[i];
                
                if (i < currentShopData.Count)
                {
                    UpdateChampionSlot(slot, currentShopData[i], i);
                    slot.style.display = DisplayStyle.Flex;
                }
                else
                {
                    slot.style.display = DisplayStyle.None;
                }
            }
        }
        
        private void UpdateChampionSlot(VisualElement slot, ChampionData championData, int slotIndex)
        {
            if (slot == null || championData == null) return;
            
            // Update champion name
            var nameLabel = slot.Q<Label>($"champion-name-{slotIndex + 1}");
            nameLabel.text = championData.championName;
            
            // Update cost with gold emoji
            var costLabel = slot.Q<Label>($"champion-cost-{slotIndex + 1}");
            costLabel.text = $"{championData.cost}💰";
            
            // Update tier styling
            slot.RemoveFromClassList("champion-slot--tier-1");
            slot.RemoveFromClassList("champion-slot--tier-2");
            slot.RemoveFromClassList("champion-slot--tier-3");
            slot.RemoveFromClassList("champion-slot--tier-4");
            slot.RemoveFromClassList("champion-slot--tier-5");
            slot.AddToClassList($"champion-slot--tier-{championData.tier}");
            
            // Update champion icon
            var iconElement = slot.Q<VisualElement>($"champion-icon-{slotIndex + 1}");
            if (iconElement != null && championData.championIcon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(championData.championIcon);
            }
            
            // Enable/disable based on gold
            bool canAfford = player != null && player.gold >= championData.cost;
            slot.SetEnabled(canAfford);
            
            if (!canAfford)
            {
                slot.AddToClassList("disabled");
            }
            else
            {
                slot.RemoveFromClassList("disabled");
            }
        }
        
        private void UpdateButtonStates()
        {
            if (player == null) return;
            
            // Refresh button
            bool canRefresh = player.gold >= 2 && !isLocked;
            refreshButton?.SetEnabled(canRefresh);
            
            // Buy XP button
            bool canBuyExp = player.gold >= 4;
            buyExpButton?.SetEnabled(canBuyExp);
            
            // Lock button with Vietnamese text
            if (lockButton != null)
            {
                lockButton.text = isLocked ? "🔓 Mở khóa" : "🔒 Khóa";
            }
        }
        
        // Event Handlers
        private void OnChampionSlotClicked(int slotIndex)
        {
            if (slotIndex >= currentShopData.Count) return;
            
            ChampionData championData = currentShopData[slotIndex];
            
            if (player != null && shopManager != null)
            {
                if (shopManager.BuyChampion(championData, player))
                {
                    Debug.Log($"Đã mua {championData.championName}!");
                    
                    // Remove champion from current display
                    currentShopData.RemoveAt(slotIndex);
                    UpdateShopDisplay(currentShopData);
                    UpdatePlayerInfo();
                    UpdateButtonStates();
                }
                else
                {
                    Debug.Log("Không thể mua tướng - không đủ vàng hoặc băng ghế đã đầy!");
                }
            }
        }
        
        private void OnRefreshClicked(ClickEvent evt)
        {
            if (player != null && shopManager != null && !isLocked)
            {
                if (shopManager.RefreshShopWithCost(player))
                {
                    UpdatePlayerInfo();
                    UpdateButtonStates();
                }
            }
        }
        
        private void OnLockClicked(ClickEvent evt)
        {
            isLocked = !isLocked;
            UpdateButtonStates();
            
            Debug.Log($"Shop {(isLocked ? "locked" : "unlocked")}");
        }
        
        private void OnBuyExpClicked(ClickEvent evt)
        {
            if (player != null && player.SpendGold(4))
            {
                player.AddExperience(4);
                UpdatePlayerInfo();
                UpdateButtonStates();
                
                Debug.Log("Bought 4 experience!");
            }
        }
        
        private void OnAllInClicked(ClickEvent evt)
        {
            // Implement all-in functionality (spend all gold on refreshes)
            if (player == null || shopManager == null) return;
            
            int refreshCount = 0;
            while (player.gold >= 2 && refreshCount < 20) // Limit to prevent infinite loop
            {
                if (shopManager.RefreshShopWithCost(player))
                {
                    refreshCount++;
                }
                else
                {
                    break;
                }
            }
            
            UpdatePlayerInfo();
            UpdateButtonStates();
            
            Debug.Log($"All-in: Refreshed {refreshCount} times!");
        }
        
        private void OnChampionPurchased(ChampionData championData)
        {
            // This is called when a champion is purchased
            UpdateButtonStates();
        }
        
        // Helper methods
        private int GetRequiredExperience(int targetLevel)
        {
            return targetLevel * 2; // Same formula as in Player.cs
        }
        
        // Public methods for external control
        public void ShowShop()
        {
            if (shopContainer != null)
                shopContainer.style.display = DisplayStyle.Flex;
        }
        
        public void HideShop()
        {
            if (shopContainer != null)
                shopContainer.style.display = DisplayStyle.None;
        }
        
        public void SetShopData(List<ChampionData> shopData)
        {
            UpdateShopDisplay(shopData);
        }
        
        // Auto-find references if not assigned
        private void AutoFindReferences()
        {
            if (player == null)
                player = FindFirstObjectByType<Player>();
                
            if (shopManager == null)
                shopManager = FindFirstObjectByType<ShopManager>();
                
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();
        }
        
        private void OnValidate()
        {
            AutoFindReferences();
        }
    }
}
