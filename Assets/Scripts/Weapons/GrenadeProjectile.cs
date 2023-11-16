using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private Action grenadeExploded;
    private void Update()
    {
        Vector3 moveDir = (targetPosition - transform.position).normalized;
        float moveSpeed = 15f;
        transform.position += moveDir * (moveSpeed * Time.deltaTime);

        float reachedTargetDistance = .2f;
        if (Vector3.Distance(transform.position, targetPosition) < reachedTargetDistance)
        {
            float damageRadius = 4f;
            Collider[] colliders = Physics.OverlapSphere(targetPosition, damageRadius);
            foreach (Collider collider in colliders)
            {
                if(collider.TryGetComponent(out Unit targetUnit))
                {
                    targetUnit.Damage(30);
                }
            }
            Destroy(gameObject);
            grenadeExploded?.Invoke();
        }
    }

    public void Setup(GridPosition targetGridPosition, Action onGrenadeExploded)
    {
        this.grenadeExploded = onGrenadeExploded;
        this.targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
    }
}
