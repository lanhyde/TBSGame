using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemHex<TGridObject>
{
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;
    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridObjects;

    public int Width => width;
    public int Height => height;
    public GridSystemHex(int width, int height, float cellSize, Func<GridSystemHex<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        gridObjects = new TGridObject[width, height];
        for (int i = 0; i < this.width; ++i)
        {
            for (int j = 0; j < this.height; ++j)
            {
                var gridObject = createGridObject(this, new GridPosition(i, j));
                gridObjects[i, j] = gridObject;
            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x + (gridPosition.z % 2 != 0 ? 0.5f : 0), 0, gridPosition.z * HEX_VERTICAL_OFFSET_MULTIPLIER) * cellSize;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        GridPosition roughXZ = new GridPosition(Mathf.RoundToInt(worldPosition.x / cellSize), 
                                                Mathf.RoundToInt(worldPosition.z / cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER));
        bool isOddRow = roughXZ.z % 2 != 0;
        IList<GridPosition> neighborGridPositionList = new List<GridPosition>
        {
            roughXZ + new GridPosition(-1, 0),
            roughXZ + new GridPosition(1, 0),
            roughXZ + new GridPosition(0, 1),
            roughXZ + new GridPosition(0, -1),
            
            roughXZ + new GridPosition(isOddRow ? 1 : -1, 1),
            roughXZ + new GridPosition(isOddRow ? 1 : -1, -1),
        };
        var closestGridPosition = roughXZ;
        foreach (var neighborGridPosition in neighborGridPositionList)
        {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighborGridPosition)) <
                Vector3.Distance(worldPosition, GetWorldPosition(closestGridPosition)))
            {
                closestGridPosition = neighborGridPosition;
            }
        }

        return closestGridPosition;
    }

    public void CreateDebugObjects(Transform debugPrefab)
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                GridPosition gridPosition = new GridPosition(i, j);
                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                var gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjects[gridPosition.x, gridPosition.z];
    }

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < width && gridPosition.z < height;
    }
}
