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
        public float hexSize = 1f;
        
        [Header("Prefabs")]
        public GameObject hexPrefab;
        
        private Hex[,] hexGrid;
        private List<Champion> championsOnBoard = new List<Champion>();
        
        private void Start()
        {
            GenerateBoard();
        }
        [ContextMenu("Generate Board")]
        private void GenerateBoard()
        {
            hexGrid = new Hex[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = HexToWorldPosition(x, y);
                    GameObject hexObject = Instantiate(hexPrefab, position, Quaternion.AngleAxis(90, Vector3.up), transform);
                    
                    Hex hex = hexObject.GetComponent<Hex>();
                    if (hex == null)
                        hex = hexObject.AddComponent<Hex>();
                    
                    hex.Initialize(x, y, this);
                    hexGrid[x, y] = hex;
                }
            }
        }
        
        private Vector3 HexToWorldPosition(int x, int y)
        {
            float worldX = x * hexSize;
            float worldY = y * hexSize;

            // Offset cho hàng lẻ
            if (y % 2 == 1)
            {
                worldX += hexSize / 2;
            }
            worldY = y * hexSize * Mathf.Sqrt(3) / 2;


            return new Vector3(worldX, 0, worldY);
        }
        
        public Hex GetHex(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
                return hexGrid[x, y];
            return null;
        }
        
        public bool PlaceChampion(Champion champion, int x, int y)
        {
            Hex hex = GetHex(x, y);
            if (hex != null && hex.IsEmpty())
            {
                hex.PlaceChampion(champion);
                champion.transform.position = hex.transform.position + Vector3.up * 0.5f;
                
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
                        if (hexGrid[x, y].champion == champion)
                        {
                            hexGrid[x, y].RemoveChampion();
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
        
        public Vector2Int WorldToHexPosition(Vector3 worldPosition)
        {
            // Convert world position back to hex coordinates
            float x = worldPosition.x / (hexSize * 0.75f);
            float y = (worldPosition.z - (int)x % 2 * hexSize * Mathf.Sqrt(3) * 0.25f) / (hexSize * Mathf.Sqrt(3) * 0.5f);
            
            return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }

        internal void Initialize()
        {
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
