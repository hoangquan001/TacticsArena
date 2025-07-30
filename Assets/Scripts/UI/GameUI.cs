using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TacticsArena.Core;
using TacticsArena.Battle;
using TacticsArena.Shop;
using TacticsArena.Synergies;

namespace TacticsArena.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("Player Info UI")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI experienceText;
        
        [Header("Game State UI")]
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI gameStateText;
        
        [Header("Shop UI")]
        public Transform shopContainer;
        public GameObject championShopItemPrefab;
        public Button refreshShopButton;
        
        [Header("Synergy UI")]
        public Transform synergyContainer;
        public GameObject synergyItemPrefab;
        
        [Header("Buttons")]
        public Button buyExperienceButton;
        
        private Player currentPlayer;
        
        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
        }
        
        private void SetupUI()
        {
            if (refreshShopButton != null)
                refreshShopButton.onClick.AddListener(OnRefreshShopClicked);
                
            if (buyExperienceButton != null)
                buyExperienceButton.onClick.AddListener(OnBuyExperienceClicked);
        }
        
        private void SubscribeToEvents()
        {
            ShopManager.ShopRefreshedEvent += UpdateShopUI;
            SynergyManager.SynergiesUpdatedEvent += UpdateSynergyUI;
        }
        
        private void OnDestroy()
        {
            ShopManager.ShopRefreshedEvent -= UpdateShopUI;
            SynergyManager.SynergiesUpdatedEvent -= UpdateSynergyUI;
        }
        
        private void Update()
        {
            UpdatePlayerInfo();
            UpdateGameState();
        }
        
        public void SetPlayer(Player player)
        {
            currentPlayer = player;
        }
        
        private void UpdatePlayerInfo()
        {
            if (currentPlayer == null) return;
            
            if (playerNameText != null)
                playerNameText.text = currentPlayer.playerName;
                
            if (healthText != null)
                healthText.text = $"Health: {currentPlayer.health}";
                
            if (goldText != null)
                goldText.text = $"Gold: {currentPlayer.gold}";
                
            if (levelText != null)
                levelText.text = $"Level: {currentPlayer.level}";
                
            if (experienceText != null)
                experienceText.text = $"Exp: {currentPlayer.experience}";
        }
        
        private void UpdateGameState()
        {
            if (GameManager.Instance == null) return;
            
            if (roundText != null)
                roundText.text = $"Round: {GameManager.Instance.currentRound}";
                
            if (gameStateText != null)
                gameStateText.text = $"State: {GameManager.Instance.currentState}";
        }
        
        private void UpdateShopUI(System.Collections.Generic.List<Champions.ChampionData> shopChampions)
        {
            if (shopContainer == null) return;
            
            // Clear existing items
            foreach (Transform child in shopContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create new items
            foreach (var champion in shopChampions)
            {
                GameObject item = Instantiate(championShopItemPrefab, shopContainer);
                ChampionShopItem shopItem = item.GetComponent<ChampionShopItem>();
                if (shopItem != null)
                {
                    shopItem.SetChampion(champion);
                }
            }
        }
        
        private void UpdateSynergyUI(System.Collections.Generic.List<string> synergyInfo)
        {
            if (synergyContainer == null) return;
            
            // Clear existing items
            foreach (Transform child in synergyContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create new items
            foreach (string info in synergyInfo)
            {
                GameObject item = Instantiate(synergyItemPrefab, synergyContainer);
                TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = info;
                }
            }
        }
        
        private void OnRefreshShopClicked()
        {
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null && currentPlayer != null)
            {
                shopManager.RefreshShopWithCost(currentPlayer);
            }
        }
        
        private void OnBuyExperienceClicked()
        {
            if (currentPlayer != null && currentPlayer.SpendGold(4))
            {
                currentPlayer.AddExperience(4);
            }
        }
    }
}
