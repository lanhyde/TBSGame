using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    private static MouseWorld instance;
    [SerializeField] private LayerMask mousePlaneLayerMask;

    private void Awake()
    {
        instance = this;
    }

    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, instance.mousePlaneLayerMask);
        return hit.point;
    }

    public static Vector3 GetPositionOnlyHitVisible()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        RaycastHit[] raycasts = Physics.RaycastAll(ray, float.MaxValue, instance.mousePlaneLayerMask);
        Array.Sort(raycasts, (raycastA, raycastB) =>
        {
            return Mathf.RoundToInt(raycastA.distance - raycastB.distance);
        });
        foreach (var raycast in raycasts)
        {
            if (raycast.transform.TryGetComponent(out Renderer renderer))
            {
                if (renderer.enabled)
                {
                    return raycast.point;
                }
            }
        }
        return Vector3.zero;
    }
}
