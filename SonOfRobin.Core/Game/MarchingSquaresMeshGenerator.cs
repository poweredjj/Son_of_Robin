using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using static SonOfRobin.MarchingSquaresMeshGenerator;

namespace SonOfRobin
{
    public readonly struct MarchingCell
    {
        public readonly Vector2 pos;
        public readonly int topLeft;
        public readonly int topRight;
        public readonly int bottomLeft;
        public readonly int bottomRight;
        public readonly int cornerID;
        public readonly HashSet<Edge> edgeSet;

        public MarchingCell(Vector2 pos, bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            this.pos = pos;
            this.topLeft = topLeft ? 1 : 0;
            this.topRight = topRight ? 1 : 0;
            this.bottomLeft = bottomLeft ? 1 : 0;
            this.bottomRight = bottomRight ? 1 : 0;

            this.cornerID = (this.topLeft * 1000) + (this.topRight * 100) + (this.bottomLeft * 10) + this.bottomRight;

            this.edgeSet = new HashSet<Edge>();
            foreach (Edge edge in edgesForIDs[this.cornerID])
            {
                this.edgeSet.Add(new Edge(start: this.pos + edge.start, end: this.pos + edge.end));
            }
        }
    }

    public class MarchingSquaresMeshGenerator
    {
        public readonly struct Edge
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public readonly double angle;

            public Edge(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end = end;
                this.angle = GetAngleDegrees(start: start, end: end);
            }

            private static double GetAngleDegrees(Vector2 start, Vector2 end)
            {
                Vector2 delta = end - start;
                double angleInRadians = Math.Atan2(delta.Y, delta.X);
                return Math.Round(angleInRadians * 180 / Math.PI);
            }

            public static Edge ReversedEdge(Edge edge)
            {
                return new Edge(start: edge.end, end: edge.start);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17; // Prime number to start with

                    // Ensure that start and end vectors are in a consistent order
                    var orderedStart = Vector2.Min(start, end);
                    var orderedEnd = Vector2.Max(start, end);

                    hash = hash * 23 + orderedStart.GetHashCode();
                    hash = hash * 23 + orderedEnd.GetHashCode();

                    return hash;
                }
            }

        }

        public static List<Edge> GenerateConnectedEdgesList(bool[,] boolArray)
        {
            int width = boolArray.GetLength(0);
            int height = boolArray.GetLength(1);

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

            var marchingCellBag = new ConcurrentBag<MarchingCell>();
            Parallel.For(startingX, width, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, x =>
            {
                for (int y = startingY; y < height; y++)
                {
                    bool topLeft = x >= 0 && y >= 0 && boolArray[x, y];
                    bool topRight = x + 1 < width && y >= 0 && boolArray[x + 1, y];
                    bool bottomLeft = x >= 0 && y + 1 < height && boolArray[x, y + 1];
                    bool bottomRight = x + 1 < width && y + 1 < height && boolArray[x + 1, y + 1];

                    marchingCellBag.Add(new(pos: new Vector2(x + 0.5f, y + 0.5f), topLeft: topLeft, topRight: topRight, bottomLeft: bottomLeft, bottomRight: bottomRight));
                }
            });

            var edgeSet = new HashSet<Edge>();
            foreach (MarchingCell marchingCell in marchingCellBag)
            {
                edgeSet.UnionWith(marchingCell.edgeSet);
            }

            //return edgeSet.ToList(); // for testing

            List<Edge> connectedEdgesList = OrderAndMergeEdges(edgeSet);

            return connectedEdgesList;
        }

        public static List<Edge> OrderAndMergeEdges(HashSet<Edge> edges)
        {
            var edgesToSort = edges.ToList();
            var sortedEdgesList = new List<Edge>();

            Edge currentEdge = edgesToSort[0];
            edgesToSort.RemoveAt(0);

            while (true)
            {
                bool connectionFound = false;

                for (int i = 0; i < edgesToSort.Count; i++)
                {
                    Edge nextEdge = edgesToSort[i];
                    if (nextEdge.start == currentEdge.end || nextEdge.end == currentEdge.end)
                    {
                        if (nextEdge.end == currentEdge.end) nextEdge = Edge.ReversedEdge(nextEdge);

                        connectionFound = true;
                        edgesToSort.RemoveAt(i);

                        if (currentEdge.angle == nextEdge.angle)
                        {
                            nextEdge = new Edge(start: currentEdge.start, end: nextEdge.end);
                            sortedEdgesList.Remove(currentEdge);
                        }
                        sortedEdgesList.Add(nextEdge);

                        currentEdge = nextEdge;
                        break;
                    }
                }
                if (!connectionFound && edgesToSort.Count > 0)
                {
                    // next subpath
                    currentEdge = edgesToSort[0];
                    sortedEdgesList.Add(currentEdge);
                    edgesToSort.RemoveAt(0);
                }

                if (edgesToSort.Count == 0) break;
            }

            return sortedEdgesList;
        }

        public static readonly Dictionary<int, List<Edge>> edgesForIDs = new()
        {
            // empty
            { 0000, new List<Edge>()},

            // full
            { 1111, new List<Edge>()},

            // single corner cases (filled corners)
            { 1000, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.0f, 0.5f)),
            }},
            { 0100, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(1.0f, 0.5f)),
            }},
            { 0010, new List<Edge> {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},
            { 0001, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 1.0f), end: new Vector2(1.0f, 0.5f)),
            }},

            // single corner cases (empty corners)
            { 0111, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.0f, 0.5f)),
            }},
            { 1011, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(1.0f, 0.5f)),
            }},
            { 1101, new List<Edge> {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},
            { 1110, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 1.0f), end: new Vector2(1.0f, 0.5f)),
            }},

            // sides

            // top
            { 1100, new List<Edge> {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(1.0f, 0.5f)),
            }},
            // bottom
            { 0011, new List<Edge> {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(1.0f, 0.5f)),
            }},
            // left
            { 1010, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.5f, 1.0f)),
            }},
            // right
            { 0101, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.5f, 1.0f)),
            }},

            // diagonals

            // right top + left bottom
            { 0110, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.0f, 0.5f)),
                new Edge(start: new Vector2(0.5f, 1.0f), end: new Vector2(1.0f, 0.5f)),
            }},

            // left top + right bottom
            { 1001, new List<Edge> {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(1.0f, 0.5f)),
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},
        };
    }
}