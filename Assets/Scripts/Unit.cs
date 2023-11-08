using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static event EventHandler OnAnyActionPointsChanged;
    
    private const int ACTION_POINTS_MAX = 2;
    private GridPosition gridPosition;
    private MoveAction moveAction;
    private SpinAction spinAction;
    private BaseAction[] baseActions;
    private HealthSystem healthSystem;
    private int actionPoints = ACTION_POINTS_MAX;
    
    [SerializeField] private bool isEnemy;
    
    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        spinAction = GetComponent<SpinAction>();
        baseActions = GetComponents<BaseAction>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += OnTurnChanged;
        healthSystem.OnDead += OnDead;
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
            LevelGrid.Instance.UnitMoveGridPosition(this, gridPosition, newGridPosition);
            gridPosition = newGridPosition;
        }
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<BaseAction> GetActions() => baseActions;
    public MoveAction GetMoveAction() => moveAction;
    public SpinAction GetSpinAction() => spinAction;

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
        Destroy(gameObject);
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
    }

    public void Damage(int damageAmount)
    {
       healthSystem.Damage(damageAmount);
    }
}
