using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitActionSystemUI : MonoBehaviour
{
    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform actionButtonContainerTransform;
    [SerializeField] private TextMeshProUGUI actionPointsText;
    private IList<ActionButtonUI> actionButtons;

    private void Awake()
    {
        actionButtons = new List<ActionButtonUI>();
    }

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged += OnSelectedActionChanged;
        UnitActionSystem.Instance.OnActionStarted += OnActionStarted;
        TurnSystem.Instance.OnTurnChanged += OnTurnChanged;
        Unit.OnAnyActionPointsChanged += OnUnitAnyActionPointsChanged;
        
        UpdateActionPoints();
        CreateUnitActionButtons();   
        UpdateSelectedVisual();
    }

    private void OnDestroy()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged -= OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged -= OnSelectedActionChanged;
        UnitActionSystem.Instance.OnActionStarted -= OnActionStarted;
        TurnSystem.Instance.OnTurnChanged -= OnTurnChanged;
        Unit.OnAnyActionPointsChanged -= OnUnitAnyActionPointsChanged;
    }

    private void CreateUnitActionButtons()
    {
        foreach (Transform buttonTransform in actionButtonContainerTransform)
        {
            Destroy(buttonTransform.gameObject);
        }
        actionButtons.Clear();
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        foreach (var action in selectedUnit.GetActions())
        {
            Transform actionButtonTransform = Instantiate(actionButtonPrefab, actionButtonContainerTransform);
            ActionButtonUI actionButtonUI = actionButtonTransform.GetComponent<ActionButtonUI>();
            actionButtonUI.SetBaseAction(action);
            actionButtons.Add(actionButtonUI);
        }
    }

    private void OnSelectedUnitChanged(object sender, EventArgs args)
    {
        CreateUnitActionButtons();
        UpdateSelectedVisual();
        UpdateActionPoints();
    }

    private void OnSelectedActionChanged(object sender, EventArgs args)
    {
        UpdateSelectedVisual();
    }

    private void OnActionStarted(object sender, EventArgs args)
    {
        UpdateActionPoints();
    }

    private void OnTurnChanged(object sender, EventArgs args)
    {
        UpdateActionPoints();
    }

    private void OnUnitAnyActionPointsChanged(object sender, EventArgs args)
    {
        UpdateActionPoints();
    }

    private void UpdateSelectedVisual()
    {
        foreach (var actionButton in actionButtons)
        {
            actionButton.UpdateSelectedVisual();
        }
    }

    private void UpdateActionPoints()
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        actionPointsText.text = $"Action Points: {selectedUnit.GetActionPoints()}";
    }
}
