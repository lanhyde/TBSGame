using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private HealthSystem healthSystem;
    private void Start()
    {
        Unit.OnAnyActionPointsChanged += OnAnyActionPointsChanged;
        healthSystem.OnDamaged += OnDamaged;
        UpdateActionPointsText();
        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        Unit.OnAnyActionPointsChanged -= OnAnyActionPointsChanged;
        healthSystem.OnDamaged -= OnDamaged;
    }

    private void UpdateActionPointsText()
    {
        actionPointsText.text = unit.GetActionPoints().ToString();
    }

    private void OnAnyActionPointsChanged(object sender, EventArgs args)
    {
        UpdateActionPointsText();
    }

    private void OnDamaged(object sender, EventArgs args)
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBarImage.fillAmount = healthSystem.GetHealthNormalized();
    }
}
