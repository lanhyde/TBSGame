using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform rifleTransform;
    [SerializeField] private Transform swordTransform;
    
    [CanBeNull] private MoveAction moveAction;
    [CanBeNull] private ShootAction shootAction;
    [CanBeNull] private SwordAction swordAction;
    private void Awake()
    {
        if (TryGetComponent(out moveAction))
        {
            moveAction.OnStartMoving += OnStartMoving;
            moveAction.OnStopMoving += OnStopMoving;
            moveAction.OnChangedFloorStarted += OnChangedFloorsStarted;
        }

        if (TryGetComponent(out shootAction))
        {
            shootAction.OnShoot += OnShoot;
        }

        if (TryGetComponent(out swordAction))
        {
            swordAction.OnSwordActionStarted += OnSwordActionStarted;
            swordAction.OnSwordActionCompleted += OnSwordActionCompleted;
        }
    }

    private void Start()
    {
        EquipRifle();
    }

    private void OnDestroy()
    {
        if (moveAction)
        {
            moveAction.OnStartMoving -= OnStartMoving;
            moveAction.OnStopMoving -= OnStopMoving;
            moveAction.OnChangedFloorStarted -= OnChangedFloorsStarted;
        }

        if (shootAction)
        {
            shootAction.OnShoot -= OnShoot;
        }

        if (swordAction)
        {
            swordAction.OnSwordActionStarted -= OnSwordActionStarted;
            swordAction.OnSwordActionCompleted -= OnSwordActionCompleted;
        }
    }

    private void OnChangedFloorsStarted(object sender, MoveAction.OnChangeFloorsStartedEventArgs e)
    {
        if (e.targetGridPosition.floor > e.unitGridPosition.floor)
        {
            animator.SetTrigger("JumpUp");
        }
        else
        {
            animator.SetTrigger("JumpDown");
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

    private void OnShoot(object sender, ShootAction.OnShootEventArgs args)
    {
        animator.SetTrigger("Shoot");
        Transform bulletTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletTransform.GetComponent<BulletProjectile>();
        Vector3 targetUnitShootAtPosition = args.targetUnit.GetWorldPosition();
        float unitShoulderHeight = 1.7f;
        
        targetUnitShootAtPosition.y += unitShoulderHeight;
        bulletProjectile.Setup(targetUnitShootAtPosition);
    }

    private void OnSwordActionStarted(object sender, EventArgs e)
    {
        EquipSword();
        animator.SetTrigger("SwordSlash");
    }

    private void OnSwordActionCompleted(object sender, EventArgs e)
    {
        EquipRifle();
    }

    private void EquipSword()
    {
        swordTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);
    }

    private void EquipRifle()
    {
        swordTransform.gameObject.SetActive(false);
        rifleTransform.gameObject.SetActive(true);
    }
}
