using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }
    [SerializeField] private Transform gridDebugObjectPrefab;
    private GridSystemHex<GridObject> _gridSystemHex;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

    public int Width => width;
    public int Height => height;
    public event EventHandler OnAnyUnitMovedGridPosition;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
        }
        _gridSystemHex = new GridSystemHex<GridObject>(width, height, cellSize, 
            (GridSystemHex<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
        // gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }

    private void Start()
    {
        PathFinding.Instance.Setup(width, height, cellSize);
    }
    
    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = _gridSystemHex.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
        
    }

    // public IList<Unit> GetUnitsAtGridPosition(GridPosition gridPosition)
    // {
    //     GridObject gridObject = gridSystem.GetGridObject(gridPosition);
    //     return gridObject.GetUnits();
    // }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = _gridSystemHex.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);
        
        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => _gridSystemHex.GetGridPosition(worldPosition);
    public bool IsValidGridPosition(GridPosition gridPosition) => _gridSystemHex.IsValidGridPosition(gridPosition);
    public Vector3 GetWorldPosition(GridPosition gridPosition) => _gridSystemHex.GetWorldPosition(gridPosition);
    public (int, int) GetSize() => (_gridSystemHex.Width, _gridSystemHex.Height);
    
    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystemHex.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystemHex.GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public Interactable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = _gridSystemHex.GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }
    
    public void SetInteractableAtGridPosition(GridPosition gridPosition, Interactable interactable)
    {
        GridObject gridObject = _gridSystemHex.GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }
}
