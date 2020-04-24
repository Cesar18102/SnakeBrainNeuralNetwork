using System;

namespace SnakeBrain.SnakeGame.Snakes.SnakeEventArgs
{
    public class SnakeCreatedEventArgs : EventArgs
    {
        public DividingStudentSnake Created { get; private set; }
        public SnakeCreatedEventArgs(DividingStudentSnake created) => Created = created;
    }
}
