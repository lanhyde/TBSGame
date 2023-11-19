using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAction : BaseAction
{
    public static event EventHandler OnAnySwordHit;
    public event EventHandler OnSwordActionStarted;
    public event EventHandler OnSwordActionCompleted;
    
    public const int MaxSwordDistance = 1;

    private enum State
    {
        SwingSwordBeforeHit,
        SwingSwordAfterHit
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.SwingSwordBeforeHit:
                float angularSpeed = 10f;
                Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * angularSpeed);
                break;
            case State.SwingSwordAfterHit:
                break;
        }
        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    public override string GetActionName()
    {
        return "Sword";
    }

    public override void TakeAction(GridPosition gridPosition, Action completion)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.SwingSwordBeforeHit;
        float beforeHitsStateTime = 0.5f;
        stateTimer = beforeHitsStateTime;
        OnSwordActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(completion);
    }

    public override IList<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -MaxSwordDistance; x <= MaxSwordDistance; ++x)
        {
            for (int z = -MaxSwordDistance; z <= MaxSwordDistance; ++z)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    // Both units on same team
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 200
        };
    }
    
    private void NextState()
    {
        switch (state)
        {
            case State.SwingSwordBeforeHit:
                state = State.SwingSwordAfterHit;
                float afterHitsStateTime = 0.1f;
                stateTimer = afterHitsStateTime;
                OnAnySwordHit?.Invoke(this, EventArgs.Empty);
                targetUnit.Damage(100);
                break;
            case State.SwingSwordAfterHit:
                OnSwordActionCompleted?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }
    }
}
