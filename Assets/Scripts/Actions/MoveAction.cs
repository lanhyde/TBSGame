using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    
    [SerializeField] private int maxMoveDistance = 4;
    private Vector3 targetPosition;
    protected override void Awake()
    {
        base.Awake();
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        float stoppingDistance = .1f;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            OnStopMoving?.Invoke(this, EventArgs.Empty);
            ActionComplete();
        }
        float angularSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, angularSpeed * Time.deltaTime);
    }


    public override void TakeAction(GridPosition gridPosition, Action completeAction)
    {
        this.targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        OnStartMoving?.Invoke(this, EventArgs.Empty);
        ActionStart(completeAction);
    }
    
    public override IList<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        
        for (int x = -maxMoveDistance; x <= maxMoveDistance; ++x)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; ++z)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (unitGridPosition == testGridPosition)
                {
                    continue;
                }

                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }
        
        return validGridPositionList;
    }

    public override string GetActionName() => "Move";
    
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetShootAction().GetTargetCountAtPosition(gridPosition);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPosition * 10
        };
    }
}
