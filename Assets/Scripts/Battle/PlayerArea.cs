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
        
        private List<Champion> allChampions = new List<Champion>();
        
        public void Initialize(string name, bool isLocal = true)
        {
            playerName = name;
            isLocalPlayer = isLocal;
            
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
                    var hex = board.GetHex(x, y);
                    if (hex != null && hex.IsEmpty())
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
            
            // Tìm vị trí trống đầu tiên trên board
            for (int x = 0; x < board.width; x++)
            {
                for (int y = 0; y < board.height; y++)
                {
                    var hex = board.GetHex(x, y);
                    if (hex != null && hex.IsEmpty())
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
            
            bool success = bench.AddChampion(champion);
            if (success)
            {
                RefreshChampionsList();
            }
            return success;
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
        
        Champion selectedChampion;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    Champion champion = hit.transform.GetComponent<Champion>();
                    if (champion != null)
                    {
                        Debug.Log("Champion clicked: " + champion.name);
                        selectedChampion = champion;
                    }
                }
            }
            if (Input.GetMouseButton(0) && selectedChampion != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("PlayerArea")))
                {
                    Vector3 newPosition = hit.point;
                    newPosition.y = selectedChampion.transform.position.y; // Giữ nguyên Y để không bị rơi xuống đất
                    selectedChampion.transform.position = newPosition;
                }
                if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, LayerMask.GetMask("Hex")))
                {
                    Hex hex = hit2.transform.GetComponent<Hex>();
                    if (hex != null && hex.IsEmpty())
                    {
                        hex.Highlight(true);
                    }
                }


            }
            if (Input.GetMouseButtonUp(0) && selectedChampion != null)
            {
                // Right click to remove champion
                selectedChampion = null;
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
                    var hex = board.GetHex(x, y);
                    if (hex != null && hex.IsEmpty())
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
