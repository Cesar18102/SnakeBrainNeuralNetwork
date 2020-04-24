using System;
using System.Linq;
using System.Collections.Generic;

using SFML.Graphics;

using SnakeBrain.SnakeGame.Snakes;
using SnakeBrain.SnakeGame.FieldCells;

namespace SnakeBrain.SnakeGame
{
    public class Field : Drawable
    {
        private Random R = new Random();

        public int Width { get; private set; }
        public int Height { get; private set; }

        public float CellWidthPixel { get; private set; }
        public float CellHeightPixel { get; private set; }

        private FieldCellBase[,] Cells { get; set; }
        public FieldCellBase this[int y, int x]
        {
            get => y >= 0 && y < Height && x >= 0 && x < Width ? Cells[y, x] : null;
            set
            {
                if (y >= 0 && y < Height && x >= 0 && x < Width)
                    Cells[y, x] = value;
            }
        }

        public Field(int width, int height, float cellWidthPx, float cellHeightPx)
        {
            Width = width;
            Height = height;

            CellWidthPixel = cellWidthPx;
            CellHeightPixel = cellHeightPx;

            Cells = new FieldCellBase[Height, Width];
        }

        public void Update(IEnumerable<SnakeBase> snakes)
        {
            if(R.NextDouble() < 0.05)
            {
                int x = R.Next(0, Width);
                int y = R.Next(0, Height);

                if (Cells[y, x] is FieldCellEmpty && !snakes.Any(S => S.ContainsCell(Cells[y, x])))
                    Cells[y, x] = new FieldCellFood(y, x);
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    Cells[i, j]?.Draw(target, states, CellWidthPixel, CellHeightPixel);
        }
    }
}
