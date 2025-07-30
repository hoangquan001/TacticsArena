using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TacticsArena.Core;

namespace TacticsArena.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Battle Settings")]
        public float battleDuration = 60f;
        
        [Header("Player Areas")]
        public PlayerArea playerArea;
        public PlayerArea enemyArea;
        
        private List<Champion> playerChampions = new List<Champion>();
        private List<Champion> enemyChampions = new List<Champion>();
        private bool battleInProgress = false;
        
        public void StartBattle()
        {
            if (battleInProgress) return;
            
            // Validate player areas
            if (playerArea == null || enemyArea == null)
            {
                Debug.LogError("Player areas not assigned!");
                return;
            }
            
            battleInProgress = true;
            
            // Lấy champions từ board của cả hai player areas (chỉ những champion trên board mới tham gia battle)
            playerChampions = playerArea.GetBoardChampions();
            enemyChampions = enemyArea.GetBoardChampions();
            
            Debug.Log($"Battle started! {playerArea.playerName}: {playerChampions.Count} vs {enemyArea.playerName}: {enemyChampions.Count}");
            
            // Bắt đầu battle cho tất cả champions trên board
            playerArea.StartBattle();
            enemyArea.StartBattle();
            
            // Bắt đầu battle timer
            StartCoroutine(BattleTimer());
        }
        
        private IEnumerator BattleTimer()
        {
            float timeRemaining = battleDuration;
            
            while (timeRemaining > 0 && !IsBattleFinished())
            {
                timeRemaining -= Time.deltaTime;
                yield return null;
            }
            
            EndBattle();
        }
        
        public bool IsBattleFinished()
        {
            // Kiểm tra xem có team nào còn champion sống trên board không
            int alivePlayerChampions = playerArea != null ? playerArea.GetAliveBoardChampionsCount() : 0;
            int aliveEnemyChampions = enemyArea != null ? enemyArea.GetAliveBoardChampionsCount() : 0;
            
            return alivePlayerChampions == 0 || aliveEnemyChampions == 0;
        }
        
        public void EndBattle()
        {
            if (!battleInProgress) return;
            
            battleInProgress = false;
            
            // End battle for both player areas
            if (playerArea != null) playerArea.EndBattle();
            if (enemyArea != null) enemyArea.EndBattle();
            
            BattleResult result = CalculateBattleResult();
            
            Debug.Log($"Battle ended! Winner: {result.winner}");
            Debug.Log($"Damage dealt: {result.damageDealt}");
            
            // Thông báo kết quả battle cho GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(Core.GameState.PostBattle);
            }
        }
        
        private BattleResult CalculateBattleResult()
        {
            BattleResult result = new BattleResult();
            
            int alivePlayerChampions = playerArea != null ? playerArea.GetAliveBoardChampionsCount() : 0;
            int aliveEnemyChampions = enemyArea != null ? enemyArea.GetAliveBoardChampionsCount() : 0;
            
            // Store alive champions count
            result.playerChampionsAlive = alivePlayerChampions;
            result.enemyChampionsAlive = aliveEnemyChampions;
            
            if (alivePlayerChampions > aliveEnemyChampions)
            {
                result.winner = BattleWinner.Player;
                result.winnerName = playerArea != null ? playerArea.playerName : "Player";
            }
            else if (aliveEnemyChampions > alivePlayerChampions)
            {
                result.winner = BattleWinner.Enemy;
                result.winnerName = enemyArea != null ? enemyArea.playerName : "Enemy";
            }
            else
            {
                result.winner = BattleWinner.Draw;
                result.winnerName = "Draw";
            }
            
            // Tính damage (dựa trên level champions còn sống và stage)
            result.damageDealt = CalculateDamage(result.winner == BattleWinner.Enemy ? aliveEnemyChampions : 0);
            
            return result;
        }
        
        private int CalculateDamage(int aliveChampions)
        {
            if (aliveChampions == 0) return 0;
            
            int baseDamage = 2; // Base damage
            int stageDamage = GameManager.Instance != null ? GameManager.Instance.currentRound : 1; // Damage tăng theo round
            int championDamage = aliveChampions; // Damage từ số champion còn sống
            
            return baseDamage + stageDamage + championDamage;
        }
        
        // Utility methods for external access
        public PlayerArea GetPlayerArea()
        {
            return playerArea;
        }
        
        public PlayerArea GetEnemyArea()
        {
            return enemyArea;
        }
        
        public void SetPlayerAreas(PlayerArea player, PlayerArea enemy)
        {
            playerArea = player;
            enemyArea = enemy;
            
            // Initialize player areas if not already done
            if (playerArea != null && !playerArea.isLocalPlayer)
            {
                playerArea.Initialize("Player", true);
            }
            
            if (enemyArea != null && enemyArea.isLocalPlayer)
            {
                enemyArea.Initialize("Enemy", false);
            }
        }
    }
    
    [System.Serializable]
    public class BattleResult
    {
        public BattleWinner winner;
        public string winnerName;
        public int damageDealt;
        public int playerChampionsAlive;
        public int enemyChampionsAlive;
    }
    
    public enum BattleWinner
    {
        Player,
        Enemy,
        Draw
    }
}
