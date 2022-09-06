// compile_check
// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse {

    public class CreateGrid
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Tom Nguyen";


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if point p is inside (or on edge) the polygon defined by pts (CCW winding). False, otherwise
        public static bool IsPointInsidePolygon(Vector2Int[] pts, Vector2Int p)
        {
            return CG.InPoly1(pts, p) != CG.PointPolygonIntersectionType.Outside;
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        public static int Convert(float v)
        {
            return CG.Convert(v);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        public static Vector2Int Convert(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static public bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        // PointInsideBoundingBox(): Determines whether a point (Vector2Int:p) is On/Inside a bounding box (such as a grid cell) defined by
        // minCellBounds and maxCellBounds (both Vector2Int's).
        // Returns true if the point is ON/INSIDE the cell and false otherwise
        // This method should return true if the point p is on one of the edges of the cell.
        // This is more efficient than PointInsidePolygon() for an equivalent dimension poly
        public static bool PointInsideBoundingBox(Vector2Int minCellBounds, Vector2Int maxCellBounds, Vector2Int p)
        {
            Debug.Log("minCellBounds: " + minCellBounds + "     maxCellBounds: " + maxCellBounds + "      p: " + p);

            if ((p.x >= minCellBounds.x) &
                (p.x <= maxCellBounds.x) &
                (p.y >= minCellBounds.y) &
                (p.y <= maxCellBounds.y))
            {
                return true;
            }

            return false;
        }


        // Istraversable(): returns true if the grid is traversable from grid[x,y] in the direction dir, false otherwise.
        // The grid boundaries are not traversable. If the grid position x,y is itself not traversable but the grid cell in direction
        // dir is traversable, the function will return false.
        // returns false if the grid is null, or any dimension of grid is zero length
        // returns false if x,y is out of range
        public static bool IsTraversable(bool[,] grid, int x, int y, TraverseDirection dir)
        {
            if (grid == null){
                return false;
            }

            if (grid.GetLength(0) == 0){
                return false;
            }

            if (grid.GetLength(1) == 0){
                return false;
            }

            switch (dir)
            {
                case TraverseDirection.Up:
                    return grid[x, y + 1];
                case TraverseDirection.Down:
                    return grid[x, y - 1];
                case TraverseDirection.Left:
                    return grid[x - 1, y];
                case TraverseDirection.Right:
                    return grid[x + 1, y];
                case TraverseDirection.UpLeft:
                    return grid[x - 1, y + 1];
                case TraverseDirection.UpRight:
                    return grid[x + 1, y + 1];
                case TraverseDirection.DownLeft:
                    return grid[x - 1, y - 1];
                case TraverseDirection.DownRight:
                    return grid[x - 1, y + 1];
                default:
                    break;
            }

            return true;
        }

        // Create(): Creates a grid lattice discretized space for navigation.
        // canvasOrigin: bottom left corner of navigable region in world coordinates
        // canvasWidth: width of navigable region in world dimensions
        // canvasHeight: height of navigable region in world dimensions
        // cellWidth: target cell width (of a grid cell) in world dimensions
        // obstacles: a list of collider obstacles
        // grid: an array of bools. row major. a cell is true if navigable, false otherwise

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight, float cellWidth,
            List<Polygon> obstacles,
            out bool[,] grid
            )
        {
            // ignoring the obstacles for this limited demo; 
            // Marks cells of the grid untraversable if geometry intersects interior!
            // Carefully consider all possible geometry interactions

            // also ignoring the world boundary defined by canvasOrigin and canvasWidth and canvasHeight

            // create grid to match size of canvas
            
            Vector2 canvasVector;
            canvasVector.x = canvasWidth;
            canvasVector.y = canvasHeight;

            Vector2Int convertedCanvasVector = Convert(canvasVector);
            Vector2Int convertedCanvasOrigin = Convert(canvasOrigin);
            Vector2Int offsetOrigin = convertedCanvasOrigin * -1;

            int convertedCellWidth = Convert(cellWidth);
            //Debug.Log("canvasDimensions: " + convertedCanvasVector + "    canvasCell: " + convertedCellWidth + "   canvas origin: " + convertedCanvasOrigin);

            int heightCount = convertedCanvasVector.y / convertedCellWidth;
            int widthCount = convertedCanvasVector.x / convertedCellWidth;

            grid = new bool[widthCount, heightCount];

            // fill array with true
            for(int i=0; i<widthCount; i++)
            {
                for(int j=0; j<heightCount; j++)
                {
                    grid[i, j] = true;
                }
            }

            // for each obstacle, find each point
            foreach(var obstacle in obstacles)
            {

                Vector2Int[] obstaclePoints = obstacle.getIntegerPoints();

                foreach(var point in obstaclePoints)
                {
                    int x = point.x;
                    int y = point.y;

                    int offset_x = x + offsetOrigin.x;
                    int offset_y = y + offsetOrigin.y;

                    Vector2Int offset_point = new Vector2Int(offset_x, offset_y);

                    for (int i = 0; i < widthCount; i++)
                    {
                        for (int j = 0; j < heightCount; j++)
                        {
                            Vector2Int minBounds = new Vector2Int(i * convertedCellWidth+1, j * convertedCellWidth+1);
                            Vector2Int maxBounds = new Vector2Int((i + 1) * convertedCellWidth-1, (j + 1) * convertedCellWidth-1);

                            // check points
                            if (PointInsideBoundingBox(minBounds, maxBounds, offset_point))
                            {
                                grid[i, j] = false;
                            }

                        }
                    }
                }

                // check if edges intersect
                var len = obstaclePoints.Length;
                for (int i = 0, j = len - 1; i < len; j = i++){
                    var pt1 = obstaclePoints[j];
                    var pt2 = obstaclePoints[i];

                    Debug.Log("pt1: " + pt1 + "        pt2: " + pt2);

                    for (int k = 0; k < widthCount; k++)
                    {
                        for (int m = 0; m < heightCount; m++)
                        {
                            Vector2Int cell_origin = new Vector2Int(k * convertedCellWidth, m * convertedCellWidth); //bottom-left
                            Vector2Int cell_top_left = new Vector2Int(cell_origin.x, cell_origin.y + convertedCellWidth);

                            if (Intersects(pt1, pt2, cell_origin, cell_top_left))
                            {
                                grid[k, m] = false;
                            }
                        }
                    }
                }
            }

        }

    }

}