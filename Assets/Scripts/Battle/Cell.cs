using UnityEngine;

namespace TacticsArena.Battle
{
    public class Cell : MonoBehaviour
    {
        [Header("Hex Info")]
        public int x;
        public int y;
        // public Board board;

        [Header("State")]
        public Champion champion;
        public bool isOccupied = false;
        public bool isPlayerSide = true; // true cho player, false cho enemy

        [Header("Visual")]
        public Renderer _renderer;
        public Color defaultColor = Color.white;
        public Color highlightColor = Color.yellow;
        public Color occupiedColor = Color.red;
        protected virtual void Start()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
        }



        public bool IsEmpty()
        {
            return champion == null && !isOccupied;
        }

        public bool PlaceChampion(Champion newChampion)

        {
            var oldChampion = champion; // Lưu champion cũ
            if (newChampion == null)
            {
                champion = null;
                isOccupied = false;
                UpdateVisual();
                return true; // Không có champion nào để đặt
            }
            if (this.champion == newChampion)
            {
                isOccupied = true;
                this.champion.transform.position = transform.position; // Đặt lại vị trí nếu champion đã ở đây
                UpdateVisual();

                return true;
            }
            if (IsEmpty())
            {
                var oldCell = newChampion.CurCell; // Cập nhật ô cũ của champion
                oldCell?.RemoveChampion(); // Xóa champion khỏi ô cũ nếu có
                champion = newChampion;
                isOccupied = true;
                newChampion.CurCell = this; // Cập nhật ô hiện tại của champion
                UpdateVisual();
                return true;
            }

            this.champion.CurCell = newChampion.CurCell; // Cập nhật ô cũ của champion
            newChampion.CurCell.PlaceChampion(oldChampion); // Đặt champion cũ vào ô mới
            isOccupied = true;
            newChampion.CurCell.UpdateVisual(); // Cập nhật ô cũ của champion
            champion = newChampion;
            newChampion.CurCell = this; // Cập nhật ô hiện tại của champion
            UpdateVisual();
            return true;

        }

        public void RemoveChampion()
        {
            champion = null;
            isOccupied = false;
            UpdateVisual();
        }

        public void Highlight(bool highlight)
        {
            if (_renderer != null)
            {
                if (highlight)
                    _renderer.material.color = highlightColor;
                else
                    UpdateVisual();
            }
        }

        protected void UpdateVisual()
        {
            if (_renderer != null)
            {
                if (isOccupied)
                    _renderer.material.color = occupiedColor;
                else
                    _renderer.material.color = defaultColor;
            }
        }

        // public static System.Action<Cell> OnClickEvent;
    }
}