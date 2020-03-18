using System;

using SFML.Graphics;

using SnakeBrain.SnakeGame;

namespace SnakeBrain
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Game game = new Game(30, 30, 30, 30, "test", 20, Color.White);
            game.Start();
        }
    }
}
