using UnityEngine;

namespace TacticsArena.Battle
{
    public class BenchSlot : Hex
    {
        [Header("Slot Info")]
        public int slotIndex;
        public Bench bench;
        
        [Header("Visual")]
        
        private bool isDragging = false;
        private Camera mainCamera;
        
        protected override void Start()
        {
            base.Start();
            mainCamera = Camera.main;
            UpdateVisual();
        }
        
        public void Initialize(int index, Bench parentBench)
        {
            slotIndex = index;
            bench = parentBench;
            UpdateVisual();
        }
        
        private bool MoveChampionToBoard(Champion championToMove, Hex targetHex)
        {
            Board board = targetHex.board;
            if (board == null) return false;
            
            // Place champion on board
            if (board.PlaceChampion(championToMove, targetHex.x, targetHex.y))
            {
                // Remove from bench
                RemoveChampion();
                Debug.Log($"Moved {championToMove.championData.championName} from bench to board at ({targetHex.x}, {targetHex.y})");
                return true;
            }
            
            return false;
        }
        
        private bool SwapWithSlot(BenchSlot targetSlot)
        {
            if (targetSlot.bench != bench) return false;
            
            int targetIndex = bench.GetSlotIndex(targetSlot);
            if (targetIndex == -1) return false;
            
            // Swap champions
            bench.SwapChampions(slotIndex, targetIndex);
            Debug.Log($"Swapped champions between bench slots {slotIndex} and {targetIndex}");
            return true;
        }
        
        private void OnEmptySlotClicked()
        {
            Debug.Log($"Clicked empty bench slot {slotIndex}");
            // Could be used for placing selected champion from shop
        }
        
        // Events
        public static System.Action<Champion, BenchSlot> ChampionDragStarted;
        public static System.Action<Champion, BenchSlot, bool> ChampionDragEnded;
    }
}
