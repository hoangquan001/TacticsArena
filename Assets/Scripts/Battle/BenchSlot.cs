using UnityEngine;

namespace TacticsArena.Battle
{
    public class BenchSlot : Cell
    {
        [Header("Slot Info")]
        public Bench bench;
        
        [Header("Visual")]
        
        private bool isDragging = false;
        
        protected override void Start()
        {
            base.Start();
            UpdateVisual();
        }
        
        public void Initialize(int index, Bench parentBench)
        {
            x = index;
            bench = parentBench;
            UpdateVisual();
        }
        
        public new void Highlight(bool highlight)
        {
            // Override highlight để có thể customize cho bench slot
            base.Highlight(highlight);
        }
        
        private bool MoveChampionToBoard(Champion championToMove, BoardSlot targetSlot)
        {
            Board board = targetSlot.board;
            if (board == null) return false;
            
            // Place champion on board
            if (board.PlaceChampion(championToMove, targetSlot.x, targetSlot.y))
            {
                // Remove from bench
                RemoveChampion();
                Debug.Log($"Moved {championToMove.data.championName} from bench to board at ({targetSlot.x}, {targetSlot.y})");
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
            bench.SwapChampions(x, targetIndex);
            Debug.Log($"Swapped champions between bench slots {x} and {targetIndex}");
            return true;
        }
        
        private void OnEmptySlotClicked()
        {
            Debug.Log($"Clicked empty bench slot {x}");
            // Could be used for placing selected champion from shop
        }
        
        // Events
        public static System.Action<Champion, BenchSlot> ChampionDragStarted;
        public static System.Action<Champion, BenchSlot, bool> ChampionDragEnded;
    }
}
