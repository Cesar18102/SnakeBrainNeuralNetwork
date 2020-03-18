using SFML.Graphics;

namespace SnakeBrain.SnakeGame.FieldCells
{
    public class FieldCellFood : FieldCellBase
    {
        public FieldCellFood(int y, int x) :
            base(y, x, Color.Green, Color.Green, 2) { }
    }
}
