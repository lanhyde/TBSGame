using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractAction : BaseAction
{
    public const int MaxInteractDistance = 1;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }
    }

    public override string GetActionName()
    {
        return "Interact";
    }

    public override void TakeAction(GridPosition gridPosition, Action completion)
    {
        Interactable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(gridPosition);
        interactable.Interact(OnInteractComplete);
        ActionStart(completion);
    }

    public override IList<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -MaxInteractDistance; x <= MaxInteractDistance; ++x)
        {
            for (int z = -MaxInteractDistance; z <= MaxInteractDistance; ++z)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                Interactable door = LevelGrid.Instance.GetInteractableAtGridPosition(testGridPosition);

                if (door == null)
                {
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
            actionValue = 0
        };
    }

    private void OnInteractComplete()
    {
        onActionComplete();
    }
}
