using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UnityLinker;
using UnityEngine;

public class GridObject
{
    private GridPosition gridPosition;
    private GridSystemHex<GridObject> _gridSystemHex;
    private List<Unit> units;
    private Interactable interactable;
    public GridObject(GridSystemHex<GridObject> gridSystemHex, GridPosition gridPosition)
    {
        this._gridSystemHex = gridSystemHex;
        this.gridPosition = gridPosition;
        units = new List<Unit>();
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

    // public IList<Unit> GetUnits()
    // {
    //     return units;
    // }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (var unit in units)
        {
            unitString += unit + Environment.NewLine;
        }
        return gridPosition.ToString() + Environment.NewLine + unitString;
    }

    public bool HasAnyUnit()
    {
        return units.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return units[0];
        }

        return null;
    }

    public Interactable GetInteractable()
    {
        return interactable;
    }

    public void SetInteractable(Interactable door)
    {
        this.interactable = door;
    }
}
