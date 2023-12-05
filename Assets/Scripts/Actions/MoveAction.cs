using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    public event EventHandler<OnChangeFloorsStartedEventArgs> OnChangedFloorStarted;

    public class OnChangeFloorsStartedEventArgs : EventArgs
    {
        public GridPosition unitGridPosition;
        public GridPosition targetGridPosition;
    }

    [SerializeField] private int maxMoveDistance = 4;
    private List<Vector3> positionList;
    private int currentPositionIndex;
    private bool isChangingFloors;
    private float differentFloorsTeleportTimer;
    private float differentFloorsTeleportTimeMax = .5f;
    
    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        Vector3 targetPosition = positionList[currentPositionIndex];
        if (isChangingFloors)
        {
            // Stop and teleport
            Vector3 targetSameFloorPosition = targetPosition;
            targetSameFloorPosition.y = transform.position.y;
            Vector3 rotateDirection = (targetSameFloorPosition - transform.position).normalized;
            
            float angularSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, rotateDirection, angularSpeed * Time.deltaTime);
            
            differentFloorsTeleportTimer -= Time.deltaTime;
            if (differentFloorsTeleportTimer < 0f)
            {
                isChangingFloors = false;
                transform.position = targetPosition;
            }
        }
        else
        {
            Vector3 moveDirection = (targetPosition - transform.position).normalized;

            float angularSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, angularSpeed * Time.deltaTime);
            
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        float stoppingDistance = .1f;
        
        if (Vector3.Distance(transform.position, targetPosition) < stoppingDistance)
        {
            ++currentPositionIndex;
            if (currentPositionIndex >= positionList.Count)
            {
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }
            else
            {
                targetPosition = positionList[currentPositionIndex];
                GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition);
                GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

                if (targetGridPosition.floor != unitGridPosition.floor)
                {
                    // Different floors
                    isChangingFloors = true;
                    differentFloorsTeleportTimer = differentFloorsTeleportTimeMax;
                    
                    OnChangedFloorStarted?.Invoke(this, new OnChangeFloorsStartedEventArgs
                    {
                        unitGridPosition = unitGridPosition,
                        targetGridPosition = targetGridPosition
                    });
                }
            }
        }
    }


    public override void TakeAction(GridPosition gridPosition, Action completeAction)
    {
        var pathGridPositionList = PathFinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out _);

        currentPositionIndex = 0;
        positionList = new List<Vector3>();
        positionList.AddRange(pathGridPositionList.Select(grid => LevelGrid.Instance.GetWorldPosition(grid)));
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
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; ++floor)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, floor);
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

                    if (!PathFinding.Instance.IsWalkableGridPosition(testGridPosition))
                    {
                        continue;
                    }

                    if (!PathFinding.Instance.HasPath(unitGridPosition, testGridPosition))
                    {
                        continue;
                    }

                    int pathFindingMultiplier = 10;
                    if (PathFinding.Instance.GetPathLength(unitGridPosition, testGridPosition) >
                        maxMoveDistance * pathFindingMultiplier)
                    {
                        // path length is too long
                        continue;
                    }

                    validGridPositionList.Add(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName() => "Move";

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPosition * 10
        };
    }
}