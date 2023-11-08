using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdollSpawner : MonoBehaviour
{
    [SerializeField] private Transform regdollPrefab;
    [SerializeField] private Transform originalRootBone;
    private HealthSystem healthSystem;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnDead += OnDead;
    }

    private void OnDestroy()
    {
        healthSystem.OnDead -= OnDead;
    }

    private void OnDead(object sender, EventArgs args)
    {
        Transform ragdollTransform = Instantiate(regdollPrefab, transform.position, transform.rotation);
        var unitRagdoll = ragdollTransform.GetComponent<UnitRagdoll>();
        unitRagdoll.Setup(originalRootBone);
    }
}
