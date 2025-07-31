using UnityEngine;
using TacticsArena.Core;
using TacticsArena.Battle;
using TacticsArena.Shop;
using TacticsArena.Synergies;
using TacticsArena.UI;

namespace TacticsArena
{
    /// <summary>
    /// Main controller để setup và quản lý toàn bộ game
    /// </summary>
    public class MainGameController : MonoBehaviour
    {
        [Header("Game Systems")]
        public GameManager gameManager;
        public TurnManager turnManager;
        public BattleManager battleManager;
        public ShopManager shopManager;
        public SynergyManager synergyManager;
        // public BenchManager benchManager;
        
        
        [Header("Players")]
        public Player mainPlayer;
        public Player[] allPlayers;
        
        [Header("UI")]
        public GameUI gameUI;

        private void Awake()
        {
            SetupGameSystems();
        }
        
        private void Start()
        {
            InitializeGame();
        }
        [ContextMenu("Setup Game Systems")]
        private void SetupGameSystems()
        {
            // Tạo GameManager nếu chưa có
            if (gameManager == null)
            {
                GameObject gmObject = new GameObject("GameManager");
                gameManager = gmObject.AddComponent<GameManager>();
            }
            
            // Tạo các system khác nếu cần
            if (turnManager == null)
            {
                GameObject tmObject = new GameObject("TurnManager");
                turnManager = tmObject.AddComponent<TurnManager>();
            }
            
            if (battleManager == null)
            {
                GameObject bmObject = new GameObject("BattleManager");
                battleManager = bmObject.AddComponent<BattleManager>();
            }
            
            if (shopManager == null)
            {
                GameObject smObject = new GameObject("ShopManager");
                shopManager = smObject.AddComponent<ShopManager>();
            }
            
            if (synergyManager == null)
            {
                GameObject synObject = new GameObject("SynergyManager");
                synergyManager = synObject.AddComponent<SynergyManager>();
            }
            
            // Setup bench manager will be done separately
            
            // Link references
            if (gameManager != null)
            {
                gameManager.turnManager = turnManager;
                gameManager.shopManager = shopManager;
                gameManager.battleManager = battleManager;
            }
            
           
        }
        
        private void InitializeGame()
        {
            // Setup boards
            SetupBoards();
            
            // Setup player
            
            // Setup UI
            SetupUI();
            
            Debug.Log("TacticsArena game initialized!");
        }
        
        private void SetupBoards()
        {

            
        }
        

    
        
        private void SetupUI()
        {
            if (gameUI != null && mainPlayer != null)
            {
                gameUI.SetPlayer(mainPlayer);
            }
        }
        
        [ContextMenu("Start Battle Test")]
        public void StartBattleTest()
        {
            if (battleManager != null)
            {
                battleManager.StartBattle();
            }
        }
        
        [ContextMenu("Refresh Shop Test")]
        public void RefreshShopTest()
        {
            if (shopManager != null)
            {
                shopManager.RefreshShop();
            }
        }
    }
}
