using System;
using WF = System.Windows.Forms;
using System.Collections.Generic;

using SFML.Window;
using SFML.System;
using SFML.Graphics;

using SnakeBrain.SnakeGame.Snakes;
using SnakeBrain.SnakeGame.FieldCells;
using SnakeBrain.SnakeGame.Snakes.Brain;
using SnakeBrain.SnakeGame.Snakes.SnakeEventArgs;
using System.IO;
using Newtonsoft.Json;

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

        private int INIT_SNAKE_COUNT = 5;
        private int INIT_STUDENT_NRG = 750;
        private int NRG_PER_FOOD = 250;
        private int CREATE_NRG = 1500;
        private int DIE_NRG = 0;

        private const int INIT_SIZE = 7;
        private const int BRAIN_WIDTH = 11;
        private const int BRAIN_HEIGHT = 11;

        private class Result
        {
            public BrainBase Brain { get; private set; }
            public int Score { get; private set; }
            public int Produced { get; private set; }

            public Result(BrainBase brain, int score, int produced)
            {
                Brain = brain;
                Score = score;
                Produced = produced;
            }
        }

        private List<DividingStudentSnake> Snakes = new List<DividingStudentSnake>();
        private List<DividingStudentSnake> CreatedSnakes = new List<DividingStudentSnake>();

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

            Pause();
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
                /*WF.FileDialog dialog = new WF.SaveFileDialog();
                if (dialog.ShowDialog() == WF.DialogResult.OK)
                    Snake.Brain.Save(dialog.FileName);*/
            }
            else if (e.Code == Keyboard.Key.L)
            {
                StudentSnake protoSnake = new StudentSnake(Color.Red, Color.Blue, BRAIN_HEIGHT, BRAIN_WIDTH, 0, 0, INIT_SIZE);

                WF.FileDialog dialog = new WF.OpenFileDialog();
                if (dialog.ShowDialog() == WF.DialogResult.OK)
                    protoSnake.LoadBrainFromFile(dialog.FileName);

                for (int i = 0; i < INIT_SNAKE_COUNT; ++i)
                {
                    DividingStudentSnake clone = new DividingStudentSnake(protoSnake, INIT_STUDENT_NRG, CREATE_NRG, DIE_NRG, NRG_PER_FOOD);
                    clone.MoveHeadTo(R.Next(INIT_SIZE, GameField.Height - INIT_SIZE - 1), R.Next(1, GameField.Width - 1));
                    clone.OnDevide += Clone_OnDevide;
                    clone.OnDie += Clone_OnDie;
                    Snakes.Add(clone);
                }

                Continue();
            }
            else if (e.Code == Keyboard.Key.K) Play();
            else if (e.Code == Keyboard.Key.R) Continue();
        }

        private void Clone_OnDie(object sender, EventArgs e)
        {
            DividingStudentSnake died = sender as DividingStudentSnake;
            Result result = new Result(died.Brain, died.Body.Count + 1, died.TotalCreated);

            using (StreamWriter strw = new StreamWriter(Environment.CurrentDirectory + "/Natural/brains.txt", true))
                strw.WriteLine(JsonConvert.SerializeObject(result) + ", ");
        }

        private void Clone_OnDevide(object sender, SnakeCreatedEventArgs e)
        {
            e.Created.OnDevide += Clone_OnDevide;
            e.Created.OnDie += Clone_OnDie;

            CreatedSnakes.Add(e.Created);
        }

        public void Play()
        {
            Continue();
            Snakes.ForEach(snake => snake.Play());
            DrawExtra = () => { };
        }

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

                    GameField.Update(Snakes);
                    Snakes.ForEach(snake => snake.Update(GameField, Snakes));

                    foreach (DividingStudentSnake created in CreatedSnakes)
                        Snakes.Add(created);
                    CreatedSnakes.Clear();
                }

                Win.Draw(GameField);
                Snakes.ForEach(snake => snake.Draw(Win, RenderStates.Default, GameField.CellWidthPixel, GameField.CellHeightPixel));

                for(int i = 0; i < Snakes.Count; ++i)
                {
                    DividingStudentSnake snake = Snakes[i];
                    BRAIN_SHAPE.Size = new Vector2f(
                       snake.Brain.BrainWidth * GameField.CellWidthPixel,
                       snake.Brain.BrainHeight * GameField.CellHeightPixel
                    );

                    BRAIN_SHAPE.Position = new Vector2f(
                        (snake.Head.Position.X - snake.Brain.BrainWidth / 2) * GameField.CellWidthPixel,
                        (snake.Head.Position.Y - snake.Brain.BrainHeight / 2) * GameField.CellHeightPixel
                    );

                    Win.Draw(BRAIN_SHAPE);

                    Text score = new Text($"{i + 1}) size = {snake.Body.Count + 1}; energy = {snake.CurrentEnergy}; " +
                                          $"daughters = {snake.TotalCreated}; mutations = {snake.Brain.Mutations}", FONT)
                    {
                        FillColor = Color.Black,
                        CharacterSize = 24
                    };

                    score.Position = new Vector2f(GameField.Width * GameField.CellWidthPixel + 60, i * 25 + 20);
                    Win.Draw(score);

                    RectangleShape colorIndicator = new RectangleShape(new Vector2f(20, 20))
                    {
                        OutlineColor = Color.Black,
                        FillColor = snake.BodyColor
                    };

                    colorIndicator.Position = new Vector2f(GameField.Width * GameField.CellWidthPixel + 30, i * 25 + 25);
                    Win.Draw(colorIndicator);
                }

                DrawExtra();
                Win.Display();
            }
        }
    }
}
