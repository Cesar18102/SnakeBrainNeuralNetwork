using System;
using System.Linq;
using System.Collections.Generic;

using SFML.Graphics;

using SnakeBrain.SnakeGame.FieldCells;

namespace SnakeBrain.SnakeGame.Snakes
{
    public partial class SnakeBase
    {
        public event EventHandler<EventArgs> OnEat;
        public event EventHandler<EventArgs> OnDie;

        public int Score { get; private set; }
        public FieldCellBase Head { get; protected set; }
        public List<FieldCellBase> Body { get; protected set; } = new List<FieldCellBase>();

        public bool Dead { get; private set; }
        public bool MovementDone { get; protected set; }
        public Direction CurrentDirection { get; protected set; } = Direction.TOP;

        public Color HeadColor { get; protected set; }
        public Color BodyColor { get; protected set; }

        public SnakeBase(Color headColor, Color bodyColor, int initY, int initX, uint initSize)
        {
            HeadColor = headColor;
            BodyColor = bodyColor;

            Head = new FieldCellBase(initY, initX, HeadColor, Color.Black, 2);

            for (int i = 0; i < initSize; i++)
                Body.Add(new FieldCellBase(initY + i + 1, initX, BodyColor, Color.Black, 2));

            OnDie += (sender, e) => Dead = true;
        }

        protected SnakeBase(Color headColor, Color bodyColor, Point head, IEnumerable<Point> body)
        {
            HeadColor = headColor;
            BodyColor = bodyColor;

            Head = new FieldCellBase(head.Y, head.X, HeadColor, Color.Black, 2);

            foreach(Point bodySection in body)
                Body.Add(new FieldCellBase(bodySection.Y, bodySection.X, BodyColor, Color.Black, 2));

            OnDie += (sender, e) => Dead = true;
        }

        public void MoveHeadTo(int y, int x)
        {
            int dy = y - Head.Position.Y;
            int dx = x - Head.Position.X;

            Head.Position.MoveTo(y, x);

            foreach (FieldCellBase bodySection in Body)
                bodySection.Position.MoveTo(
                    bodySection.Position.Y + dy, 
                    bodySection.Position.X + dx
                );
        }

        protected void Die() => OnDie?.Invoke(this, new EventArgs());

        public SnakeBase(SnakeBase snake)
        {
            HeadColor = snake.HeadColor;
            BodyColor = snake.BodyColor;

            Head = new FieldCellBase(snake.Head.Position.Y, snake.Head.Position.X, HeadColor, Color.Black, 2);

            for (int i = 0; i < snake.Body.Count; i++)
                Body.Add(new FieldCellBase(snake.Body[i].Position.Y, snake.Body[i].Position.X, BodyColor, Color.Black, 2));

            OnDie += (sender, e) => Dead = true;
        }

        public bool ContainsCell(FieldCellBase cell) =>
            CellInBody(cell) || Head.Position.Equals(cell.Position);

        public bool CellInBody(FieldCellBase cell) =>
            Body.Exists(B => B.Position.Equals(cell.Position));

        public virtual bool ProcessCollisionWith(Field gameField, FieldCellBase cell, IEnumerable<SnakeBase> otherSnakes) 
        {
            if (cell is FieldCellWall)
            {
                OnDie?.Invoke(this, new EventArgs());
                return false;
            }

            if(cell is FieldCellFood)
            {
                Grow();
                ++Score;
                OnEat(this, new EventArgs());
                FieldCellEmpty empty = new FieldCellEmpty(cell.Position.Y, cell.Position.X);
                gameField[cell.Position.Y, cell.Position.X] = empty;
                return true;
            }

            if(otherSnakes.Count(S => S.ContainsCell(cell)) != 0 || CellInBody(cell))
            {
                OnDie?.Invoke(this, new EventArgs());
                return false;
            }

            return true;
        }

        public void Move(Direction direction)
        {
            if (!MovementDone && Math.Abs(CurrentDirection - direction) != 2)
            {
                CurrentDirection = direction;
                MovementDone = true;
            }
        }

        public void Grow()
        {
            int y = 0;
            int x = 0;

            if(Body.Count == 0) 
            {
                x = Head.Position.X - (CurrentDirection == Direction.LEFT ? -1 : (CurrentDirection == Direction.RIGHT ? 1 : 0));
                y = Head.Position.Y - (CurrentDirection == Direction.TOP ? -1 : (CurrentDirection == Direction.BOTTOM ? 1 : 0));
            }
            else if(Body.Count == 1)
            {
                FieldCellBase neck = Body.First();

                x = neck.Position.X - (Head.Position.X - neck.Position.X);
                y = neck.Position.Y - (Head.Position.Y - neck.Position.Y);
            }
            else
            {
                FieldCellBase tail = Body[Body.Count - 1];
                FieldCellBase preTail = Body[Body.Count - 2];

                x = tail.Position.X - (preTail.Position.X - tail.Position.X);
                y = tail.Position.Y - (preTail.Position.Y - tail.Position.Y);
            }

            Body.Add(new FieldCellBase(y, x, BodyColor, Color.Black, 2));
        }

        public virtual void Update(Field gameField, IEnumerable<SnakeBase> otherSnakes)
        {
            int dx = CurrentDirection == Direction.LEFT ? -1 : (CurrentDirection == Direction.RIGHT ? 1 : 0);
            int dy = CurrentDirection == Direction.TOP ? -1 : (CurrentDirection == Direction.BOTTOM ? 1 : 0);

            int oldX = Head.Position.X;
            int oldY = Head.Position.Y;

            Head.Position.MoveTo(Head.Position.Y + dy, Head.Position.X + dx);
            Head.FitToSize(gameField.Width, gameField.Height);

            if (!ProcessCollisionWith(gameField, gameField[Head.Position.Y, Head.Position.X], otherSnakes.Where(S => S != this)))
            {
                Head.Position.MoveTo(oldY, oldX);
                Head.FitToSize(gameField.Width, gameField.Height);
                return;
            }

            if (Body.Count > 0)
            {
                Body.RemoveAt(Body.Count - 1);
                Body.Insert(0, new FieldCellBase(oldY, oldX, BodyColor, Color.Black, 2));
                Body[0].FitToSize(gameField.Width, gameField.Height);
            }
        }

        public void Draw(RenderTarget target, RenderStates states, float width, float height)
        {
            Head.Draw(target, states, width, height);
            Body.ForEach(B => B.Draw(target, states, width, height));

            MovementDone = false;
        }
    }
}
