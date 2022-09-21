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
            ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
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


            // STUDENT CODE HERE - incremental search taken from BasicPathSearchImpl

            pathResult = PathSearchResultType.InProgress;

            if (doInitialization)
            {
                currentNodeIndex = startNodeIndex;

                searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();

                // initialize the record for the start node
                var firstNodeRecord = new PathSearchNodeRecord(currentNodeIndex);
                searchNodeRecords.Add(firstNodeRecord.NodeIndex, firstNodeRecord);

                // initialize the open and closed lists
                openNodes = new SimplePriorityQueue<int, float>();
                openNodes.Enqueue(firstNodeRecord.NodeIndex, 0f);

                closedNodes = new HashSet<int>();
                returnPath = new List<int>();
            }

            int nodesProcessed = 0;

            // iterate through processing each node
            while (nodesProcessed < maxNumNodesToExplore && openNodes.Count > 0)
            {
                // find the smallest element in the open list using the estimated total cost
                var currentNodeRecord = searchNodeRecords[openNodes.First];
                currentNodeIndex      = currentNodeRecord.NodeIndex;

                ++nodesProcessed;

                // if it is the goal node, then terminate
                if (currentNodeIndex == goalNodeIndex)
                {
                    break;
                }

                // edge node record
                PathSearchNodeRecord edgeNodeRecord = null;

                // otherwise get all its outgoing connections
                var currEdges = getAdjacencies(currentNodeIndex);

                foreach (var edgeNodeIndex in currEdges)
                {
                    // get the cost estimate for the end node
                    var costToEdgeNode = currentNodeRecord.CostSoFar +
                                         G(getNode(currentNodeIndex), getNode(edgeNodeIndex));

                    var edgeNodeHeuristic = 0f;

                    // if the node is closed we may have to skip or remove it from closed list
                    if (closedNodes.Contains(edgeNodeIndex))
                    {
                        // here we find the record in the closed list
                        edgeNodeRecord = searchNodeRecords[edgeNodeIndex];

                        // if we didn't find a shorter route, skip
                        if (edgeNodeRecord.CostSoFar <= costToEdgeNode)
                        {
                            continue;
                        }

                        // otherwise remove it from the closed list
                        closedNodes.Remove(edgeNodeIndex);

                        // we can use the node's old cost values to calculate its heuristic
                        edgeNodeHeuristic = edgeNodeRecord.EstimatedTotalCost - edgeNodeRecord.CostSoFar;
                    }

                    // skip if node is open and we've not found a better route
                    else if (openNodes.Contains(edgeNodeIndex))
                    {
                        // here we find the record in the open list
                        edgeNodeRecord = searchNodeRecords[edgeNodeIndex];

                        // if our route is no better, then skip
                        if (edgeNodeRecord.CostSoFar <= costToEdgeNode)
                        {
                            continue;
                        }

                        // again calculate heuristic
                        edgeNodeHeuristic = edgeNodeRecord.EstimatedTotalCost - edgeNodeRecord.CostSoFar;
                    }

                    // otherwise we have an unvisited node, so make a record for it
                    else
                    {
                        edgeNodeRecord = new PathSearchNodeRecord(edgeNodeIndex);

                        // we need to calculate heuristic using function
                        edgeNodeHeuristic = H(getNode(currentNodeIndex), getNode(edgeNodeIndex));
                    }

                    // add to search node records
                    edgeNodeRecord.FromNodeIndex      = currentNodeIndex;
                    edgeNodeRecord.CostSoFar          = costToEdgeNode;
                    edgeNodeRecord.EstimatedTotalCost = costToEdgeNode + edgeNodeHeuristic;
                    searchNodeRecords[edgeNodeIndex]  = edgeNodeRecord;

                    // and add it to the open list
                    if (!openNodes.Contains(edgeNodeIndex))
                    {
                        openNodes.Enqueue(edgeNodeIndex, edgeNodeHeuristic);
                    }
                } // end foreach

                // we've finished looking at the connections for the current node so add it to the closed list
                // and remove it from the open list
                openNodes.Remove(currentNodeIndex);
                closedNodes.Add(currentNodeIndex);
            } // end while

            if (openNodes.Count <= 0 && currentNodeIndex != goalNodeIndex)
            {
                pathResult = PathSearchResultType.Partial;
                // find the closest node we looked at and use for partial path
                int closest = -1;
                float closestDist = float.MaxValue;

                foreach (var n in closedNodes)
                {
                    var nrec = searchNodeRecords[n];

                    var d = Vector2.Distance(getNode(nrec.NodeIndex), getNode(goalNodeIndex));
                    if (d < closestDist)
                    {
                        closest = n;
                        closestDist = d;
                    }
                }
                if (closest >= 0)
                {
                    currentNodeIndex = closest;
                }
            }

            // we're here if we found the goal
            else if (currentNodeIndex == goalNodeIndex)
            {
                pathResult = PathSearchResultType.Complete;
            }

            if (pathResult != PathSearchResultType.InProgress)
            {
                // processing complete, a path can be generated
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