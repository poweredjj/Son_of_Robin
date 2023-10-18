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
            public readonly double angleRadians;

            public Edge(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end = end;
                this.angleRadians = Math.Atan2(end.Y - start.Y, end.X - start.X);
            }

            public static Edge ReversedEdge(Edge edge)
            {
                return new Edge(start: edge.end, end: edge.start);
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

                    if (x == -1 && y == -1)
                    {
                        int prevVal = neighbourArray[0, 0];
                        neighbourArray[0, 0] = 2;
                        int edgeID = GetEdgeID(neighbourArray);
                        if (!edgesForIDs.ContainsKey(edgeID)) neighbourArray[0, 0] = prevVal;
                    }

                    if (x == width - 1 && y == -1)
                    {
                        int prevVal = neighbourArray[1, 0];
                        neighbourArray[1, 0] = 2;
                        int edgeID = GetEdgeID(neighbourArray);
                        if (!edgesForIDs.ContainsKey(edgeID)) neighbourArray[1, 0] = prevVal;
                    }

                    if (x == -1 && y == height - 1)
                    {
                        int prevVal = neighbourArray[0, 1];
                        neighbourArray[0, 1] = 2;
                        int edgeID = GetEdgeID(neighbourArray);
                        if (!edgesForIDs.ContainsKey(edgeID)) neighbourArray[0, 1] = prevVal;
                    }

                    if (x == width - 1 && y == height - 1)
                    {
                        int prevVal = neighbourArray[1, 1];
                        neighbourArray[1, 1] = 2;
                        int edgeID = GetEdgeID(neighbourArray);
                        if (!edgesForIDs.ContainsKey(edgeID)) neighbourArray[1, 1] = prevVal;
                    }

                    CalculateMarchingCellAndAddEdgesToSet(edgeSet: edgeSet, pos: currentPos, neighbourArray: neighbourArray);
                }
            }

            return OrderAndMergeEdges(edgeSet);
        }

        public static void CalculateMarchingCellAndAddEdgesToSet(HashSet<Edge> edgeSet, Vector2 pos, int[,] neighbourArray)
        {
            foreach (Edge edge in edgesForIDs[GetEdgeID(neighbourArray)])
            {
                edgeSet.Add(new Edge(start: pos + edge.start, end: pos + edge.end));
            }
        }

        private static int GetEdgeID(int[,] neighbourArray)
        {
            return (neighbourArray[0, 0] * 1000) + (neighbourArray[1, 0] * 100) + (neighbourArray[0, 1] * 10) + neighbourArray[1, 1];
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
                                currentShape.pointList.Remove(currentEdge.end);
                                nextEdge = new Edge(start: currentEdge.start, end: nextEdge.end);
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

            // left top corner, connected to neighbour chunk
            { 2001, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(1.0f, 0.5f)),
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},  

            // right top corner, connected to neighbour chunk
            { 0210, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(0.0f, 0.5f)),
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(0.5f, 1.0f)),
            }},  
            
            // bottom left corner, connected to neighbour chunk
            { 0120, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(0.5f, 0.0f)),
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(1.0f, 0.5f)),
            }},  
            
            // bottom right corner, connected to neighbour chunk
            { 1002, new Edge[] {
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(0.5f, 0.0f)),
                new Edge(start: new Vector2(0.5f, 0.5f), end: new Vector2(0.0f, 0.5f)),
            }},  

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