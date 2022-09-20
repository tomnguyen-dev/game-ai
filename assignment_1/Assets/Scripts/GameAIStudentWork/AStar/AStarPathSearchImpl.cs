// compile_check
// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;


namespace GameAICourse
{


    public class AStarPathSearchImpl
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Tom Nguyen";


        // Null Heuristic for Dijkstra
        public static float HeuristicNull(Vector2 nodeA, Vector2 nodeB)
        {
            return 0f;
        }

        // Null Cost for Greedy Best First
        public static float CostNull(Vector2 nodeA, Vector2 nodeB)
        {
            return 0f;
        }



        // Heuristic distance fuction implemented with manhattan distance
        public static float HeuristicManhattan(Vector2 nodeA, Vector2 nodeB)
        {
            return Mathf.Abs(nodeA.x - nodeB.x) + Mathf.Abs(nodeA.y - nodeB.y);
        }

        // Heuristic distance function implemented with Euclidean distance
        public static float HeuristicEuclidean(Vector2 nodeA, Vector2 nodeB)
        {
            return Mathf.Abs(Vector2.Distance(nodeA, nodeB));
        }


        // Cost is only ever called on adjacent nodes. So we will always use Euclidean distance.
        // We could use Manhattan dist for 4-way connected grids and avoid sqrroot and mults.
        // But we will avoid that for simplicity.
        public static float Cost(Vector2 nodeA, Vector2 nodeB)
        {
            return Mathf.Abs(Vector2.Distance(nodeA, nodeB));
        }



        public static PathSearchResultType FindPathIncremental(
            GetNodeCount getNodeCount,
            GetNode getNode,
            GetNodeAdjacencies getAdjacencies,
            CostCallback G,
            CostCallback H,
            int startNodeIndex, int goalNodeIndex,
            int maxNumNodesToExplore, bool doInitialization,
            ref int currentNodeIndex,
            ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,    // don't forget to add goal node unless it isn't reachable
            ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
        {
            PathSearchResultType pathResult = PathSearchResultType.InProgress;

            var nodeCount = getNodeCount();

            if (startNodeIndex >= nodeCount || goalNodeIndex >= nodeCount ||
                startNodeIndex < 0 || goalNodeIndex < 0 ||
                maxNumNodesToExplore <= 0 ||
                (!doInitialization &&
                 (openNodes == null || closedNodes == null || currentNodeIndex < 0 ||
                  currentNodeIndex >= nodeCount )))

                return PathSearchResultType.InitializationError;


            // STUDENT CODE HERE

            //pathResult = PathSearchResultType.Complete;

            if (doInitialization)
            {
                searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();

                // initialize the record for the start node
                var startRecord = new PathSearchNodeRecord(startNodeIndex);

                // add startRecord to the searchNodeRecords
                searchNodeRecords.Add(startRecord.NodeIndex, startRecord);

                // initialize the open and closed lists
                openNodes = new SimplePriorityQueue<int, float>();
                openNodes.Enqueue(startRecord.NodeIndex, 0f);

                closedNodes = new HashSet<int>();
                returnPath = new List<int>();
            }

            // iterate through processing each node
            while (openNodes.Count > 0)
            {
                // find the smallest element in the open list (using estimatedTotalCost)
                var currentNodeRecord = searchNodeRecords[openNodes.First];
                currentNodeIndex = currentNodeRecord.NodeIndex;

                // if it is the goal node, then terminate
                if (currentNodeIndex == goalNodeIndex)
                {
                    break;
                }

                // otherwise get its outgoing connections
                List<int> connections = getAdjacencies(currentNodeIndex);

                // loop through each connection in turn
                foreach (var connection in connections)
                {
                    float endNodeCost = currentNodeRecord.CostSoFar + G(getNode(currentNodeIndex), getNode(connection));
                }
            }

            //returnPath.Add(startNodeIndex);

            return pathResult;

            //END STUDENT CODE HERE
        }

    }

}