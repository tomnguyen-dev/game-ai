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
            // Debug.Log("x: " + x + "      y: " + y + "       dir: " + dir);
            int gridWidth  = grid.GetLength(0) - 1;
            int gridHeight = grid.GetLength(1) - 1;

            // returns false if the grid is null
            if (grid == null){
                return false;
            }

            // or any dimension of grid is zero length
            if (gridWidth == 0 || gridHeight == 0){
                return false;
            }

            // If the grid position x,y is itself not traversable but the grid cell in direction
            // dir is traversable, the function will return false.
            if (grid[x, y] == false){
                return false;
            }

            switch (dir)
            {
                case TraverseDirection.Up:
                    if (y+1 <= gridHeight){return grid[x, y + 1];}
                    break;
                case TraverseDirection.Down:
                    if (y-1 >= 0){return grid[x, y - 1];}
                    break;
                case TraverseDirection.Left:
                    if (x-1 >= 0){return grid[x - 1, y];}
                    break;
                case TraverseDirection.Right:
                    if (x+1 <= gridWidth){return grid[x + 1, y];}
                    break;
                case TraverseDirection.UpLeft:
                    if (x-1 >= 0 & y+1 <= gridHeight) {return grid[x - 1, y + 1];}
                    break;
                case TraverseDirection.UpRight:
                    if (x+1 <= gridWidth & y+1 <= gridHeight) {return grid[x + 1, y + 1];}
                    break;
                case TraverseDirection.DownLeft:
                    if (x-1 >= 0 & y-1 >= 0) {return grid[x - 1, y - 1];}
                    break;
                case TraverseDirection.DownRight:
                    if (x-1 >= 0 & y+1 <= gridHeight) {return grid[x - 1, y + 1];}
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void ProcessObstacleCorners(ref bool[,] grid, List<Polygon> obstacles, Vector2Int convertedCanvasOrigin, Vector2Int scaledGridDimensions, Vector2Int scaledCanvasDimensions, int scaledCellWidth){
            // for each obstacle
            foreach (var obstacle in obstacles){

                // for each vertex in the obstacle
                foreach (var point in obstacle.getIntegerPoints()){
                    // check if the vertex is within the bounds of a cell
                    for (int i = 0; i < scaledGridDimensions.x; i++){
                        for (int j = 0; j < scaledGridDimensions.y; j++){
                            // adjust cell to world coordinates using origin and shrink the cell sizes
                            Vector2Int minCellBounds = new Vector2Int(i * scaledCellWidth + convertedCanvasOrigin.x + 1, j * scaledCellWidth + convertedCanvasOrigin.y + 1);
                            Vector2Int maxCellBounds = new Vector2Int((i+1) * scaledCellWidth + convertedCanvasOrigin.x - 1, (j+1) * scaledCellWidth + convertedCanvasOrigin.y - 1);

                            // check point against minCellBounds and maxCellBounds
                            if(PointInsideBoundingBox(minCellBounds, maxCellBounds, point))
                            {
                                grid[i, j] = false;
                            }
                        }
                    }
                }
            }
        }

        public static void ProcessInsidePolygon(ref bool[,] grid, List<Polygon> obstacles, Vector2Int convertedCanvasOrigin, Vector2Int scaledGridDimensions, Vector2Int scaledCanvasDimensions, int scaledCellWidth){
            // go through each obstacle and get their points
            foreach(var obstacle in obstacles){
                Vector2Int[] obstaclePoints = obstacle.getIntegerPoints();

                // go through each cell and check if the bottom-left corner is inside a point
                for (int i = 0; i < scaledGridDimensions.x; i++){
                    for (int j = 0; j < scaledGridDimensions.y; j++){
                        Vector2Int cellBottomLeft = new Vector2Int(i * scaledCellWidth + convertedCanvasOrigin.x + 1, j * scaledCellWidth + convertedCanvasOrigin.y + 1);
                        Vector2Int cellBottomRight = new Vector2Int((i+1) * scaledCellWidth + convertedCanvasOrigin.x + 1, j * scaledCellWidth + convertedCanvasOrigin.y + 1);
                        Vector2Int cellTopLeft = new Vector2Int(i * scaledCellWidth + convertedCanvasOrigin.x + 1, (j+1) * scaledCellWidth + convertedCanvasOrigin.y + 1);
                        Vector2Int cellTopRight = new Vector2Int((i+1) * scaledCellWidth + convertedCanvasOrigin.x + 1, (j+1) * scaledCellWidth + convertedCanvasOrigin.y + 1);
                        if ((IsPointInsidePolygon(obstaclePoints, cellBottomLeft))  ||
                            (IsPointInsidePolygon(obstaclePoints, cellBottomRight)) ||
                            (IsPointInsidePolygon(obstaclePoints, cellTopLeft))     ||
                            (IsPointInsidePolygon(obstaclePoints, cellTopRight))){
                            grid[i, j] = false;
                        }
                    }
                }
            }
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

            // convert canvas dimensions to a Vector2Int using scaling factor
            Vector2Int scaledCanvasDimensions = Convert(new Vector2(canvasWidth, canvasHeight));

            // scale the cellWidth
            int scaledCellWidth = Convert(cellWidth);

            // find the grid counts based on the scaled canvas dimensions and scaled cell widths
            Vector2Int scaledGridDimensions = new Vector2Int(scaledCanvasDimensions.x / scaledCellWidth, scaledCanvasDimensions.y / scaledCellWidth);

            // create grid and set everything to true
            grid = new bool[scaledGridDimensions.x, scaledGridDimensions.y];
            for(int i = 0; i < scaledGridDimensions.x; i++){
                for(int j = 0; j < scaledGridDimensions.y; j++){
                    grid[i,j] = true;
                }
            }

            // grid[1,1] = true;
            // grid[1,2] = true;

            // convert the origin to a Vector2Int
            Vector2Int convertedCanvasOrigin = Convert(canvasOrigin);

            // check the corners for each obstacle
            ProcessObstacleCorners(ref grid, obstacles, convertedCanvasOrigin, scaledGridDimensions, scaledCanvasDimensions, scaledCellWidth);

            // check for points inside obstacles
            ProcessInsidePolygon(ref grid, obstacles, convertedCanvasOrigin, scaledGridDimensions, scaledCanvasDimensions, scaledCellWidth);
        }
    }
}