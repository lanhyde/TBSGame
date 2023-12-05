using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorVisibility : MonoBehaviour
{
    [SerializeField] private bool isDynamicFloorPosition;
    [SerializeField] private List<Renderer> ignoredRenderers;
    private Renderer[] renderers;
    private int floor;
    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Start()
    {
        floor = LevelGrid.Instance.GetFloor(transform.position);
        
        if (floor == 0 && !isDynamicFloorPosition)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (isDynamicFloorPosition)
        {
            floor = LevelGrid.Instance.GetFloor(transform.position);
        }
        float cameraHeight = CameraController.Instance.GetCameraHeight();
        float floorHeightOffset = 2f;
        bool showObject = cameraHeight > LevelGrid.FLOOR_HEIGHT * floor + floorHeightOffset;

        if (showObject || floor == 0)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (var renderer in renderers)
        {
            if (ignoredRenderers.Contains(renderer))
            {
                continue;
            }
            renderer.enabled = true;
        }
    }

    private void Hide()
    {
        foreach (var renderer in renderers)
        {
            if (ignoredRenderers.Contains(renderer))
            {
                continue;
            }
            renderer.enabled = false;
        }
    }
}
