using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private float timer;

    private enum State
    {
        WaitingForEnemyTurn,
        TakingTurn,
        Busy
    }

    private State state;

    private void Awake()
    {
        state = State.WaitingForEnemyTurn;
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += OnTurnChanged;
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnTurnChanged -= OnTurnChanged;
    }

    private void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        switch (state)
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.TakingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    state = State.Busy;
                    if (TryTakeEnemyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        // no more enemies have actions, end enemy turn
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                break;
        }
    }

    private void OnTurnChanged(object sender, EventArgs args)
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        state = State.TakingTurn;
        timer = 2f;
    }

    private void SetStateTakingTurn()
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }
    private bool TryTakeEnemyAIAction(Action completionCallback) 
    {
        foreach (var enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if (TryTakeEnemyAIAction(enemyUnit, completionCallback))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action completionCallback)
    {
        var spinAction = enemyUnit.GetSpinAction();
        GridPosition actionGridPosition = enemyUnit.GetGridPosition();
        if (!spinAction.IsValidActionGridPosition(actionGridPosition))
        {
            return false;
        }

        if (!enemyUnit.TrySpendActionPointsToTakeAction(spinAction))
        {
            return false;
        }
        
        spinAction.TakeAction(actionGridPosition, completionCallback);
        return true;
    }
}
