using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BitmapToShapesConverter
    {
        public class Shape
        {
            public readonly List<Vector2> pointList;
            public bool isHole;

            public Shape()
            {
                this.pointList = new List<Vector2>();
                this.isHole = false;
            }
        }

        public readonly struct Edge
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public readonly float angleRadians;

            public Edge(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end = end;
                this.angleRadians = GetAngleRadians(start: start, end: end);
            }

            private static float GetAngleRadians(Vector2 start, Vector2 end)
            {
                Vector2 delta = end - start;
                return (float)Math.Atan2(delta.Y, delta.X);
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

        public static Dictionary<Shape, List<Shape>> GenerateShapes(BitArrayWrapperChunk chunk)
        {
            var shapeList = GenerateConnectedEdgesList(chunk);
            return GroupShapes(shapeList);
        }

        private static List<Shape> GenerateConnectedEdgesList(BitArrayWrapperChunk chunk)
        {
            int width = chunk.width;
            int height = chunk.height;

            bool xZeroFilled = false;
            for (int y = 0; y < height; y++)
            {
                if (chunk.GetVal(0, y))
                {
                    xZeroFilled = true;
                    break;
                }
            }

            bool yZeroFilled = false;
            for (int x = 0; x < width; x++)
            {
                if (chunk.GetVal(x, 0))
                {
                    yZeroFilled = true;
                    break;
                }
            }

            int startX = xZeroFilled ? -1 : 0;
            int startY = yZeroFilled ? -1 : 0;

            var neighbourArray = new int[2, 2];

            var edgeSet = new HashSet<Edge>();
            Vector2 currentPos = Vector2.Zero;

            for (int x = startX; x < width; x++)
            {
                currentPos.X = x + 0.5f;

                for (int y = startY; y < height; y++)
                {
                    currentPos.Y = y + 0.5f;

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            neighbourArray[i, j] = x + i >= 0 && x + i < width && y + j >= 0 && y + j < height && chunk.GetVal(x + i, y + j) ? 1 : 0;
                        }
                    }

                    CalculateMarchingCellAndAddEdgesToSet(edgeSet: edgeSet, pos: currentPos, neighbourArray: neighbourArray);
                }
            }

            return OrderAndMergeEdges(edgeSet);
        }

        public static void CalculateMarchingCellAndAddEdgesToSet(HashSet<Edge> edgeSet, Vector2 pos, int[,] neighbourArray)
        {
            foreach (Edge edge in edgesForIDs[(neighbourArray[0, 0] * 1000) + (neighbourArray[1, 0] * 100) + (neighbourArray[0, 1] * 10) + neighbourArray[1, 1]])
            {
                edgeSet.Add(new Edge(start: pos + edge.start, end: pos + edge.end));
            }
        }

        public static List<Shape> OrderAndMergeEdges(HashSet<Edge> edges)
        {
            var edgesToSort = new HashSet<Edge>(edges);

            var edgesByPosDict = new Dictionary<Vector2, List<Edge>>();
            foreach (Edge edge in edges)
            {
                if (!edgesByPosDict.ContainsKey(edge.start)) edgesByPosDict[edge.start] = new List<Edge>();
                if (!edgesByPosDict.ContainsKey(edge.end)) edgesByPosDict[edge.end] = new List<Edge>();

                edgesByPosDict[edge.start].Add(edge);
                edgesByPosDict[edge.end].Add(edge);
            }

            var shapeList = new List<Shape>();

            if (edgesToSort.Count == 0) return shapeList;

            var currentShape = new Shape();
            shapeList.Add(currentShape);

            Edge currentEdge = edgesToSort.First();
            edgesToSort.Remove(currentEdge);
            currentShape.pointList.Add(currentEdge.start);

            while (true)
            {
                bool connectionFound = false;

                if (edgesByPosDict.ContainsKey(currentEdge.end))
                {
                    foreach (Edge checkedEdge in edgesByPosDict[currentEdge.end])
                    {
                        if (edgesToSort.Contains(checkedEdge))
                        {
                            connectionFound = true;
                            Edge nextEdge = checkedEdge;
                            edgesToSort.Remove(nextEdge);

                            if (nextEdge.end == currentEdge.end) nextEdge = Edge.ReversedEdge(nextEdge);

                            if (currentEdge.angleRadians == nextEdge.angleRadians)
                            {
                                nextEdge = new Edge(start: currentEdge.start, end: nextEdge.end);
                                currentShape.pointList.Remove(currentEdge.end);
                            }
                            currentShape.pointList.Add(nextEdge.end);

                            currentEdge = nextEdge;
                            break;
                        }
                    }
                }

                if (!connectionFound && edgesToSort.Count > 0)
                {
                    // next subpath
                    currentShape = new Shape();
                    shapeList.Add(currentShape);

                    currentEdge = edgesToSort.First();
                    currentShape.pointList.Add(currentEdge.start);
                    edgesToSort.Remove(currentEdge);
                }

                if (edgesToSort.Count == 0) break;
            }

            return shapeList;
        }

        private static Dictionary<Shape, List<Shape>> GroupShapes(List<Shape> shapes)
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

                if (IsPointInPolygon(shape.pointList.First(), outer.pointList))
                {
                    shape.isHole = true;
                    holes.Add(shape);
                }
            }

            return holes;
        }

        public static bool IsPointInPolygon(Vector2 point, List<Vector2> points)
        {
            bool inside = false;

            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                if ((points[i].Y <= point.Y && point.Y < points[j].Y ||
                    points[j].Y <= point.Y && point.Y < points[i].Y) &&
                    point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static readonly Dictionary<int, Edge[]> edgesForIDs = new()
        {
            // empty
            { 0000, Array.Empty<Edge>() },

            // full
            { 1111, Array.Empty<Edge>() },

            // single corner cases (filled corners)
            { 1000, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.0f, 0.5f)),
            }},
            { 0100, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(1.0f, 0.5f)),
            }},
            { 0010, new Edge[] {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},
            { 0001, new Edge[] {
                new Edge(start: new Vector2(0.5f, 1.0f), end: new Vector2(1.0f, 0.5f)),
            }},

            // single corner cases (empty corners)
            { 0111, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.0f, 0.5f)),
            }},
            { 1011, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(1.0f, 0.5f)),
            }},
            { 1101, new Edge[] {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},
            { 1110, new Edge[] {
                new Edge(start: new Vector2(0.5f, 1.0f), end: new Vector2(1.0f, 0.5f)),
            }},

            // sides

            // top
            { 1100, new Edge[] {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(1.0f, 0.5f)),
            }},
            // bottom
            { 0011, new Edge[] {
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(1.0f, 0.5f)),
            }},
            // left
            { 1010, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.5f, 1.0f)),
            }},
            // right
            { 0101, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.5f, 1.0f)),
            }},

            // diagonals

            // right top + left bottom
            { 0110, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(0.0f, 0.5f)),
                new Edge(start: new Vector2(0.5f, 1.0f), end: new Vector2(1.0f, 0.5f)),
            }},

            // left top + right bottom
            { 1001, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.0f), end: new Vector2(1.0f, 0.5f)),
                new Edge(start: new Vector2(0.0f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},
        };
    }
}