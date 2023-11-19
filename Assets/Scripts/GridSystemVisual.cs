using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridSystemVisual : MonoBehaviour
{
    public enum GridVisualType
    {
        White = 0, Blue = 1, Red = 2, RedSoft = 4, Yellow = 3
    }

    [Serializable]
    public struct GridVisualTypeMaterial
    {
        public GridVisualType gridVisualType;
        public Material material;
    }
    
    
    [FormerlySerializedAs("gridSystemVisualInstance")] [SerializeField] private Transform gridSystemVisualPrefab;
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterials;
    
    private GridSystemVisualInstance[,] gridSystemVisualInstances;
    
    public static GridSystemVisual Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("There's more than one GridSystemVisual! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        var (width, height) = LevelGrid.Instance.GetSize();
        gridSystemVisualInstances = new GridSystemVisualInstance[width, height];
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSystemVisualInstanceTransform = Instantiate(gridSystemVisualPrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                gridSystemVisualInstances[x, z] =
                    gridSystemVisualInstanceTransform.GetComponent<GridSystemVisualInstance>();
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChanged += OnSelectActionChanged;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += OnAnyUnitMovedGridPosition;
        
        UpdateGridVisual();
    }

    private void OnDestroy()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged -= OnSelectActionChanged;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition -= OnAnyUnitMovedGridPosition;
    }

    public void HideAllGridPosition()
    {
        var (width, height) = LevelGrid.Instance.GetSize();
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                gridSystemVisualInstances[x, z].Hide();
            }
        }
    }

    public void ShowGridPositionList(IList<GridPosition> gridPositions, GridVisualType gridVisualType)
    {
        foreach (var gridPosition in gridPositions)
        {
            gridSystemVisualInstances[gridPosition.x, gridPosition.z].Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; ++x)
        {
            for (int z = -range; z <= range; ++z)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > range)
                {
                    continue;
                }
                gridPositionList.Add(testGridPosition);
            }
        }    
        ShowGridPositionList(gridPositionList, gridVisualType);
    }
    
    private void ShowGridPositionRangeSquare(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; ++x)
        {
            for (int z = -range; z <= range; ++z)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                gridPositionList.Add(testGridPosition);
            }
        }    
        ShowGridPositionList(gridPositionList, gridVisualType);
    }
    
    private void UpdateGridVisual()
    {
        HideAllGridPosition();
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        GridVisualType gridVisualType;
        switch (selectedAction)
        {
            default:
            case MoveAction:
                gridVisualType = GridVisualType.White;
                break;
            case SpinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case ShootAction:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), ShootAction.MaxShootDistance, GridVisualType.RedSoft);
                break;
            case GrenadeAction:
                gridVisualType = GridVisualType.Yellow;
                break;
            case SwordAction:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), SwordAction.MaxSwordDistance, GridVisualType.RedSoft);
                break;
        }
        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
    }

    private void OnSelectActionChanged(object sender, EventArgs args)
    {
        UpdateGridVisual();
    }

    private void OnAnyUnitMovedGridPosition(object sender, EventArgs args)
    {
        UpdateGridVisual();
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (var gridVisualTypeMaterial in gridVisualTypeMaterials)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }
        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        return null;
    }
}
