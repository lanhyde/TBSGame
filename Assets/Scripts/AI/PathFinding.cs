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
    [SerializeField] private LayerMask floorLayer;
    
    private int width;
    private int height;
    private float cellSize;
    private IList<GridSystem<PathNode>> gridSystems;
    private int floorAmount;
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

    public void Setup(int width, int height, float cellSize, int floorAmount)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.floorAmount = floorAmount;

        gridSystems = new List<GridSystem<PathNode>>();
        for (int floor = 0; floor < floorAmount; ++floor)
        {
            var gridSystem =
                new GridSystem<PathNode>(width, height, cellSize, floor, LevelGrid.FLOOR_HEIGHT, (_, gridPosition) => new PathNode(gridPosition));
            gridSystems.Add(gridSystem);
        }

        // gridSystem.CreateDebugObjects(gridDebugObjectPrefab);

        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                for (int floor = 0; floor < floorAmount; ++floor)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    float raycastOffsetDistance = 1f;

                    GetNode(x, z, floor).IsWalkable = false;
                    
                    if (Physics.Raycast(worldPosition + Vector3.up * raycastOffsetDistance,
                            Vector3.down,
                            raycastOffsetDistance * 2,
                            floorLayer))
                    {
                        GetNode(x, z, floor).IsWalkable = true;
                    }
                    
                    if (Physics.Raycast(worldPosition + Vector3.down * raycastOffsetDistance,
                            Vector3.up,
                            raycastOffsetDistance * 2,
                            obstacleLayer))
                    {
                        GetNode(x, z, floor).IsWalkable = false;
                    }
                }
            }
        }
    }

    public IList<GridPosition> FindPath(GridPosition startPosition, GridPosition endPosition, out int pathLength)
    {
        IList<PathNode> openList = new List<PathNode>();
        IList<PathNode> closedList = new List<PathNode>();

        PathNode startNode = GetGridSystem(startPosition.floor).GetGridObject(startPosition);
        PathNode endNode = GetGridSystem(endPosition.floor).GetGridObject(endPosition);
        openList.Add(startNode);

        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                for (int floor = 0; floor < floorAmount; ++floor)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    PathNode pathNode = GetGridSystem(floor).GetGridObject(gridPosition);

                    pathNode.SetGCost(int.MaxValue);
                    pathNode.SetHCost(0);
                    pathNode.CalculateFCost();
                    pathNode.ResetCameFromPathNode();
                }

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

        pathLength = 0;
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

    private GridSystem<PathNode> GetGridSystem(int floor)
    {
        return gridSystems[floor];
    }

    private PathNode GetNode(int x, int z, int floor)
    {
        return GetGridSystem(floor).GetGridObject(new GridPosition(x, z, floor));
    }
    private IList<PathNode> GetNeighborNodes(PathNode currentNode)
    {
        List<PathNode> neighbors = new List<PathNode>();
        
        GridPosition gridPosition = currentNode.GetGridPosition();
        if (gridPosition.x - 1 >= 0)
        {
            // Left
            neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z, gridPosition.floor));
            if (gridPosition.z - 1 >= 0)
            {
                // Left Down
                neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor));
            }

            if (gridPosition.z + 1 < height)
            {
                // Left Up
                neighbors.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor));
            }
        }

        if (gridPosition.x + 1 < width)
        {
            // Right
            neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z, gridPosition.floor));
            if (gridPosition.z - 1 >= 0)
            {
                // Right Down
                neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor));
            }

            if (gridPosition.z + 1 < height)
            {
                // Right Up
                neighbors.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor));
            }
        }

        if (gridPosition.z - 1 >= 0)
        {
            // Down
            neighbors.Add(GetNode(gridPosition.x, gridPosition.z - 1, gridPosition.floor));
        }

        if (gridPosition.z + 1 < height)
        {
            // Up
            neighbors.Add(GetNode(gridPosition.x, gridPosition.z + 1, gridPosition.floor));
        }

        List<PathNode> totalNeighbors = new List<PathNode>(neighbors);

        foreach (var pathNode in neighbors)
        {
            GridPosition neighborGridPosition = pathNode.GetGridPosition();
            if (neighborGridPosition.floor - 1 >= 0)
            {
                totalNeighbors.Add(GetNode(neighborGridPosition.x, neighborGridPosition.z, neighborGridPosition.floor - 1));
            }
            if (neighborGridPosition.floor + 1 < floorAmount)
            {
                totalNeighbors.Add(GetNode(neighborGridPosition.x, neighborGridPosition.z, neighborGridPosition.floor + 1));
            }
        }
        return totalNeighbors;
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
        return GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).IsWalkable;
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
        GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).IsWalkable = isWalkable;
    }
}
