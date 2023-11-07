using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [CanBeNull] private MoveAction moveAction;
    [CanBeNull] private ShootAction shootAction;
    private void Awake()
    {
        if (TryGetComponent(out moveAction))
        {
            moveAction.OnStartMoving += OnStartMoving;
            moveAction.OnStopMoving += OnStopMoving;
        }

        if (TryGetComponent(out shootAction))
        {
            shootAction.OnShoot += OnShoot;
        }
    }

    private void OnDestroy()
    {
        if (moveAction)
        {
            moveAction.OnStartMoving -= OnStartMoving;
            moveAction.OnStopMoving -= OnStopMoving;
        }

        if (shootAction)
        {
            shootAction.OnShoot -= OnShoot;
        }
    }

    private void OnStartMoving(object sender, EventArgs args)
    {
        animator.SetBool("IsWalking", true);
    }

    private void OnStopMoving(object sender, EventArgs args)
    {
        animator.SetBool("IsWalking", false);
    }

    private void OnShoot(object sender, EventArgs args)
    {
        animator.SetTrigger("Shoot");
    }
}
