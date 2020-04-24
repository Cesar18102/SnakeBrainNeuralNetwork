using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using SFML.System;
using SFML.Window;
using SFML.Graphics;

using SnakeBrain.SnakeGame;
using System.Collections;

namespace SnakeBrain
{
    public class Program
    {        
        public class Result : IComparable<Result>
        {
            public int Size { get; set; }

            public int CompareTo(Result other) =>
                Size - other.Size;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            /*Test t = new Test();
            foreach(int i in t)
            {
                Console.WriteLine(i);
            }*/

            Game game = new Game(100, 100, 10, 10, "test", 60, Color.White);
            game.Start();

            //string[] files = Enumerable.Range(1, 100)
            //                           .Select(path => Environment.CurrentDirectory + "/Probes2/" + path + ".bot")
            //                           .ToArray();

            //Dictionary<string, Result> results = new Dictionary<string, Result>();
            //Play(files, 0, 3, results);
            //DrawStatistics(results);
        }

        /*public static void DrawStatistics(Dictionary<string, Result> results)
        {
            int max = results.Max(res => res.Value).Size;
            List<string> bestConfigs = results.Where(res => res.Value.Size == max)
                                              .Select(res => res.Key)
                                              .ToList();

            Console.WriteLine("\n***BEST CONFIGS***\n\n");
            Console.WriteLine(string.Join("\n", bestConfigs));

            RenderWindow window = new RenderWindow(VideoMode.FullscreenModes[0], "result");

            float delta = VideoMode.FullscreenModes[0].Width / results.Count;
            CircleShape point = new CircleShape(5) { FillColor = Color.Red };

            List<CircleShape> points = results.Skip(1).Select((result, i) =>
            {
                CircleShape pnt = new CircleShape(point);
                pnt.Position = new Vector2f(i * delta, VideoMode.FullscreenModes[0].Height * (1 - (float)results.ElementAt(i).Value.Size / max));
                return pnt;
            }).ToList();

            CircleShape firstPoint = new CircleShape(point);
            firstPoint.Position = new Vector2f(0, VideoMode.FullscreenModes[0].Height * (1 - (float)results.ElementAt(0).Value.Size / max));
            points.Prepend(firstPoint);

            List<Vertex> lines = points.Select(pnt => new Vertex(pnt.Position, Color.Red)).ToList();

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(Color.Black);

                points.ForEach(pnt => window.Draw(pnt));
                window.Draw(lines.ToArray(), PrimitiveType.LineStrip);
                window.Display();
            }
        }

        public static void Play(string[] files, int fileId, int roundsMax, Dictionary<string, Result> results)
        {
            if (fileId >= files.Length || results.ContainsKey(files[fileId]))
                return;

            Result result = new Result();
            results.Add(files[fileId], result);

            Play(files[fileId], roundsMax, result, (sender, e) => Play(files, ++fileId, roundsMax, results));
        }

        public static void Play(string file, int roundsLeft, Result result, EventHandler<EventArgs> snakeDeathEventHandler)
        {
            if (roundsLeft == 0)
                return;

            Game game = new Game(30, 30, 30, 30, "test - " + file, 1440, Color.White);
            game.Snake.LoadBrainFromFile(file);
            game.Play();

            game.Snake.OnDie += (sender, e) =>
            {
                result.Size += game.Snake.Body.Count;
                game.Close();
                Play(file, --roundsLeft, result, snakeDeathEventHandler);
            };

            game.Snake.OnDie += snakeDeathEventHandler;

            game.Start();
        }*/
    }
}
