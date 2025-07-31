using TacticsArena.Core;
using UnityEngine;

namespace TacticsArena.Battle
{
    public class ChampionMovementInput : MonoBehaviour
    {
        [Header("Settings")]
        public KeyCode cancelKey = KeyCode.Escape;
        public KeyCode rotateKey = KeyCode.R;
        
        private PlayerArea currentPlayerArea;
        private bool isEnabled = true;
        
        public void SetPlayerArea(PlayerArea area)
        {
            currentPlayerArea = area;
        }
        
        public void EnableInput(bool enable)
        {
            isEnabled = enable;
        }
        
        private void Update()
        {
            if (!isEnabled || currentPlayerArea == null) return;
            
            HandleKeyboardInput();
        }
        
        private void HandleKeyboardInput()
        {
            // Cancel current drag operation
            if (Input.GetKeyDown(cancelKey))
            {
                CancelCurrentOperation();
            }
            
            // Rotate selected champion (if implemented)
            if (Input.GetKeyDown(rotateKey))
            {
                RotateSelectedChampion();
            }
            
            // Number keys for quick bench selection
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    SelectBenchSlot(i - 1);
                }
            }
        }
        
        private void CancelCurrentOperation()
        {
            // Reset any dragging state
            Debug.Log("Champion movement cancelled");
            // Implementation would depend on current drag state
        }
        
        private void RotateSelectedChampion()
        {
            // Rotate champion (if needed for game mechanics)
            Debug.Log("Champion rotation requested");
        }
        
        private void SelectBenchSlot(int slotIndex)
        {
            if (currentPlayerArea == null || currentPlayerArea.bench == null) return;
            
            var slot = currentPlayerArea.bench.GetSlot(slotIndex);
            if (slot != null && slot.champion != null)
            {
                Debug.Log($"Selected champion in bench slot {slotIndex}: {slot.champion.data?.championName}");
                // Could highlight the champion or show info
                HighlightChampion(slot.champion);
            }
        }
        
        private void HighlightChampion(Champion champion)
        {
            if (champion == null) return;
            
            // Add visual highlight to champion
            var renderer = champion.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Temporary highlight - in real game you'd use proper highlighting system
                renderer.material.color = Color.yellow;
                
                // Reset after 1 second
                Invoke(nameof(ResetHighlight), 1f);
            }
        }
        
        private void ResetHighlight()
        {
            // Reset all champion highlights
            var champions = currentPlayerArea?.GetAllChampions();
            if (champions != null)
            {
                foreach (var champion in champions)
                {
                    var renderer = champion.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.white;
                    }
                }
            }
        }
        
        // Called when game state changes
        public void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Preparation:
                    EnableInput(true);
                    break;
                case GameState.Battle:
                    EnableInput(false); // No movement during battle
                    break;
                case GameState.PostBattle:
                    EnableInput(false);
                    break;
                default:
                    EnableInput(true);
                    break;
            }
        }
    }
}
