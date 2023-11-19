using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingUpdater : MonoBehaviour
{
    private void Start()
    {
        DestructableCrate.OnAnyDestroyed += OnAnyDestroyed;
    }

    private void OnDestroy()
    {
        DestructableCrate.OnAnyDestroyed -= OnAnyDestroyed;
    }

    void OnAnyDestroyed(object sender, EventArgs args)
    {
        DestructableCrate destrutableCrate = sender as DestructableCrate;
        PathFinding.Instance.SetIsWalkableGridPosition(destrutableCrate.GetGridPosition(), true);
    }
}
