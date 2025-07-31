using UnityEngine;
using TacticsArena.Core;

namespace TacticsArena.Battle
{
    public class PlayerAreaManager : MonoBehaviour
    {
        [Header("Player Areas")]
        public PlayerArea playerArea;
        public PlayerArea enemyArea;
        
        [Header("Settings")]
        public string playerName = "Player";
        public string enemyName = "Enemy";
        
        private void Start()
        {
            InitializePlayerAreas();
        }
        
        private void InitializePlayerAreas()
        {
            // Initialize player area
            if (playerArea != null)
            {
                playerArea.Initialize(playerName, true, 0); // Team 0 = Player
                playerArea.EnableMovement(true); // Enable movement for player
                Debug.Log($"Initialized {playerName} area with team ID 0");
            }
            
            // Initialize enemy area
            if (enemyArea != null)
            {
                enemyArea.Initialize(enemyName, false, 1); // Team 1 = Enemy
                enemyArea.EnableMovement(false); // Disable movement for enemy
                Debug.Log($"Initialized {enemyName} area with team ID 1");
            }
            
            // Setup battle manager với player areas
            var battleManager = FindAnyObjectByType<BattleManager>();
            if (battleManager != null)
            {
                battleManager.SetPlayerAreas(playerArea, enemyArea);
                Debug.Log("Connected PlayerAreas to BattleManager");
            }
            
            // Subscribe to game state changes
            GameManager.OnStateChanged += OnGameStateChanged;
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Preparation:
                    // Enable movement during preparation
                    if (playerArea != null) playerArea.EnableMovement(true);
                    break;
                    
                case GameState.Battle:
                    // Disable movement during battle
                    if (playerArea != null) playerArea.EnableMovement(false);
                    break;
                    
                case GameState.PostBattle:
                    // Disable movement after battle
                    if (playerArea != null) playerArea.EnableMovement(false);
                    break;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameManager.OnStateChanged -= OnGameStateChanged;
        }
        
        public PlayerArea GetPlayerArea()
        {
            return playerArea;
        }
        
        public PlayerArea GetEnemyArea()
        {
            return enemyArea;
        }
        
        public PlayerArea GetPlayerAreaByTeam(int teamId)
        {
            if (teamId == 0) return playerArea;
            if (teamId == 1) return enemyArea;
            return null;
        }
        
        public bool IsPlayerTeam(int teamId)
        {
            return teamId == 0;
        }
        
        // Utility method để spawn champion cho player
        public bool SpawnChampionForPlayer(GameObject championPrefab, bool onBoard = false)
        {
            if (championPrefab == null || playerArea == null) return false;
            
            GameObject championObject = Instantiate(championPrefab);
            Champion champion = championObject.GetComponent<Champion>();
            
            if (champion == null)
            {
                Destroy(championObject);
                return false;
            }
            
            if (onBoard)
            {
                return playerArea.TryPlaceChampionOnBoard(champion);
            }
            else
            {
                return playerArea.TryPlaceChampionOnBench(champion);
            }
        }
        
        // Utility method để spawn champion cho enemy (AI)
        public bool SpawnChampionForEnemy(GameObject championPrefab, bool onBoard = false)
        {
            if (championPrefab == null || enemyArea == null) return false;
            
            GameObject championObject = Instantiate(championPrefab);
            Champion champion = championObject.GetComponent<Champion>();
            
            if (champion == null)
            {
                Destroy(championObject);
                return false;
            }
            
            if (onBoard)
            {
                return enemyArea.TryPlaceChampionOnBoard(champion);
            }
            else
            {
                return enemyArea.TryPlaceChampionOnBench(champion);
            }
        }
        
        // Debug methods
        [ContextMenu("Debug Player Area")]
        public void DebugPlayerArea()
        {
            if (playerArea != null)
                playerArea.DebugInfo();
        }
        
        [ContextMenu("Debug Enemy Area")]
        public void DebugEnemyArea()
        {
            if (enemyArea != null)
                enemyArea.DebugInfo();
        }
    }
}
