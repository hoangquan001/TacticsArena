using UnityEngine;
using System.Collections;

namespace TacticsArena.Core
{
    public class TurnManager : MonoBehaviour
    {
        [Header("Turn Settings")]
        public float preparationTime = 30f;
        public float battleTime = 60f;
        public float postBattleTime = 5f;
        
        private Coroutine currentPhaseCoroutine;
        
        public void StartPreparationPhase()
        {
            if (currentPhaseCoroutine != null)
                StopCoroutine(currentPhaseCoroutine);
                
            currentPhaseCoroutine = StartCoroutine(PreparationPhaseCoroutine());
        }
        
        public void StartBattlePhase()
        {
            if (currentPhaseCoroutine != null)
                StopCoroutine(currentPhaseCoroutine);
                
            currentPhaseCoroutine = StartCoroutine(BattlePhaseCoroutine());
        }
        
        public void StartPostBattlePhase()
        {
            if (currentPhaseCoroutine != null)
                StopCoroutine(currentPhaseCoroutine);
                
            currentPhaseCoroutine = StartCoroutine(PostBattlePhaseCoroutine());
        }
        
        private IEnumerator PreparationPhaseCoroutine()
        {
            float timeRemaining = preparationTime;
            
            while (timeRemaining > 0)
            {
                // Update UI timer
                timeRemaining -= Time.deltaTime;
                yield return null;
            }
            
            // Chuyển sang battle phase
            GameManager.Instance.ChangeState(GameState.Battle);
        }
        
        private IEnumerator BattlePhaseCoroutine()
        {
            float timeRemaining = battleTime;
            
            while (timeRemaining > 0 && !IsBattleFinished())
            {
                timeRemaining -= Time.deltaTime;
                yield return null;
            }
            
            // Chuyển sang post battle phase
            GameManager.Instance.ChangeState(GameState.PostBattle);
        }
        
        private IEnumerator PostBattlePhaseCoroutine()
        {
            yield return new WaitForSeconds(postBattleTime);
            
            // Tăng round và bắt đầu round mới
            GameManager.Instance.currentRound++;
            GameManager.Instance.ChangeState(GameState.Preparation);
        }
        
        private bool IsBattleFinished()
        {
            // Logic kiểm tra battle đã kết thúc chưa
            // Ví dụ: tất cả champion của một bên đã chết
            return false;
        }
    }
}
