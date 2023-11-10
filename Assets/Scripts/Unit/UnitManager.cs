using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }
    private List<Unit> unitList;
    private List<Unit> friendUnitList;
    private List<Unit> enemyUnitList;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        unitList = new List<Unit>();
        friendUnitList = new List<Unit>();
        enemyUnitList = new List<Unit>();
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += OnAnyUnitDead;
    }

    private void OnDestroy()
    {
        Unit.OnAnyUnitSpawned -= OnAnyUnitSpawned;
        Unit.OnAnyUnitDead -= OnAnyUnitDead;
    }

    public IList<Unit> GetUnitList() => unitList;
    public IList<Unit> GetEnemyUnitList() => enemyUnitList;
    public IList<Unit> GetFriendUnitList() => friendUnitList;
    private void OnAnyUnitSpawned(object sender, EventArgs args)
    {
        Unit unit = sender as Unit;
        unitList.Add(unit);
        if (unit.IsEnemy())
        {
            enemyUnitList.Add(unit);
        }
        else
        {
            friendUnitList.Add(unit);
        }
    }

    private void OnAnyUnitDead(object sender, EventArgs args)
    {
        Unit unit = sender as Unit;
        unitList.Remove(unit);
        if (unit.IsEnemy())
        {
            enemyUnitList.Remove(unit);
        }
        else
        {
            friendUnitList.Remove(unit);
        }
    }
}
