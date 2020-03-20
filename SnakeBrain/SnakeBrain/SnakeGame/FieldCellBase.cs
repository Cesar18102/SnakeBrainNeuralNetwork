using SFML.System;
using SFML.Graphics;

namespace SnakeBrain.SnakeGame
{
    public class FieldCellBase
    {
        public Point Position { get; protected set; }

        public Color FillColor { get; private set; }
        public Color OutlineColor { get; private set; }
        public float OutlineThickness { get; private set; }

        public FieldCellBase(int y, int x, Color fillColor, Color outlineColor, float outlineThickness)
        {
            Position = new Point(y, x);
            FillColor = fillColor;
            OutlineColor = outlineColor;
            OutlineThickness = outlineThickness;
        }

        public void FitToSize(int w, int h) =>
            Position.MoveTo(
                (Position.Y % h + h) % h,
                (Position.X % w + w) % w
            );

        public void Draw(RenderTarget target, RenderStates states, float width, float height)
        {
            Vector2f size = new Vector2f(width, height);

            RectangleShape shape = new RectangleShape(size)
            {
                FillColor = FillColor,
                OutlineColor = OutlineColor,
                OutlineThickness = OutlineThickness,
                Position = new Vector2f(Position.X * width, Position.Y * height)
            };

            target.Draw(shape, states);
        }
    }
}
