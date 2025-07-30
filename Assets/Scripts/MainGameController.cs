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
        
        [Header("Boards")]
        public Board playerBoard;
        public Board enemyBoard;
        
        [Header("Players")]
        public Player mainPlayer;
        public Player[] allPlayers;
        
        [Header("UI")]
        public GameUI gameUI;
        
        [Header("Prefabs")]
        public GameObject hexPrefab;
        public GameObject championPrefab;
        
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
            SetupPlayer();
            
            // Setup UI
            SetupUI();
            
            // Create sample champions for testing
            CreateSampleChampions();
            
            Debug.Log("TacticsArena game initialized!");
        }
        
        private void SetupBoards()
        {
            if (playerBoard == null)
            {
                GameObject playerBoardObject = new GameObject("PlayerBoard");
                playerBoard = playerBoardObject.AddComponent<Board>();
                playerBoard.hexPrefab = hexPrefab;
            }
            
            if (enemyBoard == null)
            {
                GameObject enemyBoardObject = new GameObject("EnemyBoard");
                enemyBoard = enemyBoardObject.AddComponent<Board>();
                enemyBoard.hexPrefab = hexPrefab;
                
                // Position enemy board
                enemyBoardObject.transform.position = new Vector3(0, 0, 10);
            }
        }
        
        private void SetupPlayer()
        {
            if (mainPlayer == null)
            {
                GameObject playerObject = new GameObject("MainPlayer");
                mainPlayer = playerObject.AddComponent<Player>();
                mainPlayer.playerName = "Player 1";
                mainPlayer.playerBoard = playerBoard;
                mainPlayer.gold = 10; // Starting gold
                
                // Create bench for player
                SetupPlayerBench(playerObject);
            }
        }
        
        private void SetupPlayerBench(GameObject playerObject)
        {
            // Create bench GameObject
            GameObject benchObject = new GameObject("PlayerBench");
            benchObject.transform.SetParent(playerObject.transform);
            benchObject.transform.localPosition = new Vector3(0, 0, -5); // Position bench in front of player
            
            // Add a placeholder component for now
            MonoBehaviour benchComponent = benchObject.AddComponent<MonoBehaviour>();
            mainPlayer.playerBench = benchComponent; // Store reference as MonoBehaviour
            
            Debug.Log("Player bench setup completed (will be activated when Bench script compiles)");
        }
        
        private void SetupUI()
        {
            if (gameUI != null && mainPlayer != null)
            {
                gameUI.SetPlayer(mainPlayer);
            }
        }
        
        private void CreateSampleChampions()
        {
            // Tạo một số sample champion data cho testing
            // Trong thực tế, các data này sẽ được load từ ScriptableObjects
            
            Debug.Log("Sample champions would be created here");
            // TODO: Create actual champion data assets
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
