using UnityEngine;
using System.Collections.Generic;

namespace TacticsArena.Battle
{
    public class PlayerArea : MonoBehaviour
    {
        [Header("Player Area Components")]
        public Board board;
        public Bench bench;

        [Header("Player Info")]
        public string playerName = "Player";
        public bool isLocalPlayer = true;
        public int teamId = 0; // 0 = player, 1 = enemy

        private List<Champion> allChampions = new List<Champion>();

        [Header("Champion Movement")]
        public LayerMask championLayer;
        public LayerMask boardLayer;
        public LayerMask benchLayer;

        private Vector3 offset = new Vector3(0, 0.1f, 0);
        private Champion selectedChampion;
        private Vector3 originalPosition;
        private bool isDragging = false;
        private bool movementEnabled = true;
        private Camera playerCamera;

        public void Initialize(string name, bool isLocal = true, int team = 0)
        {
            playerName = name;
            isLocalPlayer = isLocal;
            teamId = team;

            // Initialize board and bench
            if (board != null)
            {
                // Board không có Initialize method, nó tự init trong Start()
                // board.Initialize();
            }

            if (bench != null)
            {
                // Bench không có Initialize method, nó tự init trong Start()
                // bench.Initialize();
            }

            RefreshChampionsList();
        }

        public void RefreshChampionsList()
        {
            allChampions.Clear();

            // Add champions from board
            if (board != null)
            {
                allChampions.AddRange(board.GetAllChampions());
            }

            // Add champions from bench
            if (bench != null)
            {
                allChampions.AddRange(bench.GetAllChampions());
            }
        }

        public List<Champion> GetBoardChampions()
        {
            if (board != null)
                return board.GetAllChampions();
            return new List<Champion>();
        }

        public List<Champion> GetBenchChampions()
        {
            if (bench != null)
                return bench.GetAllChampions();
            return new List<Champion>();
        }

        public List<Champion> GetAllChampions()
        {
            RefreshChampionsList();
            return new List<Champion>(allChampions);
        }

        public int GetChampionCount()
        {
            return GetAllChampions().Count;
        }

        public int GetBoardChampionCount()
        {
            return GetBoardChampions().Count;
        }

        public int GetBenchChampionCount()
        {
            return GetBenchChampions().Count;
        }

        public bool CanPlaceChampionOnBoard()
        {
            if (board == null) return false;

            // Board có thể đặt champion bất cứ đâu miễn là có hex trống
            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.height; y++)
                {
                    var cell = board.GetCell(x, y);
                    if (cell != null && cell.IsEmpty())
                        return true;
                }
            }
            return false;
        }

        public bool CanPlaceChampionOnBench()
        {
            if (bench == null) return false;
            return bench.GetEmptySlotCount() > 0;
        }

        public bool TryPlaceChampionOnBoard(Champion champion)
        {
            if (board == null || champion == null) return false;

            // Assign team and owner
            AssignChampionToTeam(champion);

            // Tìm vị trí trống đầu tiên trên board
            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.height; y++)
                {
                    var slot = board.GetCell(x, y);
                    if (slot != null && slot.IsEmpty())
                    {
                        bool success = board.PlaceChampion(champion, x, y);
                        if (success)
                        {
                            RefreshChampionsList();
                        }
                        return success;
                    }
                }
            }
            return false;
        }

        public bool TryPlaceChampionOnBench(Champion champion)
        {
            if (bench == null || champion == null) return false;

            // Assign team and owner
            AssignChampionToTeam(champion);

            bool success = bench.AddChampion(champion);
            if (success)
            {
                RefreshChampionsList();
            }
            return success;
        }

        private void AssignChampionToTeam(Champion champion)
        {
            if (champion != null)
            {
                champion.teamId = this.teamId;
                champion.ownerArea = this;
            }
        }

        public void StartBattle()
        {
            // Chỉ những champion trên board mới tham gia battle
            List<Champion> boardChampions = GetBoardChampions();

            foreach (Champion champion in boardChampions)
            {
                if (champion != null && champion.isAlive)
                {
                    champion.StartBattle();
                }
            }
        }
       

        private void Start()
        {
            // Tìm camera chính
            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = FindAnyObjectByType<Camera>();
        }

        private void Update()
        {
            // Chỉ local player mới có thể move champion
            if (!isLocalPlayer) return;

            HandleChampionMovement();
        }

        private void HandleChampionMovement()
        {
            // Chỉ cho phép move trong preparation phase
            if (!movementEnabled) return;

            if (Input.GetMouseButtonDown(0))
            {
                StartDrag();
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                DragChampion();
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                EndDrag();
            }
        }

        private void StartDrag()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Kiểm tra có click vào champion không
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, championLayer))
            {
                Champion champion = hit.collider.GetComponent<Champion>();
                if (champion != null && IsMyChampion(champion))
                {
                    selectedChampion = champion;
                    originalPosition = champion.transform.position;
                    isDragging = true;

                    // Highlight possible positions
                    HighlightValidPositions(true);

                    Debug.Log($"Started dragging {champion.data?.championName}");
                }
            }
        }

        private void DragChampion()
        {
            if (selectedChampion == null) return;

            // Move champion theo mouse position
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, originalPosition.y);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                selectedChampion.transform.position = worldPosition + offset;
            }
        }

        private void EndDrag()
        {
            if (selectedChampion == null) return;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool placed = false;

            // Kiểm tra có drop vào cell không
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, boardLayer | benchLayer))
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && IsValidDropPosition(cell))
                {
                    placed = MoveChampionToCell(selectedChampion, cell);
                }
            }

            // Nếu không place được, return về vị trí cũ
            if (!placed)
            {
                selectedChampion.transform.position = originalPosition;
                Debug.Log("Invalid drop position, returning champion to original position");
            }

            // Clean up
            HighlightValidPositions(false);
            selectedChampion = null;
            isDragging = false;
        }

        private bool IsMyChampion(Champion champion)
        {
            // Kiểm tra champion có thuộc PlayerArea này không
            List<Champion> myChampions = GetAllChampions();
            return myChampions.Contains(champion);
        }

        private bool IsValidDropPosition(Cell cell)
        {
            // Chỉ cho phép drop vào cell thuộc board của mình
            if (board == null) return false;

            if (board.GetCell(cell.x, cell.y) == cell)
            {
                return true; // Cell không thuộc board
            }
            if (bench.GetCell(cell.x, cell.y) == cell)
            {
                return true; // Cell thuộc bench
            }

            return false;
        }

        private bool MoveChampionToCell(Champion champion, Cell targetCell)
        {
            if (champion == null || targetCell == null) return false;

            // Remove champion from current position
            RemoveChampionFromCurrentPosition(champion);

            // Place champion to new cell
            bool success = targetCell.PlaceChampion(champion);
            if (success)
            {
                champion.transform.position = targetCell.transform.position + offset;
                RefreshChampionsList();
                Debug.Log($"Moved {champion.data?.championName} to board position ({targetCell.x}, {targetCell.y})");
                return true;
            }

            return false;
        }


        private void RemoveChampionFromCurrentPosition(Champion champion)
        {
            // Remove from board
            if (board != null)
            {
                for (int x = 0; x < board.width; x++)
                {
                    for (int y = 0; y < board.height; y++)
                    {
                        var slot = board.GetCell(x, y);
                        if (slot != null && slot.champion == champion)
                        {
                            slot.RemoveChampion();
                            break;
                        }
                    }
                }
            }

            // Remove from bench
            if (bench != null)
            {
                BenchSlot[] slots = bench.GetComponentsInChildren<BenchSlot>();
                foreach (var slot in slots)
                {
                    if (slot.champion == champion)
                    {
                        slot.RemoveChampion();
                        break;
                    }
                }
            }
        }

        private void HighlightValidPositions(bool highlight)
        {
            if (board != null)
            {
                // Highlight board cells
                for (int x = 0; x < board.width; x++)
                {
                    for (int y = 0; y < board.height; y++)
                    {
                        var slot = board.GetCell(x, y);
                        if (slot != null && (slot.IsEmpty() || slot.champion == selectedChampion))
                        {
                            slot.Highlight(highlight);
                        }
                    }
                }
            }

            if (bench != null)
            {
                // Highlight bench slots
                BenchSlot[] slots = bench.GetComponentsInChildren<BenchSlot>();
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty() || slot.champion == selectedChampion)
                    {
                        slot.Highlight(highlight);
                    }
                }
            }
        }


        public void EndBattle()
        {
            List<Champion> boardChampions = GetBoardChampions();

            foreach (Champion champion in boardChampions)
            {
                if (champion != null)
                {
                    // Champion không có EndBattle method
                    // champion.EndBattle();
                }
            }
        }

        public int GetAliveBoardChampionsCount()
        {
            int count = 0;
            List<Champion> boardChampions = GetBoardChampions();

            foreach (Champion champion in boardChampions)
            {
                if (champion != null && champion.isAlive)
                {
                    count++;
                }
            }

            return count;
        }

        public void RemoveChampion(Champion champion)
        {
            if (champion == null) return;

            // Try to remove from board first
            if (board != null)
            {
                board.RemoveChampion(champion);
            }

            // Then try to remove from bench
            if (bench != null)
            {
                bench.RemoveChampion(champion);
            }

            RefreshChampionsList();
        }

        public void MoveChampionToBench(Champion champion)
        {
            if (champion == null || bench == null) return;

            // Remove from board
            if (board != null)
            {
                board.RemoveChampion(champion);
            }

            // Add to bench
            bench.AddChampion(champion);
            RefreshChampionsList();
        }

        public void MoveChampionToBoard(Champion champion)
        {
            if (champion == null || board == null) return;

            // Remove from bench
            if (bench != null)
            {
                bench.RemoveChampion(champion);
            }

            // Add to board - tìm vị trí trống đầu tiên
            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.height; y++)
                {
                    var slot = board.GetCell(x, y);
                    if (slot != null && slot.IsEmpty())
                    {
                        board.PlaceChampion(champion, x, y);
                        RefreshChampionsList();
                        return;
                    }
                }
            }
            RefreshChampionsList();
        }

        public void ClearAll()
        {
            if (board != null)
            {
                // Board không có ClearBoard, cần clear từng champion
                List<Champion> boardChampions = board.GetAllChampions();
                foreach (Champion champion in boardChampions)
                {
                    if (champion != null)
                    {
                        board.RemoveChampion(champion);
                        Destroy(champion.gameObject);
                    }
                }
            }

            if (bench != null)
            {
                // Bench không có ClearBench, cần clear từng champion
                List<Champion> benchChampions = bench.GetAllChampions();
                foreach (Champion champion in benchChampions)
                {
                    if (champion != null)
                    {
                        bench.RemoveChampion(champion);
                        Destroy(champion.gameObject);
                    }
                }
            }

            allChampions.Clear();
        }

        public void EnableMovement(bool enable)
        {
            movementEnabled = enable;

            // Cancel current drag if disabling
            if (!enable && isDragging)
            {
                CancelCurrentDrag();
            }
        }

        private void CancelCurrentDrag()
        {
            if (selectedChampion != null)
            {
                selectedChampion.transform.position = originalPosition;
                HighlightValidPositions(false);
            }

            selectedChampion = null;
            isDragging = false;
        }

        // For debugging
        public void DebugInfo()
        {
            Debug.Log($"=== {playerName} Area Info ===");
            Debug.Log($"Board Champions: {GetBoardChampionCount()}");
            Debug.Log($"Bench Champions: {GetBenchChampionCount()}");
            Debug.Log($"Total Champions: {GetChampionCount()}");
            Debug.Log($"Alive Board Champions: {GetAliveBoardChampionsCount()}");
        }
    }
}
