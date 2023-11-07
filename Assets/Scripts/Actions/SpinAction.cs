using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{
    private float totalSpinAmount;
    public override void TakeAction(GridPosition gridPosition, Action callback)
    {
        totalSpinAmount = 0;
        ActionStart(callback);
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        float spinAmount = 360 * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAmount, 0);
        totalSpinAmount += spinAmount;
        if (totalSpinAmount >= 360f)
        {
            ActionComplete();
        }
    }

    public override string GetActionName() => "Spin";

    public override IList<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return new List<GridPosition>
        {
            unitGridPosition
        };
    }

    public override int GetActionPointsCost() => 2;
}