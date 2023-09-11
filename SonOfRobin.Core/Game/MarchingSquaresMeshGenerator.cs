using System;
using System.Collections.Generic;
using System.Numerics;

namespace SonOfRobin
{
    public class MarchingSquaresMeshGenerator
    {
        public readonly struct Edge
        {
            public readonly Vector2 start;
            public readonly Vector2 end;

            public Edge(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end = end;
            }
        }

        public readonly struct MarchingCell
        {
            public readonly int x;
            public readonly int y;
            public readonly int topLeft;
            public readonly int topRight;
            public readonly int bottomLeft;
            public readonly int bottomRight;
            public readonly int cornerID;
            public readonly List<Edge> edgeList;

            public MarchingCell(int x, int y, bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
            {
                this.x = x;
                this.y = y;
                this.topLeft = topLeft ? 1 : 0;
                this.topRight = topRight ? 1 : 0;
                this.bottomLeft = bottomLeft ? 1 : 0;
                this.bottomRight = bottomRight ? 1 : 0;

                this.cornerID = (this.topLeft * 1000) + (this.topRight * 100) + (this.bottomLeft * 10) + this.bottomRight;

                this.edgeList = new List<Edge>();
                foreach (Edge edge in edgesForIDs[this.cornerID])
                {
                    this.edgeList.Add(new Edge(
                        start: new Vector2(edge.start.X + this.x, edge.start.Y + this.y),
                        end: new Vector2(edge.end.X + this.x, edge.end.Y + this.y)));
                }
            }
        }

        public static void GenerateMesh(bool[,] boolArray)
        {
            int width = boolArray.GetLength(0);
            int height = boolArray.GetLength(1);

            var edgeList = new List<Edge>();

            bool xZeroFilled = false;
            for (int y = 0; y < height; y++)
            {
                if (boolArray[0, y])
                {
                    xZeroFilled = true;
                    break;
                }
            }

            bool yZeroFilled = false;
            for (int x = 0; x < width; x++)
            {
                if (boolArray[x, 0])
                {
                    yZeroFilled = true;
                    break;
                }
            }

            int startingX = xZeroFilled ? -1 : 0;
            int startingY = yZeroFilled ? -1 : 0;

            for (int x = startingX; x < width; x++)
            {
                for (int y = startingY; y < height; y++)
                {
                    bool topLeft = x >= 0 && y >= 0 && boolArray[x, y];
                    bool topRight = x + 1 < width && y >= 0 && boolArray[x + 1, y];
                    bool bottomLeft = x >= 0 && y + 1 < height && boolArray[x, y + 1];
                    bool bottomRight = x + 1 < width && y + 1 < height && boolArray[x + 1, y + 1];

                    MarchingCell marchingCell = new MarchingCell(x: x, y: y, topLeft: topLeft, topRight: topRight, bottomLeft: bottomLeft, bottomRight: bottomRight);
                    edgeList.AddRange(marchingCell.edgeList);
                }
            }

            // visualization for testing

            Console.Write("\n");

            for (int y = 0; y < height; y++)
            {
                Console.Write("[ ");
                for (int x = 0; x < width; x++)
                {
                    Console.Write(boolArray[x, y].ToString().ToLower());
                    if (x < width - 1) Console.Write(", ");
                }
                Console.Write(" ],\n");
            }

            Console.Write("\n");

            foreach (Edge edge in edgeList)
            {
                string startX = edge.start.X.ToString().Replace(",", ".");
                string startY = edge.start.Y.ToString().Replace(",", ".");
                string endX = edge.end.X.ToString().Replace(",", ".");
                string endY = edge.end.Y.ToString().Replace(",", ".");

                Console.Write("{ ");
                Console.Write($"start: new Point({startX}, {startY}), end: new Point({endX}, {endY})");
                Console.Write(" },\n");
            }
        }

        private static readonly Dictionary<int, List<Edge>> edgesForIDs = new()
        {
            // empty
            { 0000, new List<Edge>()},

            // full
            { 1111, new List<Edge>()},

            // single corner cases (filled corners)
            { 1000, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(0.0f, 0.5f)),
            }},
            { 0100, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(1.0f, 0.5f)),
            }},
            { 0010, new List<Edge> {
                new Edge(new Vector2(0.0f, 0.5f), new Vector2(0.5f, 1.0f)),
            }},
            { 0001, new List<Edge> {
                new Edge(new Vector2(0.5f, 1.0f), new Vector2(1.0f, 0.5f)),
            }},

            // single corner cases (empty corners)
            { 0111, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(0.0f, 0.5f)),
            }},
            { 1011, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(1.0f, 0.5f)),
            }},
            { 1101, new List<Edge> {
                new Edge(new Vector2(0.0f, 0.5f), new Vector2(0.5f, 1.0f)),
            }},
            { 1110, new List<Edge> {
                new Edge(new Vector2(0.5f, 1.0f), new Vector2(1.0f, 0.5f)),
            }},

            // sides

            // top
            { 1100, new List<Edge> {
                new Edge(new Vector2(0.0f, 0.5f), new Vector2(1.0f, 0.5f)),
            }},

            // bottom
            { 0011, new List<Edge> {
                new Edge(new Vector2(0.0f, 0.5f), new Vector2(1.0f, 0.5f)),
            }},

            // left
            { 1010, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(0.5f, 1.0f)),
            }},

            // right
            { 0101, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(0.5f, 1.0f)),
            }},

            // diagonals

            // right top + left bottom
            { 0110, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(0.0f, 0.5f)),
                new Edge(new Vector2(0.5f, 1.0f), new Vector2(1.0f, 0.5f)),
            }},

            // left top + right bottom
            { 1001, new List<Edge> {
                new Edge(new Vector2(0.5f, 0.0f), new Vector2(1.0f, 0.5f)),
                new Edge(new Vector2(0.0f, 0.5f), new Vector2(0.5f, 1.0f)),
            }},
        };
    }
}