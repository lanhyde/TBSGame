using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;
    
    private const int ACTION_POINTS_MAX = 2;
    private GridPosition gridPosition;
    private BaseAction[] baseActions;
    private HealthSystem healthSystem;
    private int actionPoints = ACTION_POINTS_MAX;
    
    [SerializeField] private bool isEnemy;
    
    private void Awake()
    {
        baseActions = GetComponents<BaseAction>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += OnTurnChanged;
        healthSystem.OnDead += OnDead;
        
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnTurnChanged -= OnTurnChanged;
        healthSystem.OnDead -= OnDead;
    }

    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition)
        {
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public T GetAction<T>() where T: BaseAction
    {
        foreach (var baseAction in baseActions)
        {
            if (baseAction is T action)
            {
                return action;
            }
        }

        return null;
    }
    public IEnumerable<BaseAction> GetActions() => baseActions;
    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction) =>
        actionPoints >= baseAction.GetActionPointsCost();
    public GridPosition GetGridPosition() => gridPosition;
    public Vector3 GetWorldPosition() => transform.position;
    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }

        return false;
    }

    public int GetActionPoints() => actionPoints;
    public bool IsEnemy() => isEnemy;
    private void OnTurnChanged(object sender, EventArgs args)
    {
        if ((isEnemy && !TurnSystem.Instance.IsPlayerTurn()) || 
            (!isEnemy && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = ACTION_POINTS_MAX;
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    private void OnDead(object sender, EventArgs args)
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        Destroy(gameObject);
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void Damage(int damageAmount)
    {
       healthSystem.Damage(damageAmount);
       
    }

    public float GetHealthNormalized() => healthSystem.GetHealthNormalized();
}
