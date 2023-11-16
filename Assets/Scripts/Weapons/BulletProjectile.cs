using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform bulletHitVfxPrefab;
    private Vector3 targetPosition;
    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private void Update()
    {
        Vector3 moveDir = (targetPosition - transform.position).normalized;
        float moveSpeed = 200f;
        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);
        
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);
        if (distanceBeforeMoving < distanceAfterMoving)
        {
            transform.position = targetPosition;
            trailRenderer.transform.SetParent(null);
            Destroy(gameObject);

            Instantiate(bulletHitVfxPrefab, targetPosition, Quaternion.identity);
        }
    }
}
