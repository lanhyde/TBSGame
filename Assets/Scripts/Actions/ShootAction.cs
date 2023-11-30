using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    private enum State { Aiming, Shooting, Cooloff }
    public override string GetActionName() => "Shoot";
    public const int MaxShootDistance = 5;
    [SerializeField] private LayerMask obstaclesLayer;
    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private bool canShootBullet;

    public event EventHandler<OnShootEventArgs> OnShoot;
    public static event EventHandler<OnShootEventArgs> OnAnyShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }
    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aiming:
                float angularSpeed = 10f;
                Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * angularSpeed);
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }
        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    public Unit GetTargetUnit() => targetUnit;
    
    public override void TakeAction(GridPosition gridPosition, Action completion)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        
        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;
        canShootBullet = true;
        
        ActionStart(completion);
    }

    public override IList<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }
    
    public IList<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        
        for (int x = -MaxShootDistance; x <= MaxShootDistance; ++x)
        {
            for (int z = -MaxShootDistance; z <= MaxShootDistance; ++z)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > MaxShootDistance)
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

                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;
                float unitShoulderHeight = 1.7f;
                if (Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight, shootDir, 
                        Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()), obstaclesLayer))
                {
                    // Blocked by obstacles
                    continue;
                }
                validGridPositionList.Add(testGridPosition);
            }
        }
        
        return validGridPositionList;
    }

    private void Shoot()
    {
        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit
        });
        OnAnyShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit
        });
        targetUnit.Damage(40);
    }
    
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f)
        };
    }
    
    public int GetTargetCountAtPosition(GridPosition gridPosition) => GetValidActionGridPositionList(gridPosition).Count;
}
