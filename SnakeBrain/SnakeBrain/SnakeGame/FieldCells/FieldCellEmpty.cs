using SFML.Graphics;

namespace SnakeBrain.SnakeGame.FieldCells
{
    public class FieldCellEmpty : FieldCellBase
    {
        public FieldCellEmpty(int y, int x) :
            base(y, x, Color.White, Color.Black, 2) { }
    }
}
