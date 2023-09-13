using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SonOfRobin
{
    public class MarchingSquaresMeshGenerator
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

        public class Shape
        {
            public readonly List<Vector2> triangleVertices;
            public readonly List<int> triangeIndices;
            public readonly List<Edge> edges;
            public bool isHole;

            public Shape()
            {
                this.edges = new List<Edge>();
                this.isHole = false;
                this.triangleVertices = new List<Vector2>();
                this.triangeIndices = new List<int>();
            }

            public Shape(List<Edge> edges)
            {
                this.edges = edges;
                this.isHole = false;
                this.triangleVertices = new List<Vector2>();
                this.triangeIndices = new List<int>();
            }
        }

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

        public static List<Shape> GenerateConnectedEdgesList(bool[,] boolArray)
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

            int startX = xZeroFilled ? -1 : 0;
            int startY = yZeroFilled ? -1 : 0;

            var neighbourArray = new bool[2, 2];

            var marchingCellList = new List<MarchingCell>();

            for (int x = startX; x < width; x++)
            {
                for (int y = startY; y < height; y++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            neighbourArray[i, j] = x + i >= 0 && x + i < width && y + j >= 0 && y + j < height && boolArray[x + i, y + j];
                        }
                    }
                    marchingCellList.Add(new MarchingCell(pos: new Vector2(x + 0.5f, y + 0.5f), topLeft: neighbourArray[0, 0], topRight: neighbourArray[1, 0], bottomLeft: neighbourArray[0, 1], bottomRight: neighbourArray[1, 1]));
                }
            }

            var edgeSet = new HashSet<Edge>();
            foreach (MarchingCell marchingCell in marchingCellList)
            {
                edgeSet.UnionWith(marchingCell.edgeSet);
            }

            // return edgeSet.ToList(); // for testing

            return OrderAndMergeEdges(edgeSet);
        }

        public static List<Shape> OrderAndMergeEdges(HashSet<Edge> edges)
        {
            var edgesToSort = edges.ToList();
            var shapeList = new List<Shape>();

            var currentShape = new Shape();
            shapeList.Add(currentShape);

            Edge currentEdge = edgesToSort[0];
            edgesToSort.RemoveAt(0);
            currentShape.edges.Add(currentEdge);

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
                            currentShape.edges.Remove(currentEdge);
                        }
                        currentShape.edges.Add(nextEdge);

                        currentEdge = nextEdge;
                        break;
                    }
                }
                if (!connectionFound && edgesToSort.Count > 0)
                {
                    // next subpath
                    currentShape = new Shape();
                    shapeList.Add(currentShape);

                    currentEdge = edgesToSort[0];
                    currentShape.edges.Add(currentEdge);
                    edgesToSort.RemoveAt(0);
                }

                if (edgesToSort.Count == 0) break;
            }

            return shapeList;
        }

        public static Dictionary<Shape, List<Shape>> GroupShapes(List<Shape> shapes)
        {
            var shapeGroups = new Dictionary<Shape, List<Shape>>();

            foreach (Shape shape in shapes)
            {
                shapeGroups[shape] = FindHoles(shape, shapes);
            }

            foreach (Shape shape in shapeGroups.Keys.ToList())
            {
                if (shape.isHole) shapeGroups.Remove(shape);
            }

            return shapeGroups;
        }

        public static List<Shape> FindHoles(Shape outer, List<Shape> shapes)
        {
            var holes = new List<Shape>();

            foreach (var shape in shapes)
            {
                if (shape == outer) continue;

                if (IsPointInPolygon(shape.edges[0].start, outer.edges))
                {
                    shape.isHole = true;
                    holes.Add(shape);
                }
            }

            return holes;
        }

        public static bool IsPointInPolygon(Vector2 point, List<Edge> edges)
        {
            bool inside = false;

            for (int i = 0, j = edges.Count - 1; i < edges.Count; j = i++)
            {
                if (((edges[i].start.Y <= point.Y && point.Y < edges[j].start.Y) ||
                    (edges[j].start.Y <= point.Y && point.Y < edges[i].start.Y)) &&
                    (point.X < (edges[j].start.X - edges[i].start.X) * (point.Y - edges[i].start.Y) / (edges[j].start.Y - edges[i].start.Y) + edges[i].start.X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static readonly Dictionary<int, List<Edge>> edgesForIDs = new Dictionary<int, List<Edge>>
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