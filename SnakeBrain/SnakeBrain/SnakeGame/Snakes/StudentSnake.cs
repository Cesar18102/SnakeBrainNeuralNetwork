using System;
using System.Collections.Generic;

using SFML.Graphics;

using SnakeBrain.SnakeGame.FieldCells;
using SnakeBrain.SnakeGame.Snakes.Brain;
using SnakeBrain.SnakeGame.Snakes.SnakeEventArgs;

namespace SnakeBrain.SnakeGame.Snakes
{
    public class StudentSnake : SnakeBase
    {
        private Random R = new Random();
        public event EventHandler<PredictWaitEventArgs> OnPredictControlPauseNeeded;
        public event EventHandler<EventArgs> OnContinueGame;

        public bool Studying { get; private set; }
        public BrainBase Brain { get; private set; }

        public StudentSnake(int initY, int initX, uint initSize, 
                            Color headColor, Color bodyColor,
                            int brainHeight, int brainWidth) : 
            base(initY, initX, initSize, headColor, bodyColor)
        {
            Studying = true;
            Brain = new BrainBase(brainHeight, brainWidth);
        }

        public void LoadBrainFromFile(string filename)
        {
            Studying = false;
            Brain = BrainBase.LoadFromFile(filename);
        }

        public void Play() => Studying = false;
        public void Study() => Studying = true;

        public override void Update(Field gameField, List<SnakeBase> otherSnakes)
        {
            List<Point> wallTriggers = new List<Point>();
            List<Point> foodTriggers = new List<Point>();

            int brainOffsetY = Brain.BrainHeight / 2;
            int yStart = Head.Position.Y - brainOffsetY; //Math.Min(Math.Max(Head.Position.Y - brainOffsetY, 0), gameField.Height - 1);
            int yEnd = Head.Position.Y + brainOffsetY; //Math.Min(Math.Max(Head.Position.Y + brainOffsetY, 0), gameField.Height - 1);

            int brainOffsetX = Brain.BrainWidth / 2;
            int xStart = Head.Position.X - brainOffsetX; //Math.Min(Math.Max(Head.Position.X - brainOffsetX, 0), gameField.Width - 1);
            int xEnd = Head.Position.X + brainOffsetX; //Math.Min(Math.Max(Head.Position.X + brainOffsetX, 0), gameField.Width - 1);

            for (int i = yStart; i <= yEnd; i++)
                if (i >= 0 && i < gameField.Height)
                    for (int j = xStart; j <= xEnd; j++)
                    {
                        if (j < 0 || j >= gameField.Width)
                            continue;

                        if (gameField[i, j] is FieldCellWall || CellInBody(gameField[i, j]) || otherSnakes.Exists(S => S.ContainsCell(gameField[i, j])))
                            wallTriggers.Add(new Point(i - yStart, j - xStart));
                        else if (gameField[i, j] is FieldCellFood)
                            foodTriggers.Add(new Point(i - yStart, j - xStart));
                    }

            Direction decision = Brain.GetDecision(wallTriggers, foodTriggers);

            if (Studying)
            {
                PredictWaitEventArgs decisionArgs = new PredictWaitEventArgs(decision, gameField, otherSnakes);
                decisionArgs.OnTeacherAnswered += DecisionArgs_OnTeacherAnswered;
                OnPredictControlPauseNeeded?.Invoke(this, decisionArgs);
            }
            else
            {
                Move(decision);
                base.Update(gameField, otherSnakes);
            }
        }

        private void DecisionArgs_OnTeacherAnswered(object sender, TeacherAnswerEventArgs e)
        {
            if (e.Prediciton.WantedDirection == e.TeacherDecision)
                Brain.AcceptDecision();
            else
                Brain.RefuseDecision(e.TeacherDecision);

            Move(e.TeacherDecision);
            base.Update(e.Prediciton.GameField, e.Prediciton.OtherSnakes);

            OnContinueGame?.Invoke(this, new EventArgs());
        }
    }
}
