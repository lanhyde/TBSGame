using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridSystemVisual : MonoBehaviour
{
    [FormerlySerializedAs("gridSystemVisualInstance")] [SerializeField] private Transform gridSystemVisualPrefab;
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

    public void ShowGridPositionList(IList<GridPosition> gridPositions)
    {
        foreach (var gridPosition in gridPositions)
        {
            gridSystemVisualInstances[gridPosition.x, gridPosition.z].Show();
        }
    }

    private void Update()
    {
        UpdateGridVisual();
    }

    private void UpdateGridVisual()
    {
        HideAllGridPosition();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        ShowGridPositionList(selectedAction.GetValidActionGridPositionList());
    }
}
