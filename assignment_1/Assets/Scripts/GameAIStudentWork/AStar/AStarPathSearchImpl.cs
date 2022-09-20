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
                    pathResult = PathSearchResultType.Complete;
                    break;
                }

                // otherwise get its outgoing connections
                List<int> connections = getAdjacencies(currentNodeIndex);

                // endNode Record
                PathSearchNodeRecord endNodeRecord = null;

                // loop through each connection in turn
                foreach (var connectionIndex in connections)
                {
                    float endNodeCost = currentNodeRecord.CostSoFar + G(getNode(currentNodeIndex), getNode(connectionIndex));
                    float endNodeHeuristic = 0f;
                    // if the node is closed we may have to skip or remove it
                    // from the closed list
                    if (closedNodes.Contains(connectionIndex))
                    {
                        // find the record in the closed list
                        // corresponding to the endNode
                        endNodeRecord = searchNodeRecords[connectionIndex];

                        // if we didn't find a shorter route, skip
                        if (endNodeRecord.CostSoFar <= endNodeCost)
                        {
                            continue;
                        }

                        // otherwise remove it from the closed list
                        closedNodes.Remove(connectionIndex);

                        // we can use the node's old cost values to calculate its heuristic
                        // without calling the possibly expensive heuristic function
                        endNodeHeuristic = endNodeRecord.EstimatedTotalCost -
                                           endNodeRecord.CostSoFar;
                    }

                    // skip if the node is open and we've not found a better route
                    else if (openNodes.Contains(connectionIndex))
                    {
                        // here we find the record in the open list corresponding to the endNode
                        endNodeRecord = searchNodeRecords[connectionIndex];

                        // if our route is no better, then skip
                        if (endNodeRecord.CostSoFar <= endNodeCost)
                        {
                            continue;
                        }

                        // again, we calculate its heuristic
                        endNodeHeuristic = endNodeRecord.EstimatedTotalCost -
                                           endNodeRecord.CostSoFar;
                    }

                    // otherwise we know we've got an unvisited node,
                    // so make a record for it
                    else
                    {
                        endNodeRecord = new PathSearchNodeRecord(connectionIndex);

                        // calculate the heuristic value using the function
                        endNodeHeuristic = H(getNode(currentNodeIndex), getNode(connectionIndex));
                    }

                    // we're here if we need to update the node. Update cost, estimate, and connection
                    endNodeRecord.CostSoFar          = endNodeCost;
                    endNodeRecord.FromNodeIndex      = connectionIndex;
                    endNodeRecord.EstimatedTotalCost = endNodeCost + endNodeHeuristic;

                    // and add it to the open list
                    if (!openNodes.Contains(connectionIndex))
                    {
                        openNodes.Enqueue(connectionIndex, endNodeHeuristic);
                    }
                }

                // we've finished looking at the connections for the current node, so add it to the
                // closed list andr emove it from the open list
                openNodes.Remove(currentNodeIndex);
                closedNodes.Add(currentNodeIndex);
            }

            // we're here if we've either found the goal or if we've no more nodes to search, find which
            if (currentNodeIndex != goalNodeIndex)
            {
                // we've run out of nodes without finding the goal, so there's no solution
                pathResult = PathSearchResultType.Partial;
                //return null;
            }
            else
            {
                // compile the list of connections in the path
                while (currentNodeIndex != startNodeIndex)
                {
                    returnPath.Add(currentNodeIndex);
                    currentNodeIndex = searchNodeRecords[currentNodeIndex].FromNodeIndex;
                }
                returnPath.Add(startNodeIndex);
                returnPath.Reverse();
            }

            return pathResult;

            //END STUDENT CODE HERE
        }

    }

}