using UnityEngine;

namespace TacticsArena.Battle
{
    public class BenchSlot : MonoBehaviour
    {
        [Header("Slot Info")]
        public int slotIndex;
        public Bench bench;
        
        [Header("State")]
        public Champion champion;
        public bool isOccupied = false;
        
        [Header("Visual")]
        public Renderer slotRenderer;
        public Color defaultColor = Color.gray;
        public Color highlightColor = Color.yellow;
        public Color occupiedColor = Color.green;
        
        private bool isDragging = false;
        private Camera mainCamera;
        
        private void Start()
        {
            if (slotRenderer == null)
                slotRenderer = GetComponent<Renderer>();
                
            mainCamera = Camera.main;
            UpdateVisual();
        }
        
        public void Initialize(int index, Bench parentBench)
        {
            slotIndex = index;
            bench = parentBench;
            UpdateVisual();
        }
        
        public bool IsEmpty()
        {
            return champion == null && !isOccupied;
        }
        
        public bool PlaceChampion(Champion newChampion)
        {
            if (IsEmpty())
            {
                champion = newChampion;
                isOccupied = true;
                UpdateVisual();
                
                // Trigger event
                Bench.ChampionAddedToBench?.Invoke(newChampion);
                return true;
            }
            return false;
        }
        
        public void RemoveChampion()
        {
            if (champion != null)
            {
                Champion removedChampion = champion;
                champion = null;
                isOccupied = false;
                UpdateVisual();
                
                // Trigger event
                Bench.ChampionRemovedFromBench?.Invoke(removedChampion);
            }
        }
        
        public void Highlight(bool highlight)
        {
            if (slotRenderer != null)
            {
                if (highlight)
                    slotRenderer.material.color = highlightColor;
                else
                    UpdateVisual();
            }
        }
        
        private void UpdateVisual()
        {
            if (slotRenderer != null)
            {
                if (isOccupied)
                    slotRenderer.material.color = occupiedColor;
                else
                    slotRenderer.material.color = defaultColor;
            }
        }
        
        private void OnMouseEnter()
        {
            Highlight(true);
        }
        
        private void OnMouseExit()
        {
            if (!isDragging)
                Highlight(false);
        }
        
        private void OnMouseDown()
        {
            if (champion != null)
            {
                // Bắt đầu drag champion
                StartDragging();
            }
            else
            {
                // Click vào slot trống
                OnEmptySlotClicked();
            }
        }
        
        private void OnMouseDrag()
        {
            if (isDragging && champion != null)
            {
                // Di chuyển champion theo mouse
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = 10f; // Distance from camera
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
                champion.transform.position = worldPosition;
            }
        }
        
        private void OnMouseUp()
        {
            if (isDragging)
            {
                StopDragging();
            }
        }
        
        private void StartDragging()
        {
            if (champion == null) return;
            
            isDragging = true;
            Debug.Log($"Started dragging {champion.championData.championName} from bench slot {slotIndex}");
            
            // Trigger drag start event
            ChampionDragStarted?.Invoke(champion, this);
        }
        
        private void StopDragging()
        {
            if (!isDragging || champion == null) return;
            
            isDragging = false;
            
            // Raycast để tìm drop target
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            bool droppedSuccessfully = false;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Check if dropped on hex (board)
                Hex targetHex = hit.collider.GetComponent<Hex>();
                if (targetHex != null && targetHex.IsEmpty())
                {
                    // Move to board
                    if (MoveChampionToBoard(champion, targetHex))
                    {
                        droppedSuccessfully = true;
                    }
                }
                else
                {
                    // Check if dropped on another bench slot
                    BenchSlot targetSlot = hit.collider.GetComponent<BenchSlot>();
                    if (targetSlot != null && targetSlot != this)
                    {
                        // Swap or move to another bench slot
                        if (SwapWithSlot(targetSlot))
                        {
                            droppedSuccessfully = true;
                        }
                    }
                }
            }
            
            if (!droppedSuccessfully)
            {
                // Return to original position
                champion.transform.position = transform.position + Vector3.up * 0.5f;
            }
            
            Highlight(false);
            
            // Trigger drag end event
            ChampionDragEnded?.Invoke(champion, this, droppedSuccessfully);
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
