using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SnakeBrain.SnakeGame.Snakes.Brain
{
    public class Neuron
    {
        [JsonProperty]
        private Dictionary<Direction, int> Weights { get; set; } = new Dictionary<Direction, int>()
        {
            { Direction.LEFT, 0 },
            { Direction.TOP, 0 },
            { Direction.RIGHT, 0 },
            { Direction.BOTTOM, 0 }
        };

        public Neuron() { }
        private Neuron(int left, int top, int right, int bottom)
        {
            Weights[Direction.LEFT] = left;
            Weights[Direction.TOP] = top;
            Weights[Direction.RIGHT] = right;
            Weights[Direction.BOTTOM] = bottom;
        }

        public int this[Direction direction] =>
            Weights[direction];

        public void Increase(Direction direction) =>
            ++Weights[direction];

        public void Decrease(Direction direction) =>
            --Weights[direction];

        [JsonIgnore]
        public virtual int BestWeight => Weights.Max(W => W.Value);

        [JsonIgnore]
        public virtual IEnumerable<Direction> BestDirections
        {
            get
            {
                int max = BestWeight;
                return Weights.Where(W => W.Value == max).Select(W => W.Key);
            }
        }

        public static Neuron operator +(Neuron N1, Neuron N2) =>
            new Neuron(
                N1[Direction.LEFT  ] + N2[Direction.LEFT  ],
                N1[Direction.TOP   ] + N2[Direction.TOP   ],
                N1[Direction.RIGHT ] + N2[Direction.RIGHT ],
                N1[Direction.BOTTOM] + N2[Direction.BOTTOM]
            );

        public static Neuron operator -(Neuron N) =>
            new Neuron(
                -N[Direction.LEFT ], -N[Direction.TOP   ], 
                -N[Direction.RIGHT], -N[Direction.BOTTOM]
            );

        public static Neuron operator -(Neuron N1, Neuron N2) => N1 + -N2;

        public override string ToString()
        {
            return "    " + Weights[Direction.TOP] + "\n" +
                   "  " + Weights[Direction.LEFT] + "  " + Weights[Direction.RIGHT] + "\n" +
                   "    " + Weights[Direction.BOTTOM];
        }
    }
}
