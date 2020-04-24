using System;
using System.Linq;
using System.Collections.Generic;
using SnakeBrain.SnakeGame.Snakes.SnakeEventArgs;
using SnakeBrain.SnakeGame.Snakes.Brain;
using SFML.Graphics;

namespace SnakeBrain.SnakeGame.Snakes
{
    public class DividingStudentSnake : StudentSnake
    {
        public int CurrentEnergy { get; private set; }
        public int EnergyToCreate { get; private set; }
        public int EnergyToDie { get; private set; }
        public int EnergyPerFood { get; private set; }
        public int TotalCreated { get; private set; }

        public event EventHandler<SnakeCreatedEventArgs> OnDevide;

        public DividingStudentSnake(StudentSnake student, int initEnegry, int energyToCreate, 
                                    int energyToDie, int energyPerFood) : base(student)
        {
            CurrentEnergy = initEnegry;
            EnergyToCreate = energyToCreate;
            EnergyToDie = energyToDie;
            EnergyPerFood = energyPerFood;

            OnEat += DividingStudentSnake_OnEat;
        }

        private DividingStudentSnake(Color headColor, Point head, IEnumerable<Point> body, BrainBase brain, int initEnegry, int energyToCreate, 
                                     int energyToDie, int energyPerFood) : base(headColor, GetRandomColor(), head, body, brain)
        {
            CurrentEnergy = initEnegry;
            EnergyToCreate = energyToCreate;
            EnergyToDie = energyToDie;
            EnergyPerFood = energyPerFood;

            OnEat += DividingStudentSnake_OnEat;
        }

        private static Color GetRandomColor()
        {
            Random rand = new Random();
            return new Color((byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256));
        }

        private void DividingStudentSnake_OnEat(object sender, EventArgs e) =>
            CurrentEnergy += EnergyPerFood;

        public override void Update(Field gameField, IEnumerable<SnakeBase> otherSnakes)
        {
            --CurrentEnergy;
            base.Update(gameField, otherSnakes);

            if (CurrentEnergy <= EnergyToDie)
            {
                base.Die();
                return;
            }

            if (CurrentEnergy >= EnergyToCreate)
            {
                int size = Body.Count / 2;
                List<Point> newBodyPositions = Body.Skip(size).Select(bs => bs.Position).ToList();

                Body.RemoveRange(size, Body.Count - size);
                BrainBase newBrain = new BrainBase(Brain);

                Random random = new Random();
                int mutations = random.Next(1, 5);

                for(int i = 0; i < mutations; ++i)
                    newBrain.RandomMutation();

                newBodyPositions.Reverse();
                DividingStudentSnake newSnake = new DividingStudentSnake(
                    HeadColor, newBodyPositions.First(), newBodyPositions.Skip(1), newBrain, 
                    CurrentEnergy / 2, EnergyToCreate, EnergyToDie, EnergyPerFood
                );

                ++TotalCreated;
                CurrentEnergy /= 2;
                OnDevide?.Invoke(this, new SnakeCreatedEventArgs(newSnake));
            }
        }
    }
}
