using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SnakeBrain.SnakeGame.Snakes.Brain
{
    public class BrainBase
    {
        private Random R = new Random();

        public int BrainHeight { get; private set; }
        public int BrainWidth { get; private set; }

        public int Mutations { get; private set; }

        [JsonProperty]
        private Neuron[,] WallNeurons { get; set; }

        [JsonProperty]
        private Neuron[,] FoodNeurons { get; set; }

        public (Neuron WallNeuron, Neuron FoodNeuron) this[int y, int x] =>
            (WallNeurons[y, x], FoodNeurons[y, x]);

        public (Neuron WallNeuron, Neuron FoodNeuron) this[Point point] =>
            this[point.Y, point.X];

        private List<Point> LastWallTriggers { get; set; } = new List<Point>();
        private List<Point> LastFoodTriggers { get; set; } = new List<Point>();
        private Direction LastDecision { get; set; }

        public bool IsWallTriggered(Point point) =>
            LastWallTriggers.Contains(point);

        public bool IsFoodTriggered(Point point) =>
            LastFoodTriggers.Contains(point);

        private delegate void NeuronAffector(Neuron neuron, Direction direction);
        private void AffectAllNeuronsAt(Direction direction, NeuronAffector affector)
        {
            foreach (Point wallTrigger in LastWallTriggers)
                affector(WallNeurons[wallTrigger.Y, wallTrigger.X], direction);

            foreach (Point foodTrigger in LastFoodTriggers)
                affector(FoodNeurons[foodTrigger.Y, foodTrigger.X], direction);
        }

        public void AcceptDecision() { }

        public void RefuseDecision(Direction correctDirection)
        {
            AffectAllNeuronsAt(LastDecision, (neuron, direction) => neuron.Decrease(direction));
            AffectAllNeuronsAt(correctDirection, (neuron, direction) => neuron.Increase(direction));
        }

        public virtual Direction GetDecision(List<Point> wallTriggers, List<Point> foodTriggers)
        {
            LastWallTriggers = wallTriggers;
            LastFoodTriggers = foodTriggers;

            Neuron result = new Neuron();

            foreach (Point wallTriggerCoord in wallTriggers)
                result += WallNeurons[wallTriggerCoord.Y, wallTriggerCoord.X];

            foreach (Point foodTriggerCoord in foodTriggers)
                result += FoodNeurons[foodTriggerCoord.Y, foodTriggerCoord.X];

            List<Direction> bestDirections = result.BestDirections.ToList();
            LastDecision = bestDirections[R.Next(0, bestDirections.Count - 1)];

            return LastDecision;
        }

        public void Save(string filename)
        {
            using (StreamWriter strw = File.CreateText(filename))
                strw.Write(JsonConvert.SerializeObject(this));
        }

        public static BrainBase LoadFromFile(string filename)
        {
            using (StreamReader str = new StreamReader(filename))
                return JsonConvert.DeserializeObject<BrainBase>(str.ReadToEnd());
        }

        [JsonConstructor]
        public BrainBase(int brainHeight, int brainWidth)
        {
            Init(brainHeight, brainWidth);

            for (int i = 0; i < BrainHeight; ++i)
                for (int j = 0; j < BrainWidth; ++j)
                {
                    WallNeurons[i, j] = new Neuron();
                    FoodNeurons[i, j] = new Neuron();
                }
        }

        public BrainBase(BrainBase brain)
        {
            Init(brain.BrainHeight, brain.BrainWidth);

            for (int i = 0; i < BrainHeight; ++i)
                for (int j = 0; j < BrainWidth; ++j)
                {
                    WallNeurons[i, j] = new Neuron(brain.WallNeurons[i, j]);
                    FoodNeurons[i, j] = new Neuron(brain.FoodNeurons[i, j]);
                }

            Mutations = brain.Mutations;
        }

        public void RandomMutation()
        {
            int x = R.Next(0, BrainWidth);
            int y = R.Next(0, BrainHeight);

            Neuron mutated = R.NextDouble() > 0.5 ? WallNeurons[y, x] : FoodNeurons[y, x];
            Direction mutatedDirection = (Direction)R.Next((int)Direction.LEFT, (int)Direction.BOTTOM + 1);

            if (R.NextDouble() > 0.5)
                mutated.Increase(mutatedDirection);
            else
                mutated.Decrease(mutatedDirection);

            ++Mutations;
        }

        private void Init(int height, int width)
        {
            BrainHeight = height;
            BrainWidth = width;

            WallNeurons = new Neuron[BrainHeight, BrainWidth];
            FoodNeurons = new Neuron[BrainHeight, BrainWidth];
        }
    }
}
