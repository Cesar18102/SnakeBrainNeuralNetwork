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
        //private List<StudentSnake> Snakes { get; set; } = new List<StudentSnake>();
        public StudentSnake Snake { get; private set; }

        private const int BRAIN_WIDTH = 11;
        private const int BRAIN_HEIGHT = 11;
        private const int SNAKE_COUNT = 1;
        private const int INIT_SIZE = 7;

        private PredictWaitEventArgs LastTest { get; set; }
        private Action DrawExtra { get; set; } = () => { };

        private static Font FONT { get; set; } = new Font("res/ArialRegular.ttf");
        private static CircleShape STEP_PREDICTION_POINTER { get; set; } = new CircleShape() { FillColor = Color.Magenta };

        private static CircleShape WALL_GRID_ITEM { get; set; } = new CircleShape(20) { FillColor = Color.Red, OutlineThickness = 2 };
        private static Text WALL_GRID_TEXT_ITEM { get; set; } = new Text() { Font = FONT, FillColor = Color.Black, CharacterSize = 12 };
        private static Vector2f WALL_GRID_OFFSET { get; set; } = new Vector2f(1000, 10);
        private static Vector2f WALL_GRID_ITEM_SIZE { get; set; } = new Vector2f(40, 40);
        private static Color WALL_TRIGGERED_COLOR = Color.Blue;

        private static CircleShape FOOD_GRID_ITEM { get; set; } = new CircleShape(20) { FillColor = Color.Green, OutlineThickness = 2 };
        private static Text FOOD_GRID_TEXT_ITEM { get; set; } = new Text() { Font = FONT, FillColor = Color.Black, CharacterSize = 12 };
        private static Vector2f FOOD_GRID_OFFSET { get; set; } = new Vector2f(1000, 500);
        private static Vector2f FOOD_GRID_ITEM_SIZE { get; set; } = new Vector2f(40, 40);
        private static Color FOOD_TRIGGERED_COLOR = Color.Blue;

        private static RectangleShape BRAIN_SHAPE { get; set; } = new RectangleShape() 
        { 
            FillColor = Color.Transparent, 
            OutlineColor = Color.Red, 
            OutlineThickness = 4 
        };

        public Game(int fieldWidth, int fieldHeight, 
                    float cellWidthPx, float cellHeightPx, 
                    string windowTitle, uint fps, Color backColor)
        {
            STEP_PREDICTION_POINTER.Radius = Math.Min(cellWidthPx, cellHeightPx) / 2;

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

            Snake = new StudentSnake(
                R.Next(INIT_SIZE, fieldHeight - INIT_SIZE - 1),
                R.Next(1, fieldWidth - 1), (uint)INIT_SIZE,
                Color.Red, Color.Blue, BRAIN_HEIGHT, BRAIN_WIDTH
            );

            /*for (int i = 0; i < SNAKE_COUNT; ++i)
                Snakes.Add(new StudentSnake(
                    R.Next(INIT_SIZE, fieldHeight - INIT_SIZE - 1),
                    R.Next(1, fieldWidth - 1), (uint)INIT_SIZE,
                    Color.Red, Color.Blue, BRAIN_HEIGHT, BRAIN_WIDTH
                ));*/

            Snake.OnPredictControlPauseNeeded += (sender, e) =>
            {
                Pause();
                LastTest = e;

                DrawExtra = () =>
                {
                    int x = Snake.Head.Position.X + (e.WantedDirection == Direction.LEFT ? -1 : (e.WantedDirection == Direction.RIGHT ? 1 : 0));
                    int y = Snake.Head.Position.Y + (e.WantedDirection == Direction.TOP ? -1 : (e.WantedDirection == Direction.BOTTOM ? 1 : 0));

                    STEP_PREDICTION_POINTER.Position = new Vector2f(x * GameField.CellWidthPixel, y * GameField.CellWidthPixel);
                    Win.Draw(STEP_PREDICTION_POINTER);

                    DrawTable<CircleShape>(
                        Win, WALL_GRID_ITEM, WALL_GRID_OFFSET, WALL_GRID_ITEM_SIZE, 
                        Snake.Brain.BrainHeight, Snake.Brain.BrainWidth, (s, h, w) => {
                            if (Snake.Brain.IsWallTriggered(new Point(h, w)))
                                s.OutlineColor = WALL_TRIGGERED_COLOR;
                        }
                    );

                    DrawTable<Text>(
                        Win, WALL_GRID_TEXT_ITEM, WALL_GRID_OFFSET, WALL_GRID_ITEM_SIZE, Snake.Brain.BrainHeight,
                        Snake.Brain.BrainWidth, (s, h, w) => s.DisplayedString = Snake.Brain[h, w].WallNeuron.ToString()
                    );

                    DrawTable<CircleShape>(
                        Win, FOOD_GRID_ITEM, FOOD_GRID_OFFSET, FOOD_GRID_ITEM_SIZE,
                        Snake.Brain.BrainHeight, Snake.Brain.BrainWidth, (s, h, w) => {
                            if (Snake.Brain.IsFoodTriggered(new Point(h, w)))
                                s.OutlineColor = FOOD_TRIGGERED_COLOR;
                        }
                    );

                    DrawTable<Text>(
                        Win, FOOD_GRID_TEXT_ITEM, FOOD_GRID_OFFSET, FOOD_GRID_ITEM_SIZE, Snake.Brain.BrainHeight,
                        Snake.Brain.BrainWidth, (s, h, w) => s.DisplayedString = Snake.Brain[h, w].FoodNeuron.ToString()
                    );
                };
            };

            Snake.OnContinueGame += (sender, e) => Continue();

            /*Snakes.ForEach(snake =>
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
            });*/
        }

        public void Close()
        {
            Win.Close();
        }

        private void DrawTable<T>(RenderTarget target, T shape, Vector2f offset, Vector2f size, int height, int width, Action<T, int, int> predraw) where T : Transformable, Drawable
        {
            for(int i = 0; i < height; ++i)
                for(int j = 0; j < width; ++j)
                {
                    T copy = typeof(T).GetConstructor(new Type[] { typeof(T) }).Invoke(new object[] { shape }) as T;
                    copy.Position = new Vector2f(j * size.X, i * size.Y) + offset;
                    predraw(copy, i, j);
                    copy.Draw(target, RenderStates.Default);
                }
        }

        private void Win_KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.W)
                LastTest?.Answer(Direction.TOP);
            else if (e.Code == Keyboard.Key.A)
                LastTest?.Answer(Direction.LEFT);
            else if (e.Code == Keyboard.Key.D)
                LastTest?.Answer(Direction.RIGHT);
            else if (e.Code == Keyboard.Key.S)
                LastTest?.Answer(Direction.BOTTOM);
            else if (e.Code == Keyboard.Key.P)
            {
                WF.FileDialog dialog = new WF.SaveFileDialog();
                if (dialog.ShowDialog() == WF.DialogResult.OK)
                    Snake.Brain.Save(dialog.FileName);
            }
            else if (e.Code == Keyboard.Key.L)
            {
                WF.FileDialog dialog = new WF.OpenFileDialog();
                if (dialog.ShowDialog() == WF.DialogResult.OK)
                    Snake.LoadBrainFromFile(dialog.FileName);
                    //Snakes.ForEach(snake => snake.LoadBrainFromFile(dialog.FileName));
            }
            else if (e.Code == Keyboard.Key.O) Study();
            else if (e.Code == Keyboard.Key.K) Play();
            else if (e.Code == Keyboard.Key.R) Continue();
        }

        public void Play()
        {
            Snake.OnDie += (sender, e) =>
            {
                Pause();

                DrawExtra = () =>
                {
                    Text gameOverText = new Text("Game over", FONT, 48) { FillColor = Color.Black };
                    gameOverText.Position = new Vector2f((Win.Size.X - gameOverText.CharacterSize * gameOverText.DisplayedString.Length) / 2, Win.Size.Y / 2);
                    Win.Draw(gameOverText);
                };
            };
            
            /*Snakes.ForEach(snake =>
            {
                snake.OnDie += (ctx, args) =>
                {
                    foreach (FieldCellBase cell in snake.Body)
                        GameField[cell.Position.Y, cell.Position.X] = new FieldCellWall(cell.Position.Y, cell.Position.X);
                    GameField[snake.Head.Position.Y, snake.Head.Position.X] = new FieldCellWall(snake.Head.Position.Y, snake.Head.Position.X);
                };
            });*/

            Continue();
            Snake.Play();
            //Snakes.ForEach(snake => snake.Play());
            DrawExtra = () => { };
        }

        public void Study() =>
            Snake.Study();
            //Snakes.ForEach(snake => snake.Study());

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
                    /*Snakes.RemoveAll(snake => snake.Dead);
                    GameField.Update(Snakes.Cast<SnakeBase>().ToList());
                    Snakes.ForEach(snake => snake.Update(GameField, Snakes.Where(s => s != snake).Cast<SnakeBase>().ToList()));*/

                    GameField.Update(new List<SnakeBase>() { Snake });
                    Snake.Update(GameField, new List<SnakeBase>());
                }

                Win.Draw(GameField);
                Snake.Draw(Win, RenderStates.Default, GameField.CellWidthPixel, GameField.CellHeightPixel);
                //Snakes.ForEach(snake => snake.Draw(Win, RenderStates.Default, GameField.CellWidthPixel, GameField.CellHeightPixel));

                BRAIN_SHAPE.Size = new Vector2f(
                    Snake.Brain.BrainWidth * GameField.CellWidthPixel,
                    Snake.Brain.BrainHeight * GameField.CellHeightPixel
                );

                BRAIN_SHAPE.Position = new Vector2f(
                    (Snake.Head.Position.X - Snake.Brain.BrainWidth / 2) * GameField.CellWidthPixel,
                    (Snake.Head.Position.Y - Snake.Brain.BrainHeight / 2) * GameField.CellHeightPixel
                );

                Win.Draw(BRAIN_SHAPE);

                DrawExtra();
                Win.Display();
            }
        }
    }
}
