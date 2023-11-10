using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;
    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;
    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract string GetActionName();
    public abstract void TakeAction(GridPosition gridPosition, Action completion);

    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        var validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }

    public abstract IList<GridPosition> GetValidActionGridPositionList();

    public virtual int GetActionPointsCost() => 1;

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;
        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete?.Invoke();
        
        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit() => unit;

    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActions = new List<EnemyAIAction>();
        IList<GridPosition> validActionGridPositions = GetValidActionGridPositionList();
        foreach (var gridPosition in validActionGridPositions)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActions.Add(enemyAIAction);
        }

        if (enemyAIActions.Count <= 0) return null;
        enemyAIActions.Sort((a, b) => b.actionValue - a.actionValue);
        return enemyAIActions.First();

    }

    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);
}
