// compile_check
// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse
{

    public class CreatePathNetwork
    {

        public const string StudentAuthorName = "Tom Nguyen";




        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        public static Vector2Int Convert(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        public static int Convert(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static public bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }

        // Helper method provided to help you implement this file. Leave as is.
        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Determines if a point is inside/on a CCW polygon and if so returns true. False otherwise.
        public static bool IsPointInPolygon(Vector2Int[] polyPts, Vector2Int point)
        {
            return CG.PointPolygonIntersectionType.Outside != CG.InPoly1(polyPts, point);
        }




        //Student code to build the path network from the given pathNodes and Obstacles
        //Obstacles - List of obstacles on the plane
        //agentRadius - the radius of the traversing agent
        //pathEdges - out parameter that will contain the edges you build.
        //  Edges cannot intersect with obstacles or boundaries. Edges must be at least agentRadius distance
        //  from all obstacle/boundary line segments

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
            List<Polygon> obstacles, float agentRadius, List<Vector2> pathNodes, out List<List<int>> pathEdges)
        {

            //STUDENT CODE HERE

            // pathEdges[0] = pathNodes[0]
            // pathedges[0] = {1, 2, 3} means node 0 has an edge to node 1, 2, and 3
            pathEdges = new List<List<int>>(pathNodes.Count);

            for (int i = 0; i < pathNodes.Count; ++i)
            {
                pathEdges.Add(new List<int>());
            }

            // convert parameters
            Vector2Int convertedCanvasOrigin = Convert(canvasOrigin);

            int convertedAgentRadius = Convert(agentRadius);

            // find max x and y values
            int maxHorizontalBoundary = convertedCanvasOrigin.x + Convert(canvasWidth);
            int maxVerticalBoundary   = convertedCanvasOrigin.y + Convert(canvasHeight);

            // first check if the nodes are inside the canvas and not under an obstacle
            List<int> validNodeIndices = new List<int>();

            for (int i = 0; i < pathNodes.Count; i++){
                Vector2Int convertedNode = Convert(pathNodes[i]);
                if ((convertedNode.x - convertedAgentRadius >= convertedCanvasOrigin.x) &&
                    (convertedNode.x + convertedAgentRadius <= maxHorizontalBoundary)   &&
                    (convertedNode.y - convertedAgentRadius >= convertedCanvasOrigin.y) &&
                    (convertedNode.y + convertedAgentRadius <= maxVerticalBoundary)) {

                    // if node is inside the canvas, then get the points of the node
                    Vector2Int topPoint = new Vector2Int(convertedNode.x, convertedNode.y + convertedAgentRadius);
                    Vector2Int btmPoint = new Vector2Int(convertedNode.x, convertedNode.y - convertedAgentRadius);
                    Vector2Int lftPoint = new Vector2Int(convertedNode.x - convertedAgentRadius, convertedNode.y);
                    Vector2Int rgtPoint = new Vector2Int(convertedNode.x + convertedAgentRadius, convertedNode.y);

                    bool insidePolygon = false;

                    // check each obstacle
                    foreach (var obstacle in obstacles){
                        Vector2Int[] obstaclePoints = obstacle.getIntegerPoints();
                        // check if the 5 points of the node are inside an obstacle
                        if ((IsPointInPolygon(obstaclePoints, convertedNode)) ||
                            (IsPointInPolygon(obstaclePoints, topPoint))      ||
                            (IsPointInPolygon(obstaclePoints, btmPoint))      ||
                            (IsPointInPolygon(obstaclePoints, lftPoint))      ||
                            (IsPointInPolygon(obstaclePoints, rgtPoint))){
                                insidePolygon = true;
                            }
                    }
                    if (!insidePolygon) {validNodeIndices.Add(i);}
                }
            }

            foreach (var item in validNodeIndices){
                Debug.Log(item);
            }

            // END STUDENT CODE

        }


    }

}