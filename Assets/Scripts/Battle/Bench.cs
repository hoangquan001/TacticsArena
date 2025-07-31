using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TacticsArena.Battle
{
    public class Bench : MonoBehaviour
    {
        [Header("Bench Settings")]
        public int maxBenchSize = 9; // TFT thường có 9 slots bench
        public float slotSpacing = 1.5f;
        
        [Header("Prefabs")]
        public GameObject benchSlotPrefab;
        
        [Header("Visual")]
        
        private BenchSlot[] benchSlots;
        private List<Champion> championsOnBench = new List<Champion>();

        private void Start()
        {
            // GenerateBenchSlots();
            Initialize();
        }
        
        [ContextMenu("Generate Bench")]
        private void GenerateBenchSlots()
        {
            
            benchSlots = new BenchSlot[maxBenchSize];
            for (int i = 0; i < maxBenchSize; i++)
            {
                Vector3 position = GetSlotPosition(i);
                GameObject slotObject = Instantiate(benchSlotPrefab, position, Quaternion.AngleAxis(45, Vector3.up), this.transform);
                slotObject.transform.localPosition = position;
                BenchSlot benchSlot = slotObject.GetComponent<BenchSlot>();
                if (benchSlot == null)
                    benchSlot = slotObject.AddComponent<BenchSlot>();
                
                benchSlot.Initialize(i, this);
                benchSlots[i] = benchSlot;
                
                slotObject.name = $"BenchSlot_{i}";
            }
        }
        
        private Vector3 GetSlotPosition(int slotIndex)
        {
            // Arrange slots horizontally
            float x = slotIndex * slotSpacing;
            return new Vector3(x, 0, 0);
        }
        
        public bool AddChampion(Champion champion)
        {
            // Tìm slot trống đầu tiên
            for (int i = 0; i < benchSlots.Length; i++)
            {
                if (benchSlots[i].IsEmpty())
                {
                    return PlaceChampionInSlot(champion, i);
                }
            }
            
            Debug.Log("Bench is full!");
            return false;
        }
        
        public bool PlaceChampionInSlot(Champion champion, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= benchSlots.Length)
                return false;
            
            BenchSlot slot = benchSlots[slotIndex];
            if (!slot.IsEmpty())
                return false;
            
            // Remove from previous location
            RemoveChampion(champion);
            
            // Place in new slot
            slot.PlaceChampion(champion);
            champion.transform.position = slot.transform.position + Vector3.up * 0.5f;
            champion.transform.SetParent(slot.transform);
            
            if (!championsOnBench.Contains(champion))
                championsOnBench.Add(champion);
            
            return true;
        }
        
        public void RemoveChampion(Champion champion)
        {
            if (championsOnBench.Contains(champion))
            {
                championsOnBench.Remove(champion);
                
                // Tìm slot chứa champion này và clear
                foreach (var slot in benchSlots)
                {
                    if (slot.champion == champion)
                    {
                        slot.RemoveChampion();
                        break;
                    }
                }
                
                champion.transform.SetParent(null);
            }
        }
        
        public Champion GetChampionInSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < benchSlots.Length)
                return benchSlots[slotIndex].champion;
            return null;
        }
        
        public List<Champion> GetAllChampions()
        {
            return new List<Champion>(championsOnBench);
        }
        
        public int GetEmptySlotCount()
        {
            return benchSlots.Count(slot => slot.IsEmpty());
        }
        
        public bool IsFull()
        {
            return GetEmptySlotCount() == 0;
        }
        
        public bool IsEmpty()
        {
            return championsOnBench.Count == 0;
        }
        
        public BenchSlot GetSlot(int index)
        {
            if (index >= 0 && index < benchSlots.Length)
                return benchSlots[index];
            return null;
        }
        
        public int GetSlotIndex(BenchSlot slot)
        {
            for (int i = 0; i < benchSlots.Length; i++)
            {
                if (benchSlots[i] == slot)
                    return i;
            }
            return -1;
        }
        
        // Swap champions between two slots
        public void SwapChampions(int slot1Index, int slot2Index)
        {
            if (slot1Index < 0 || slot1Index >= benchSlots.Length ||
                slot2Index < 0 || slot2Index >= benchSlots.Length)
                return;
            
            BenchSlot slot1 = benchSlots[slot1Index];
            BenchSlot slot2 = benchSlots[slot2Index];
            
            Champion champion1 = slot1.champion;
            Champion champion2 = slot2.champion;
            
            // Clear both slots
            slot1.RemoveChampion();
            slot2.RemoveChampion();
            
            // Place champions in swapped positions
            if (champion1 != null)
                PlaceChampionInSlot(champion1, slot2Index);
            if (champion2 != null)
                PlaceChampionInSlot(champion2, slot1Index);
        }

        internal void Initialize()
        {

            benchSlots = this.GetComponentsInChildren<BenchSlot>();
            if (benchSlots == null || benchSlots.Length == 0)
            {
                GenerateBenchSlots();
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

        internal void ClearBench()
        {
            throw new NotImplementedException();
        }

        internal Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= maxBenchSize || y != 0)
                return null; // Bench chỉ có một hàng
            
            return benchSlots[x];
        }

        // Event for UI updates
        public static System.Action<Champion> ChampionAddedToBench;
        public static System.Action<Champion> ChampionRemovedFromBench;
    }
}
