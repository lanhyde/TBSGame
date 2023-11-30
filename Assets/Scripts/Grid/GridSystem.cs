using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject> 
{
    private int width;
    private int height;
    private float cellSize;
    private int floor;
    private float floorHeight;
    private TGridObject[,] gridObjects;

    public int Width => width;
    public int Height => height;
    public GridSystem(int width, int height, float cellSize, int floor, float floorHeight, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.floor = floor;
        this.floorHeight = floorHeight;
        gridObjects = new TGridObject[width, height];
        for (int i = 0; i < this.width; ++i)
        {
            for (int j = 0; j < this.height; ++j)
            {
                var gridObject = createGridObject(this, new GridPosition(i, j, floor));
                gridObjects[i, j] = gridObject;
            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, 0, gridPosition.z) * cellSize + new Vector3(0, floor,0) * floorHeight;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        return new GridPosition(Mathf.RoundToInt(worldPosition.x / cellSize), Mathf.RoundToInt(worldPosition.z / cellSize), floor);
    }

    public void CreateDebugObjects(Transform debugPrefab)
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                GridPosition gridPosition = new GridPosition(i, j, floor);
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
        return gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < width && gridPosition.z < height &&
               gridPosition.floor == floor;
    }
}
