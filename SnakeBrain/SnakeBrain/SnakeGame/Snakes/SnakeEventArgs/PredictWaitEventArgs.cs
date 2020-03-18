using System;
using System.Collections.Generic;

namespace SnakeBrain.SnakeGame.Snakes.SnakeEventArgs
{
    public class PredictWaitEventArgs
    {
        public event EventHandler<TeacherAnswerEventArgs> OnTeacherAnswered;
        public Direction WantedDirection { get; private set; }

        public Field GameField { get; private set; }
        public List<SnakeBase> OtherSnakes { get; private set; }

        public PredictWaitEventArgs(Direction wantedDirection, Field gameField, List<SnakeBase> otherSnakes)
        {
            WantedDirection = wantedDirection;
            GameField = gameField;
            OtherSnakes = otherSnakes;
        }

        public void Answer(Direction direction) =>
            OnTeacherAnswered?.Invoke(this, new TeacherAnswerEventArgs(direction, this));
    }
}
