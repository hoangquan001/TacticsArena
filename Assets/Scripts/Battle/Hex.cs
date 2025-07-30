using UnityEngine;

namespace TacticsArena.Battle
{
    public class Hex : MonoBehaviour
    {
        [Header("Hex Info")]
        public int x;
        public int y;
        public Board board;
        
        [Header("State")]
        public Champion champion;
        public bool isOccupied = false;
        public bool isPlayerSide = true; // true cho player, false cho enemy
        
        [Header("Visual")]
        public Renderer hexRenderer;
        public Color defaultColor = Color.white;
        public Color highlightColor = Color.yellow;
        public Color occupiedColor = Color.red;
        
        private void Start()
        {
            if (hexRenderer == null)
                hexRenderer = GetComponent<Renderer>();
        }
        
        public void Initialize(int hexX, int hexY, Board parentBoard)
        {
            x = hexX;
            y = hexY;
            board = parentBoard;
            
            // Xác định side dựa trên vị trí
            isPlayerSide = y < parentBoard.height / 2;
            
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
                return true;
            }
            return false;
        }
        
        public void RemoveChampion()
        {
            champion = null;
            isOccupied = false;
            UpdateVisual();
        }
        
        public void Highlight(bool highlight)
        {
            if (hexRenderer != null)
            {
                if (highlight)
                    hexRenderer.material.color = highlightColor;
                else
                    UpdateVisual();
            }
        }
        
        private void UpdateVisual()
        {
            if (hexRenderer != null)
            {
                if (isOccupied)
                    hexRenderer.material.color = occupiedColor;
                else
                    hexRenderer.material.color = defaultColor;
            }
        }
        
        private void OnMouseEnter()
        {
            Highlight(true);
        }
        
        private void OnMouseExit()
        {
            Highlight(false);
        }
        
        private void OnMouseDown()
        {
            // Handle hex click - có thể dùng cho việc đặt champion
            Debug.Log($"Clicked hex ({x}, {y})");
            
            // Gửi event để UI xử lý
            HexClickEvent?.Invoke(this);
        }
        
        public static System.Action<Hex> HexClickEvent;
    }
}
