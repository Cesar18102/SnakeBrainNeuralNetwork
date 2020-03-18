using System;
using System.Linq;
using WF = System.Windows.Forms;
using System.Collections.Generic;

using SFML.Window;
using SFML.System;
using SFML.Graphics;

using SnakeBrain.SnakeGame.Snakes;
using SnakeBrain.SnakeGame.FieldCells;
using SnakeBrain.SnakeGame.Snakes.SnakeEventArgs;

namespace SnakeBrain.SnakeGame
{
    public class Game
    {
        private Random R = new Random();

        private uint FPS { get; set; }
        private string Title { get; set; }
        private Color BackColor { get; set; }
        private RenderWindow Win { get; set; }
        public bool Stopped { get; private set; }

        private Field GameField { get; set; }
        private List<StudentSnake> Snakes { get; set; } = new List<StudentSnake>();
        //private StudentSnake Snake { get; set; }

        private const int BRAIN_WIDTH = 11;
        private const int BRAIN_HEIGHT = 11;
        private const int SNAKE_COUNT = 3;
        private const int INIT_SIZE = 7;

        private PredictWaitEventArgs LastTest { get; set; }
        private Action DrawExtra { get; set; } = () => { };
        private static Font Font { get; set; } = new Font("res/ArialRegular.ttf");

        public Game(int fieldWidth, int fieldHeight, 
                    float cellWidthPx, float cellHeightPx, 
                    string windowTitle, uint fps, Color backColor)
        {
            FPS = fps;
            Title = windowTitle;
            BackColor = backColor;

            Win = new RenderWindow(VideoMode.FullscreenModes[0], Title);
            GameField = new Field(fieldWidth, fieldHeight, cellWidthPx, cellHeightPx);

            Win.SetFramerateLimit(FPS);
            Win.KeyReleased += Win_KeyReleased;
            Win.Closed += (sender, e) => Win.Close();

            for (int i = 0; i < fieldHeight; i++)
                for (int j = 0; j < fieldWidth; j++)
                    GameField[i, j] = i == 0 || j == 0 || i == fieldHeight - 1 || j == fieldWidth - 1 ? 
                        new FieldCellWall(i, j) as FieldCellBase : new FieldCellEmpty(i, j);

            for (int i = 0; i < SNAKE_COUNT; ++i)
                Snakes.Add(new StudentSnake(
                    R.Next(INIT_SIZE, fieldHeight - INIT_SIZE - 1),
                    R.Next(1, fieldWidth - 1), (uint)INIT_SIZE,
                    Color.Red, Color.Blue, BRAIN_HEIGHT, BRAIN_WIDTH
                ));

            Snakes.ForEach(snake =>
            {
                snake.OnPredictControlPauseNeeded += (sender, e) =>
                {
                    Pause();
                    LastTest = e;

                    DrawExtra = () =>
                    {
                        int x = snake.Head.Position.X + (e.WantedDirection == Direction.LEFT ? -1 : (e.WantedDirection == Direction.RIGHT ? 1 : 0));
                        int y = snake.Head.Position.Y + (e.WantedDirection == Direction.TOP ? -1 : (e.WantedDirection == Direction.BOTTOM ? 1 : 0));

                        Win.Draw(new CircleShape(GameField.CellWidthPixel / 2) { Position = new Vector2f(x * GameField.CellWidthPixel, y * GameField.CellWidthPixel), FillColor = Color.Yellow });
                        for (int i = 0; i < snake.Brain.BrainHeight; ++i)
                            for (int j = 0; j < snake.Brain.BrainWidth; ++j)
                            {
                                Vector2f position = new Vector2f(
                                    (snake.Head.Position.X - snake.Brain.BrainWidth / 2 + j) * GameField.CellWidthPixel,
                                    (snake.Head.Position.Y - snake.Brain.BrainHeight / 2 + i) * GameField.CellHeightPixel
                                );

                                Text text = new Text(snake.Brain[i, j].ToString(), Font) { Position = position, FillColor = Color.Cyan, CharacterSize = 9 };
                                text.Draw(Win, RenderStates.Default);
                            }
                    };
                };

                snake.OnContinueGame += (sender, e) => Continue();
            });
        }

        private void Win_KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.W)
                LastTest.Answer(Direction.TOP);
            else if (e.Code == Keyboard.Key.A)
                LastTest.Answer(Direction.LEFT);
            else if (e.Code == Keyboard.Key.D)
                LastTest.Answer(Direction.RIGHT);
            else if (e.Code == Keyboard.Key.S)
                LastTest.Answer(Direction.BOTTOM);
            else if (e.Code == Keyboard.Key.P)
            {
                /*WF.FileDialog dialog = new WF.SaveFileDialog();
                if (dialog.ShowDialog() == WF.DialogResult.OK)
                    Snake.Brain.Save(dialog.FileName);*/
            }
            else if (e.Code == Keyboard.Key.L)
            {
                WF.FileDialog dialog = new WF.OpenFileDialog();
                if (dialog.ShowDialog() == WF.DialogResult.OK)
                    Snakes.ForEach(snake => snake.LoadBrainFromFile(dialog.FileName));
            }
            else if (e.Code == Keyboard.Key.O) Study();
            else if (e.Code == Keyboard.Key.K) Play();
            else if (e.Code == Keyboard.Key.R) Continue();
        }

        public void Play()
        {
            Snakes.ForEach(snake =>
            {
                snake.OnDie += (ctx, args) =>
                {
                    foreach (FieldCellBase cell in snake.Body)
                        GameField[cell.Position.Y, cell.Position.X] = new FieldCellWall(cell.Position.Y, cell.Position.X);
                    GameField[snake.Head.Position.Y, snake.Head.Position.X] = new FieldCellWall(snake.Head.Position.Y, snake.Head.Position.X);

                    /*Pause();

                    DrawExtra = () =>
                    {
                        Text gameOverText = new Text("Game over", Font, 48) { FillColor = Color.Black };
                        gameOverText.Position = new Vector2f((Win.Size.X - gameOverText.CharacterSize * gameOverText.DisplayedString.Length) / 2, Win.Size.Y / 2);
                        Win.Draw(gameOverText);
                    };*/
                };
            });

            Continue();
            Snakes.ForEach(snake => snake.Play());
            DrawExtra = () => { };
        }

        public void Study() =>
            Snakes.ForEach(snake => snake.Study());

        public void Pause() =>
            Stopped = true;

        public void Continue() =>
            Stopped = false;

        public void Start()
        {
            DateTime currentTime = DateTime.Now;

            while (Win.IsOpen)
            {
                Win.DispatchEvents();

                double elapsed = (DateTime.Now - currentTime).TotalSeconds;

                if (elapsed < 1.0 / FPS)
                    continue;

                currentTime = DateTime.Now;

                Win.Clear(BackColor);

                if (!Stopped)
                {
                    Snakes.RemoveAll(snake => snake.Dead);
                    GameField.Update(Snakes.Cast<SnakeBase>().ToList());
                    Snakes.ForEach(snake => snake.Update(GameField, Snakes.Where(s => s != snake).Cast<SnakeBase>().ToList()));
                }

                Win.Draw(GameField);
                Snakes.ForEach(snake => snake.Draw(Win, RenderStates.Default, GameField.CellWidthPixel, GameField.CellHeightPixel));

                DrawExtra();
                Win.Display();
            }
        }
    }
}
