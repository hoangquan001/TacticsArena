using UnityEngine;
using System.Collections.Generic;
using System;
using TacticsArena.Shop;
using TacticsArena.Battle;

namespace TacticsArena.Core
{
    public enum GameState
    {
        Preparation,
        Battle,
        PostBattle,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public int maxPlayers = 8;
        public float preparationTime = 30f;
        
        [Header("Current Game State")]
        public GameState currentState = GameState.Preparation;
        public int currentRound = 1;
        
        [Header("References")]
        public TurnManager turnManager;
        public ShopManager shopManager;
        public BattleManager battleManager;
        
        // Events
        public static event Action<GameState> OnStateChanged;
        
        private List<Player> players = new List<Player>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            // Khởi tạo game
            ChangeState(GameState.Preparation);
        }
        
        public void ChangeState(GameState newState)
        {
            currentState = newState;
            
            // Trigger event
            OnStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.Preparation:
                    StartPreparationPhase();
                    break;
                case GameState.Battle:
                    StartBattlePhase();
                    break;
                case GameState.PostBattle:
                    StartPostBattlePhase();
                    break;
                case GameState.GameOver:
                    EndGame();
                    break;
            }
        }
        
        private void StartPreparationPhase()
        {
            Debug.Log($"Round {currentRound} - Preparation Phase Started");
            // Mở shop, cho phép mua champion, sắp xếp đội hình
        }
        
        private void StartBattlePhase()
        {
            Debug.Log($"Round {currentRound} - Battle Phase Started");
            // Bắt đầu chiến đấu tự động
        }
        
        private void StartPostBattlePhase()
        {
            Debug.Log($"Round {currentRound} - Post Battle Phase");
            // Tính damage, cập nhật health, kinh nghiệm
        }
        
        private void EndGame()
        {
            Debug.Log("Game Over!");
        }
    }
}
