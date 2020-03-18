using SFML.Graphics;

namespace SnakeBrain.SnakeGame.FieldCells
{
    public class FieldCellWall : FieldCellBase
    {
        public FieldCellWall(int y, int x) : 
            base(y, x, Color.Black, Color.Black, 2) { }
    }
}
