using UnityEngine;

namespace TacticsArena.Battle
{
    public class Hex : Cell
    {
        public Board board;
    
        
        public void Initialize(int hexX, int hexY, Board parentBoard)
        {
            x = hexX;
            y = hexY;
            board = parentBoard;
            
            // Xác định side dựa trên vị trí
            isPlayerSide = y < parentBoard.height / 2;
            
            UpdateVisual();
        }
        
        
    }
}
