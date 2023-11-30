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
    
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstacleLayer;
    
    private int width;
    private int height;
    private float cellSize;
    private GridSystemHex<PathNode> _gridSystemHex;
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
        
        _gridSystemHex =
            new GridSystemHex<PathNode>(width, height, cellSize, (_, gridPosition) => new PathNode(gridPosition));
        // gridSystem.CreateDebugObjects(gridDebugObjectPrefab);

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

    public IList<GridPosition> FindPath(GridPosition startPosition, GridPosition endPosition, out int pathLength)
    {
        IList<PathNode> openList = new List<PathNode>();
        IList<PathNode> closedList = new List<PathNode>();

        PathNode startNode = _gridSystemHex.GetGridObject(startPosition);
        PathNode endNode = _gridSystemHex.GetGridObject(endPosition);
        openList.Add(startNode);

        for (int x = 0; x < _gridSystemHex.Width; ++x)
        {
            for (int z = 0; z < _gridSystemHex.Height; ++z)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                PathNode pathNode = _gridSystemHex.GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();
            }
        }
        
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateHeuristicDistance(startPosition, endPosition));
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);
            if (currentNode == endNode)
            {
                pathLength = endNode.FCost;
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

                int tentativeGCost = currentNode.GCost + MOVE_STRAIGHT_COST;
                                     
                if (tentativeGCost < neighborNode.GCost)
                {
                    neighborNode.SetCameFromPathNode(currentNode);
                    neighborNode.SetGCost(tentativeGCost);
                    neighborNode.SetHCost(CalculateHeuristicDistance(neighborNode.GetGridPosition(), endPosition));
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        pathLength = 0;
        return null;
    }

    public int CalculateHeuristicDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        return Mathf.RoundToInt(MOVE_STRAIGHT_COST * Vector3.Distance(_gridSystemHex.GetWorldPosition(gridPositionA),
            _gridSystemHex.GetWorldPosition(gridPositionB)));
    }

    private PathNode GetLowestFCostPathNode(IList<PathNode> pathNodes)
    {
        PathNode lowestFCostPathNode = pathNodes.Aggregate((a, b) => a.FCost < b.FCost ? a : b);
        return lowestFCostPathNode;
    }

    private PathNode GetNode(int x, int z)
    {
        return _gridSystemHex.GetGridObject(new GridPosition(x, z));
    }
    private IList<PathNode> GetNeighborNodes(PathNode currentNode)
    {
        List<PathNode> neighbors = new List<PathNode>();
        
        GridPosition gridPosition = currentNode.GetGridPosition();
        if (gridPosition.x - 1 >= 0)
        {
            // Left
            neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z));
        }

        if (gridPosition.x + 1 < _gridSystemHex.Width)
        {
            // Right
            neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z));
        }

        if (gridPosition.z - 1 >= 0)
        {
            // Down
            neighbors.Add(GetNode(gridPosition.x, gridPosition.z - 1));
        }

        if (gridPosition.z + 1 < _gridSystemHex.Height)
        {
            // Up
            neighbors.Add(GetNode(gridPosition.x, gridPosition.z + 1));
        }

        bool isOddRow = gridPosition.z % 2 != 0;
        if (isOddRow)
        {
            if (gridPosition.x + 1 >= _gridSystemHex.Width) return neighbors;
            if (gridPosition.z - 1 >= 0)
            {
                neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < _gridSystemHex.Height)
            {
                neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1));
            }
        }
        else
        {
            if (gridPosition.x - 1 < 0) return neighbors;
            if (gridPosition.z - 1 >= 0)
            {
                neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < _gridSystemHex.Height)
            {
                neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1));
            }
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
        
        return pathNodes.Reverse().Select(node => node.GetGridPosition()).ToList();
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return _gridSystemHex.GetGridObject(gridPosition).IsWalkable;
    }

    public bool HasPath(GridPosition startPosition, GridPosition endPosition)
    {
        return FindPath(startPosition, endPosition, out _) != null;
    }

    public int GetPathLength(GridPosition startPosition, GridPosition endPosition)
    {
        _ = FindPath(startPosition, endPosition, out int pathLength);
        return pathLength;
    }

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable)
    {
        _gridSystemHex.GetGridObject(gridPosition).IsWalkable = isWalkable;
    }
}
