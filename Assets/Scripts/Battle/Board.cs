using UnityEngine;
using System.Collections.Generic;
using System;

namespace TacticsArena.Battle
{
    public class Board : MonoBehaviour
    {
        [Header("Board Settings")]
        public int width = 8;
        public int height = 8;
        public float slotSize = 1f;
        
        [Header("Prefabs")]
        public GameObject slotPrefab;
        
        private BoardSlot[,] Grid;
        private List<Champion> championsOnBoard = new List<Champion>();

        private void Start()
        {
            Initialize();
            // GenerateBoard();
        }

        [ContextMenu("Generate Board")]
        private void GenerateBoard()
        {
            Grid = new BoardSlot[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = ToLocalPosition(x, y);
                    GameObject slotObject = Instantiate(slotPrefab, this.transform.TransformPoint(position), Quaternion.AngleAxis(90, Vector3.up), transform);
                    slotObject.name = $"Slot_{x}_{y}";
                    BoardSlot boardSlot = slotObject.GetComponent<BoardSlot>();
                    if (boardSlot == null)
                        boardSlot = slotObject.AddComponent<BoardSlot>();
                    
                    boardSlot.Initialize(x, y, this);
                    Grid[x, y] = boardSlot;
                }
            }
        }
        
        private Vector3 ToLocalPosition(int x, int y)
        {
            float worldX = x * slotSize;
            float worldY = y * slotSize;

            // Offset cho hàng lẻ
            if (y % 2 == 1)
            {
                worldX += slotSize / 2;
            }
            worldY = y * slotSize * Mathf.Sqrt(3) / 2;


            return new Vector3(worldX, 0, worldY);
        }
        
        public BoardSlot GetCell(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
                return Grid[x, y];
            return null;
        }
        
        public bool PlaceChampion(Champion champion, int x, int y)
        {
            BoardSlot slot = GetCell(x, y);
            if (slot != null && slot.IsEmpty())
            {
                slot.PlaceChampion(champion);
                champion.transform.position = slot.transform.position + Vector3.up * 0.5f;
                
                if (!championsOnBoard.Contains(champion))
                    championsOnBoard.Add(champion);
                
                return true;
            }
            return false;
        }
        
        public void RemoveChampion(Champion champion)
        {
            if (championsOnBoard.Contains(champion))
            {
                championsOnBoard.Remove(champion);
                
                // Tìm hex chứa champion này và clear
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (Grid[x, y].champion == champion)
                        {
                            Grid[x, y].RemoveChampion();
                            break;
                        }
                    }
                }
            }
        }
        
        public List<Champion> GetAllChampions()
        {
            return new List<Champion>(championsOnBoard);
        }
        
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        
        public bool ContainsCell(Cell cell)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Grid[x, y] == cell)
                        return true;
                }
            }
            return false;
        }
        
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            // Convert world position back to hex coordinates
            float x = worldPosition.x / (slotSize * 0.75f);
            float y = (worldPosition.z - (int)x % 2 * slotSize * Mathf.Sqrt(3) * 0.25f) / (slotSize * Mathf.Sqrt(3) * 0.5f);

            return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }

        internal void Initialize()
        {
            Grid = new BoardSlot[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Grid[x, y] = this.transform.Find($"Slot_{x}_{y}").GetComponent<BoardSlot>();
                }
            }
        }

        internal bool HasEmptySlot()
        {
            throw new NotImplementedException();
        }

        internal bool TryPlaceChampion(Champion champion)
        {
            throw new NotImplementedException();
        }

        internal void ClearBoard()
        {
            throw new NotImplementedException();
        }
    }
}
