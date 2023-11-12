using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public static PathFinding Instance { get; private set; }
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGNOAL_COST = 14;
    
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstacleLayer;
    
    private int width;
    private int height;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one PathFinding instances! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        
        gridSystem =
            new GridSystem<PathNode>(width, height, cellSize, (_, gridPosition) => new PathNode(gridPosition));
        gridSystem.CreateDebugObjects(gridDebugObjectPrefab);

        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                float raycastOffsetDistance = 5f;
                if (Physics.Raycast(worldPosition + Vector3.down * raycastOffsetDistance,
                        Vector3.up,
                        raycastOffsetDistance * 2,
                        obstacleLayer))
                {
                    GetNode(x, z).IsWalkable = false;
                }
            }
        }
    }

    public IList<GridPosition> FindPath(GridPosition startPosition, GridPosition endPosition)
    {
        IList<PathNode> openList = new List<PathNode>();
        IList<PathNode> closedList = new List<PathNode>();

        PathNode startNode = gridSystem.GetGridObject(startPosition);
        PathNode endNode = gridSystem.GetGridObject(endPosition);
        openList.Add(startNode);

        for (int x = 0; x < gridSystem.Width; ++x)
        {
            for (int z = 0; z < gridSystem.Height; ++z)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();
            }
        }
        
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startPosition, endPosition));
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (var neighborNode in GetNeighborNodes(currentNode))
            {
                if (closedList.Contains(neighborNode))
                {
                    continue;
                }

                if (!neighborNode.IsWalkable)
                {
                    closedList.Add(neighborNode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost +
                                     CalculateDistance(currentNode.GetGridPosition(), neighborNode.GetGridPosition());
                if (tentativeGCost < neighborNode.GCost)
                {
                    neighborNode.SetCameFromPathNode(currentNode);
                    neighborNode.SetGCost(tentativeGCost);
                    neighborNode.SetHCost(CalculateDistance(neighborNode.GetGridPosition(), endPosition));
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return null;
    }

    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGNOAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostPathNode(IList<PathNode> pathNodes)
    {
        PathNode lowestFCostPathNode = pathNodes.Aggregate((a, b) => a.FCost < b.FCost ? a : b);
        return lowestFCostPathNode;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }
    private IList<PathNode> GetNeighborNodes(PathNode currentNode)
    {
        List<PathNode> neighbors = new List<PathNode>();
        
        GridPosition gridPosition = currentNode.GetGridPosition();
        if (gridPosition.x - 1 >= 0)
        {
            // Left
            neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z));
            if (gridPosition.z - 1 >= 0)
            {
                // Left Down
                neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < gridSystem.Height)
            {
                // Left Up
                neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1));
            }
        }

        if (gridPosition.x + 1 < gridSystem.Width)
        {
            // Right
            neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z));
            if (gridPosition.z - 1 >= 0)
            {
                // Right Down
                neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < gridSystem.Height)
            {
                // Right Up
                neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1));
            }
        }

        if (gridPosition.z - 1 >= 0)
        {
            // Down
            neighbors.Add(GetNode(gridPosition.x, gridPosition.z - 1));
        }

        if (gridPosition.z + 1 < gridSystem.Height)
        {
            // Up
            neighbors.Add(GetNode(gridPosition.x, gridPosition.z + 1));
        }
        
        return neighbors;
    }

    private IList<GridPosition> CalculatePath(PathNode node)
    {
        IList<PathNode> pathNodes = new List<PathNode>(); 
        
        pathNodes.Add(node);

        PathNode currentNode = node;
        while (currentNode.GetCameFromPathNode() != null)
        {
            pathNodes.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
        }

        pathNodes.Reverse();
        return pathNodes.Select(node => node.GetGridPosition()).ToList();
    }
}
