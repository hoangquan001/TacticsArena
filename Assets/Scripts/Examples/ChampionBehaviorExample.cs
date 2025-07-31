using UnityEngine;
using TacticsArena.Battle;

namespace TacticsArena.Examples
{
    /// <summary>
    /// Example script showing how to use Champion behaviors
    /// </summary>
    public class ChampionBehaviorExample : MonoBehaviour
    {
        [Header("Champion References")]
        public Champion testChampion;
        
        [Header("Test Controls")]
        public KeyCode forceIdleKey = KeyCode.I;
        public KeyCode stopBattleKey = KeyCode.S;
        public KeyCode startBattleKey = KeyCode.B;
        public KeyCode takeDamageKey = KeyCode.D;
        
        private void Update()
        {
            if (testChampion == null) return;
            
            HandleTestInputs();
            DisplayChampionInfo();
        }
        
        private void HandleTestInputs()
        {
            if (Input.GetKeyDown(forceIdleKey))
            {
                testChampion.ForceIdle();
                Debug.Log("Forced champion to idle state");
            }
            
            if (Input.GetKeyDown(stopBattleKey))
            {
                testChampion.StopBattle();
                Debug.Log("Stopped champion battle");
            }
            
            if (Input.GetKeyDown(startBattleKey))
            {
                testChampion.StartBattle();
                Debug.Log("Started champion battle");
            }
            
            if (Input.GetKeyDown(takeDamageKey))
            {
                testChampion.TakeDamage(20f);
                Debug.Log("Champion took 20 damage");
            }
        }
        
        private void DisplayChampionInfo()
        {
            // This could be connected to UI elements
            string info = $"Champion: {testChampion.data?.championName ?? "Unknown"}\n" +
                         $"State: {testChampion.currentState}\n" +
                         $"Health: {testChampion.currentHealth:F1}\n" +
                         $"Mana: {testChampion.currentMana:F1}\n" +
                         $"Is Alive: {testChampion.isAlive}\n" +
                         $"In Combat: {testChampion.IsInCombat()}\n" +
                         $"Health %: {testChampion.GetHealthPercentage():P1}\n" +
                         $"Mana %: {testChampion.GetManaPercentage():P1}";
            
            // You can use this info for UI updates
            // For now, just log it occasionally
            if (Time.frameCount % 60 == 0) // Every 60 frames
            {
                Debug.Log(info);
            }
        }
        
        private void OnGUI()
        {
            if (testChampion == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Champion Behavior Test");
            GUILayout.Label($"State: {testChampion.currentState}");
            GUILayout.Label($"Health: {testChampion.GetHealthPercentage():P1}");
            GUILayout.Label($"Mana: {testChampion.GetManaPercentage():P1}");
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label($"I - Force Idle");
            GUILayout.Label($"S - Stop Battle");
            GUILayout.Label($"B - Start Battle");
            GUILayout.Label($"D - Take Damage");
            
            GUILayout.EndArea();
        }
    }
}
