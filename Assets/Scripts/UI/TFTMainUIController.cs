using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using TacticsArena.Champions;
using TacticsArena.Core;
using TacticsArena.Shop;
using TacticsArena.Battle;

namespace TacticsArena.UI
{
    /// <summary>
    /// Main UI Controller cho toàn bộ TFT game interface (1920x1080)
    /// Quản lý tất cả UI components: Top Bar, Side Panels, Board, Shop, Tooltips
    /// </summary>
    public class TFTMainUIController : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument mainUIDocument;
        public VisualTreeAsset mainUITemplate;
        
        [Header("Game References")]
        public Player player;
        public GameManager gameManager;
        public ShopManager shopManager;
        public BattleManager battleManager;
        
        [Header("UI Settings")]
        public bool enableTooltips = true;
        public bool enableAnimations = true;
        public float tooltipDelay = 0.5f;
        
        // Root Elements
        private VisualElement root;
        private VisualElement tftMainContainer;
        
        // Top Bar Elements
        private Label playerName;
        private Label healthValue;
        private Label levelValue;
        private Label goldValue;
        private Label roundLabel;
        private Label phaseLabel;
        private Label timerValue;
        private VisualElement timerFill;
        private Button settingsBtn;
        private Button surrenderBtn;
        private Button exitBtn;
        
        // Side Panels
        private ScrollView synergiesScroll;
        private ScrollView historyScroll;
        private VisualElement itemComponents;
        private ScrollView completedItems;
        private VisualElement enemyInfo;
        private VisualElement enemyBoardMini;
        
        // Board & Bench
        private VisualElement battleBoard;
        private List<VisualElement> benchSlots = new List<VisualElement>();
        
        // Experience & Shop
        private VisualElement expBarFill;
        private Label expText;
        private Button buyExpBtn;
        private Button refreshBtn;
        private Button lockBtn;
        private Button allInBtn;
        private Label shopLevel;
        private Label shopOdds;
        private List<VisualElement> shopSlots = new List<VisualElement>();
        
        // Tooltips
        private VisualElement championTooltip;
        private VisualElement itemTooltip;
        private VisualElement settingsPanel;
        
        // State
        private bool isShopLocked = false;
        private List<ChampionData> currentShopData = new List<ChampionData>();
        private List<SynergyData> activeSynergies = new List<SynergyData>();
        private Coroutine tooltipCoroutine;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            UpdateAllUI();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeUI()
        {
            if (mainUIDocument == null)
                mainUIDocument = GetComponent<UIDocument>();
            
            if (mainUIDocument == null)
            {
                Debug.LogError("UIDocument component not found!");
                return;
            }
            
            root = mainUIDocument.rootVisualElement;
            CacheUIElements();
            SetupEventHandlers();
            SetupTooltips();
            
            Debug.Log("✅ TFT Main UI initialized successfully!");
        }
        
        private void CacheUIElements()
        {
            // Main Container
            tftMainContainer = root.Q<VisualElement>("tft-main-container");
            
            // Top Bar Elements
            playerName = root.Q<Label>("player-name");
            healthValue = root.Q<Label>("health-value");
            levelValue = root.Q<Label>("level-value");
            goldValue = root.Q<Label>("gold-value");
            roundLabel = root.Q<Label>("round-label");
            phaseLabel = root.Q<Label>("phase-label");
            timerValue = root.Q<Label>("timer-value");
            timerFill = root.Q<VisualElement>("timer-fill");
            
            // Control Buttons
            settingsBtn = root.Q<Button>("settings-btn");
            surrenderBtn = root.Q<Button>("surrender-btn");
            exitBtn = root.Q<Button>("exit-btn");
            
            // Side Panels
            synergiesScroll = root.Q<ScrollView>("synergies-scroll");
            historyScroll = root.Q<ScrollView>("history-scroll");
            itemComponents = root.Q<VisualElement>("item-components");
            completedItems = root.Q<ScrollView>("completed-items");
            enemyInfo = root.Q<VisualElement>("enemy-info");
            enemyBoardMini = root.Q<VisualElement>("enemy-board-mini");
            
            // Board & Bench
            battleBoard = root.Q<VisualElement>("battle-board");
            for (int i = 1; i <= 9; i++)
            {
                var benchSlot = root.Q<VisualElement>($"bench-slot-{i}");
                if (benchSlot != null)
                    benchSlots.Add(benchSlot);
            }
            
            // Experience & Shop
            expBarFill = root.Q<VisualElement>("exp-bar-fill");
            expText = root.Q<Label>("exp-text");
            buyExpBtn = root.Q<Button>("buy-exp-btn");
            refreshBtn = root.Q<Button>("refresh-btn");
            lockBtn = root.Q<Button>("lock-btn");
            allInBtn = root.Q<Button>("all-in-btn");
            shopLevel = root.Q<Label>("shop-level");
            shopOdds = root.Q<Label>("shop-odds");
            
            // Shop Slots
            for (int i = 1; i <= 5; i++)
            {
                var shopSlot = root.Q<VisualElement>($"shop-slot-{i}");
                if (shopSlot != null)
                    shopSlots.Add(shopSlot);
            }
            
            // Tooltips & Overlays
            championTooltip = root.Q<VisualElement>("champion-tooltip");
            itemTooltip = root.Q<VisualElement>("item-tooltip");
            settingsPanel = root.Q<VisualElement>("settings-panel");
        }
        
        private void SetupEventHandlers()
        {
            // Control Buttons
            settingsBtn?.RegisterCallback<ClickEvent>(OnSettingsClicked);
            surrenderBtn?.RegisterCallback<ClickEvent>(OnSurrenderClicked);
            exitBtn?.RegisterCallback<ClickEvent>(OnExitClicked);
            
            // Experience & Shop Controls
            buyExpBtn?.RegisterCallback<ClickEvent>(OnBuyExpClicked);
            refreshBtn?.RegisterCallback<ClickEvent>(OnRefreshClicked);
            lockBtn?.RegisterCallback<ClickEvent>(OnLockClicked);
            allInBtn?.RegisterCallback<ClickEvent>(OnAllInClicked);
            
            // Shop Slots
            for (int i = 0; i < shopSlots.Count; i++)
            {
                int slotIndex = i; // Capture for closure
                shopSlots[i]?.RegisterCallback<ClickEvent>(evt => OnShopChampionClicked(slotIndex));
            }
            
            // Bench Slots
            for (int i = 0; i < benchSlots.Count; i++)
            {
                int slotIndex = i; // Capture for closure
                benchSlots[i]?.RegisterCallback<ClickEvent>(evt => OnBenchSlotClicked(slotIndex));
            }
            
            // Settings Panel
            var closeSettingsBtn = root.Q<Button>("close-settings-btn");
            closeSettingsBtn?.RegisterCallback<ClickEvent>(OnCloseSettingsClicked);
            
            var applySettingsBtn = root.Q<Button>("apply-settings-btn");
            applySettingsBtn?.RegisterCallback<ClickEvent>(OnApplySettingsClicked);
        }
        
        private void SetupTooltips()
        {
            if (!enableTooltips) return;
            
            // Setup champion tooltips for shop
            foreach (var shopSlot in shopSlots)
            {
                shopSlot.RegisterCallback<MouseEnterEvent>(OnShopChampionHover);
                shopSlot.RegisterCallback<MouseLeaveEvent>(OnShopChampionLeave);
            }
            
            // Setup champion tooltips for bench
            foreach (var benchSlot in benchSlots)
            {
                benchSlot.RegisterCallback<MouseEnterEvent>(OnBenchChampionHover);
                benchSlot.RegisterCallback<MouseLeaveEvent>(OnBenchChampionLeave);
            }
        }
        
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                // Game state events would go here
            }
            
            if (shopManager != null)
            {
                ShopManager.ShopRefreshedEvent += OnShopRefreshed;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (shopManager != null)
            {
                ShopManager.ShopRefreshedEvent -= OnShopRefreshed;
            }
        }
        
        private void Update()
        {
            UpdateTimer();
            UpdatePlayerStats();
            UpdateShopState();
        }
        
        private void UpdateAllUI()
        {
            UpdatePlayerInfo();
            UpdateRoundInfo();
            UpdateExperience();
            UpdateShop();
            UpdateSynergies();
            UpdateBench();
            UpdateEnemyPreview();
        }
        
        #region Player Info Updates
        
        private void UpdatePlayerInfo()
        {
            if (player == null) return;
            
            if (playerName != null)
                playerName.text = player.playerName ?? "Người Chơi";
            
            if (healthValue != null)
                healthValue.text = player.health.ToString();
            
            if (levelValue != null)
                levelValue.text = player.level.ToString();
            
            if (goldValue != null)
                goldValue.text = player.gold.ToString();
        }
        
        private void UpdatePlayerStats()
        {
            UpdatePlayerInfo();
        }
        
        #endregion
        
        #region Round & Timer Updates
        
        private void UpdateRoundInfo()
        {
            if (gameManager == null) return;
            
            if (roundLabel != null)
                roundLabel.text = $"Hiệp {gameManager.currentRound}";
            
            if (phaseLabel != null)
            {
                string phaseText = gameManager.currentState switch
                {
                    GameState.Preparation => "Chuẩn Bị",
                    GameState.Battle => "Chiến Đấu",
                    GameState.PostBattle => "Kết Thúc",
                    _ => "Đang Chờ"
                };
                phaseLabel.text = phaseText;
            }
        }
        
        private void UpdateTimer()
        {
            if (gameManager == null) return;
            
            // In a real implementation, get actual timer from GameManager
            int timeRemaining = 30; // Placeholder
            
            if (timerValue != null)
            {
                timerValue.text = timeRemaining.ToString();
                
                // Add urgency styling
                if (timeRemaining <= 10)
                {
                    timerValue.AddToClassList("urgent");
                    timerValue.style.color = new StyleColor(Color.red);
                }
                else
                {
                    timerValue.RemoveFromClassList("urgent");
                    timerValue.style.color = new StyleColor(new Color(1f, 0.84f, 0.39f));
                }
            }
            
            if (timerFill != null)
            {
                float progress = timeRemaining / 30f; // Assuming 30s max
                timerFill.style.width = Length.Percent(progress * 100);
            }
        }
        
        #endregion
        
        #region Experience Updates
        
        private void UpdateExperience()
        {
            if (player == null) return;
            
            int currentExp = player.experience;
            int requiredExp = GetRequiredExperience(player.level + 1);
            float progress = (float)currentExp / requiredExp;
            
            if (expBarFill != null)
            {
                expBarFill.style.width = Length.Percent(progress * 100);
            }
            
            if (expText != null)
            {
                expText.text = $"{currentExp}/{requiredExp}";
            }
            
            // Update buy XP button state
            if (buyExpBtn != null)
            {
                bool canBuyExp = player.gold >= 4;
                buyExpBtn.SetEnabled(canBuyExp);
            }
        }
        
        private int GetRequiredExperience(int targetLevel)
        {
            return targetLevel * 2; // Same formula as Player.cs
        }
        
        #endregion
        
        #region Shop Updates
        
        private void UpdateShop()
        {
            if (shopManager == null) return;
            
            // Update shop level and odds
            if (shopLevel != null && player != null)
            {
                shopLevel.text = $"Cấp {player.level}";
            }
            
            if (shopOdds != null && player != null)
            {
                var odds = GetShopOdds(player.level);
                shopOdds.text = $"{odds[0]}% | {odds[1]}% | {odds[2]}% | {odds[3]}% | {odds[4]}%";
            }
            
            // Update shop champions
            UpdateShopChampions(currentShopData);
        }
        
        private void UpdateShopState()
        {
            if (player == null) return;
            
            // Update button states
            if (refreshBtn != null)
            {
                bool canRefresh = player.gold >= 2 && !isShopLocked;
                refreshBtn.SetEnabled(canRefresh);
            }
            
            if (lockBtn != null)
            {
                lockBtn.text = isShopLocked ? "🔓 Mở khóa" : "🔒 Khóa";
            }
            
            if (allInBtn != null)
            {
                bool canAllIn = player.gold >= 2;
                allInBtn.SetEnabled(canAllIn);
            }
        }
        
        private void UpdateShopChampions(List<ChampionData> champions)
        {
            for (int i = 0; i < shopSlots.Count; i++)
            {
                var slot = shopSlots[i];
                if (slot == null) continue;
                
                if (i < champions.Count)
                {
                    UpdateShopSlot(slot, champions[i], i);
                    slot.style.display = DisplayStyle.Flex;
                }
                else
                {
                    slot.style.display = DisplayStyle.None;
                }
            }
        }
        
        private void UpdateShopSlot(VisualElement slot, ChampionData champion, int index)
        {
            if (slot == null || champion == null) return;
            
            var shopChampion = slot.Q<VisualElement>("shop-champion");
            var portrait = slot.Q<VisualElement>("champion-portrait");
            var nameLabel = slot.Q<Label>("champion-name");
            var costLabel = slot.Q<Label>("champion-cost");
            
            // Update champion info
            if (nameLabel != null)
                nameLabel.text = champion.championName;
            
            if (costLabel != null)
                costLabel.text = $"{champion.cost}💰";
            
            // Update portrait
            if (portrait != null && champion.championIcon != null)
            {
                portrait.style.backgroundImage = new StyleBackground(champion.championIcon);
            }
            
            // Update tier styling
            if (shopChampion != null)
            {
                // Option 1: Store tier in userData
                shopChampion.userData = champion.tier;

                // Option 2: Add a class for styling (e.g., "tier-1", "tier-2", etc.)
                for (int t = 1; t <= 5; t++)
                    shopChampion.RemoveFromClassList($"tier-{t}");
                shopChampion.AddToClassList($"tier-{champion.tier}");
            }
            
            // Update affordability
            bool canAfford = player != null && player.gold >= champion.cost;
            slot.SetEnabled(canAfford);
            
            if (canAfford)
                slot.RemoveFromClassList("disabled");
            else
                slot.AddToClassList("disabled");
        }
        
        private int[] GetShopOdds(int level)
        {
            // TFT shop odds by level
            return level switch
            {
                1 => new int[] { 100, 0, 0, 0, 0 },
                2 => new int[] { 70, 30, 0, 0, 0 },
                3 => new int[] { 60, 35, 5, 0, 0 },
                4 => new int[] { 50, 35, 15, 0, 0 },
                5 => new int[] { 35, 40, 20, 5, 0 },
                6 => new int[] { 25, 40, 30, 5, 0 },
                7 => new int[] { 19, 30, 35, 15, 1 },
                8 => new int[] { 16, 20, 35, 25, 4 },
                9 => new int[] { 9, 15, 30, 30, 16 },
                _ => new int[] { 5, 10, 20, 35, 30 }
            };
        }
        
        #endregion
        
        #region Synergies Updates
        
        private void UpdateSynergies()
        {
            if (synergiesScroll == null) return;
            
            // Clear existing synergies
            synergiesScroll.Clear();
            
            // Add active synergies
            foreach (var synergy in activeSynergies)
            {
                var synergyItem = CreateSynergyItem(synergy);
                synergiesScroll.Add(synergyItem);
            }
        }
        
        private VisualElement CreateSynergyItem(SynergyData synergy)
        {
            var item = new VisualElement();
            item.AddToClassList("synergy-item");
            
            var icon = new VisualElement();
            icon.AddToClassList("synergy-icon");
            item.Add(icon);
            
            var name = new Label(synergy.synergyName);
            name.AddToClassList("synergy-name");
            item.Add(name);
            
            var count = new Label($"{synergy.currentCount}/{synergy.requiredCount}");
            count.AddToClassList("synergy-count");
            item.Add(count);
            
            var description = new Label(synergy.description);
            description.AddToClassList("synergy-description");
            item.Add(description);
            
            return item;
        }
        
        #endregion
        
        #region Bench Updates
        
        private void UpdateBench()
        {
            // Update bench slots with current champions
            // This would integrate with the Bench system
        }
        
        #endregion
        
        #region Enemy Preview
        
        private void UpdateEnemyPreview()
        {
            // Update enemy info and mini board
            // This would show next opponent's data
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSettingsClicked(ClickEvent evt)
        {
            if (settingsPanel != null)
            {
                settingsPanel.RemoveFromClassList("hidden");
            }
        }
        
        private void OnSurrenderClicked(ClickEvent evt)
        {
            // Implement surrender functionality
            Debug.Log("Surrender clicked!");
        }
        
        private void OnExitClicked(ClickEvent evt)
        {
            // Implement exit functionality
            Debug.Log("Exit clicked!");
        }
        
        private void OnBuyExpClicked(ClickEvent evt)
        {
            if (player != null && player.SpendGold(4))
            {
                player.AddExperience(4);
                UpdateExperience();
                Debug.Log("Bought 4 experience!");
            }
        }
        
        private void OnRefreshClicked(ClickEvent evt)
        {
            if (shopManager != null && player != null && !isShopLocked)
            {
                if (shopManager.RefreshShopWithCost(player))
                {
                    Debug.Log("Shop refreshed!");
                }
            }
        }
        
        private void OnLockClicked(ClickEvent evt)
        {
            isShopLocked = !isShopLocked;
            UpdateShopState();
            Debug.Log($"Shop {(isShopLocked ? "locked" : "unlocked")}");
        }
        
        private void OnAllInClicked(ClickEvent evt)
        {
            if (shopManager == null || player == null) return;
            
            int refreshCount = 0;
            while (player.gold >= 2 && refreshCount < 20)
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
            
            Debug.Log($"All-in: Refreshed {refreshCount} times!");
        }
        
        private void OnShopChampionClicked(int slotIndex)
        {
            if (slotIndex >= currentShopData.Count) return;
            
            var champion = currentShopData[slotIndex];
            if (shopManager != null && player != null)
            {
                if (shopManager.BuyChampion(champion, player))
                {
                    Debug.Log($"Đã mua {champion.championName}!");
                    
                    // Show purchase effect
                    if (enableAnimations)
                    {
                        ShowPurchaseEffect(slotIndex);
                    }
                }
                else
                {
                    Debug.Log("Không thể mua tướng!");
                }
            }
        }
        
        private void OnBenchSlotClicked(int slotIndex)
        {
            // Handle bench slot interactions
            Debug.Log($"Bench slot {slotIndex} clicked!");
        }
        
        private void OnCloseSettingsClicked(ClickEvent evt)
        {
            if (settingsPanel != null)
            {
                settingsPanel.AddToClassList("hidden");
            }
        }
        
        private void OnApplySettingsClicked(ClickEvent evt)
        {
            ApplySettings();
            if (settingsPanel != null)
            {
                settingsPanel.AddToClassList("hidden");
            }
        }
        
        private void OnShopRefreshed(List<ChampionData> newShopData)
        {
            currentShopData = newShopData;
            UpdateShopChampions(currentShopData);
        }
        
        #endregion
        
        #region Tooltips
        
        private void OnShopChampionHover(MouseEnterEvent evt)
        {
            if (!enableTooltips) return;
            
            var target = evt.target as VisualElement;
            var slotIndex = shopSlots.IndexOf(target);
            
            if (slotIndex >= 0 && slotIndex < currentShopData.Count)
            {
                var champion = currentShopData[slotIndex];
                if (tooltipCoroutine != null)
                    StopCoroutine(tooltipCoroutine);
                
                tooltipCoroutine = StartCoroutine(ShowChampionTooltipDelayed(champion, evt.mousePosition));
            }
        }
        
        private void OnShopChampionLeave(MouseLeaveEvent evt)
        {
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }
            
            HideTooltips();
        }
        
        private void OnBenchChampionHover(MouseEnterEvent evt)
        {
            // Similar to shop hover but for bench champions
        }
        
        private void OnBenchChampionLeave(MouseLeaveEvent evt)
        {
            HideTooltips();
        }
        
        private IEnumerator ShowChampionTooltipDelayed(ChampionData champion, Vector2 mousePos)
        {
            yield return new WaitForSeconds(tooltipDelay);
            ShowChampionTooltip(champion, mousePos);
        }
        
        private void ShowChampionTooltip(ChampionData champion, Vector2 position)
        {
            if (championTooltip == null) return;
            
            // Update tooltip content
            var tooltipName = championTooltip.Q<Label>("tooltip-name");
            var tooltipCost = championTooltip.Q<Label>("tooltip-cost");
            var tooltipHp = championTooltip.Q<Label>("tooltip-hp");
            var tooltipAd = championTooltip.Q<Label>("tooltip-ad");
            var abilityName = championTooltip.Q<Label>("ability-name");
            var abilityDescription = championTooltip.Q<Label>("ability-description");
            
            if (tooltipName != null)
                tooltipName.text = champion.championName;
            
            if (tooltipCost != null)
                tooltipCost.text = $"Giá: {champion.cost}💰";
            
            if (tooltipHp != null)
                tooltipHp.text = $"❤️ {champion.baseHealth}";
            
            if (tooltipAd != null)
                tooltipAd.text = $"⚔️ {champion.baseAttackDamage}";
            
            if (abilityName != null && champion.abilityName != null)
                abilityName.text = champion.abilityName;

            if (abilityDescription != null && champion.abilityDescription != null)
                abilityDescription.text = champion.abilityDescription;

            // Position tooltip
            championTooltip.style.left = position.x + 10;
            championTooltip.style.top = position.y - 10;
            championTooltip.RemoveFromClassList("hidden");
        }
        
        private void HideTooltips()
        {
            championTooltip?.AddToClassList("hidden");
            itemTooltip?.AddToClassList("hidden");
        }
        
        #endregion
        
        #region Visual Effects
        
        private void ShowPurchaseEffect(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < shopSlots.Count)
            {
                var slot = shopSlots[slotIndex];
                slot.AddToClassList("highlight");
                
                // Remove effect after 0.5 seconds
                slot.schedule.Execute(() => {
                    slot.RemoveFromClassList("highlight");
                }).ExecuteLater(500);
            }
        }
        
        #endregion
        
        #region Settings
        
        private void ApplySettings()
        {
            // Apply audio settings
            var musicSlider = root.Q<Slider>("music-volume");
            var sfxSlider = root.Q<Slider>("sfx-volume");
            
            if (musicSlider != null)
            {
                // Apply music volume
                Debug.Log($"Music volume set to: {musicSlider.value}");
            }
            
            if (sfxSlider != null)
            {
                // Apply SFX volume
                Debug.Log($"SFX volume set to: {sfxSlider.value}");
            }
            
            // Apply graphics settings
            var fullscreenToggle = root.Q<Toggle>("fullscreen-toggle");
            if (fullscreenToggle != null)
            {
                Screen.fullScreen = fullscreenToggle.value;
            }
            
            // Apply other settings...
            Debug.Log("Settings applied!");
        }
        
        #endregion
        
        #region Public Methods
        
        public void ShowPanel(string panelName)
        {
            var panel = root.Q<VisualElement>(panelName);
            panel?.RemoveFromClassList("hidden");
        }
        
        public void HidePanel(string panelName)
        {
            var panel = root.Q<VisualElement>(panelName);
            panel?.AddToClassList("hidden");
        }
        
        public void UpdateShopData(List<ChampionData> shopData)
        {
            currentShopData = shopData;
            UpdateShopChampions(shopData);
        }
        
        public void UpdateSynergyData(List<SynergyData> synergies)
        {
            activeSynergies = synergies;
            UpdateSynergies();
        }
        
        #endregion
        
        #region Auto-Find References
        
        private void AutoFindReferences()
        {
            if (player == null)
                player = FindFirstObjectByType<Player>();
            
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();
            
            if (shopManager == null)
                shopManager = FindFirstObjectByType<ShopManager>();
            
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();
        }
        
        private void OnValidate()
        {
            AutoFindReferences();
        }
        
        #endregion
    }
    
    // Helper class for synergy data
    [System.Serializable]
    public class SynergyData
    {
        public string synergyName;
        public int currentCount;
        public int requiredCount;
        public string description;
        public Sprite icon;
    }
}
