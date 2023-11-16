using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction
{
    private const int MaxThrowDistance = 7;
    [SerializeField] private Transform grenadeProjectilePrefab;
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
    }

    public override string GetActionName() => "Grenade";
    public override void TakeAction(GridPosition gridPosition, Action completion)
    {
        Transform grenadeProjectileTransform = Instantiate(grenadeProjectilePrefab, unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeExploded);
        
        ActionStart(completion);
    }

    public override IList<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -MaxThrowDistance; x <= MaxThrowDistance; ++x)
        {
            for (int z = -MaxThrowDistance; z <= MaxThrowDistance; ++z)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > MaxThrowDistance)
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

    private void OnGrenadeExploded()
    {
        ActionComplete();
    }
}
