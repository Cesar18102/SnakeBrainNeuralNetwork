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

        [JsonProperty]
        private Neuron[,] Neurons { get; set; }

        public Neuron this[int y, int x] =>
            Neurons[y, x];

        public Neuron this[Point point] =>
            this[point.Y, point.X];

        private List<Point> LastWallTriggers { get; set; } = new List<Point>();
        private List<Point> LastFoodTriggers { get; set; } = new List<Point>();
        private Direction LastDecision { get; set; }

        private delegate void NeuronAffector(Neuron neuron, Direction direction);
        private void AffectAllNeuronsAt(Direction direction, NeuronAffector affector)
        {
            foreach (Point wallTrigger in LastWallTriggers)
                affector(Neurons[wallTrigger.Y, wallTrigger.X], direction);

            foreach (Point foodTrigger in LastFoodTriggers)
                affector(Neurons[foodTrigger.Y, foodTrigger.X], direction);
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
                result += this[wallTriggerCoord];

            foreach (Point foodTriggerCoord in foodTriggers)
                result += this[foodTriggerCoord];

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

        public BrainBase(int brainHeight, int brainWidth)
        {
            BrainHeight = brainHeight;
            BrainWidth = brainWidth;

            Neurons = new Neuron[BrainHeight, BrainWidth];

            for (int i = 0; i < BrainHeight; ++i)
                for (int j = 0; j < BrainWidth; ++j)
                    Neurons[i, j] = new Neuron();
        }
    }
}
