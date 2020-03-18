using System;

namespace SnakeBrain.SnakeGame.Snakes.SnakeEventArgs
{
    public class TeacherAnswerEventArgs : EventArgs
    {
        public Direction TeacherDecision { get; private set; }
        public PredictWaitEventArgs Prediciton { get; private set; }

        public TeacherAnswerEventArgs(Direction teacherDecision, PredictWaitEventArgs prediction)
        {
            Prediciton = prediction;
            TeacherDecision = teacherDecision;
        }
    }
}
