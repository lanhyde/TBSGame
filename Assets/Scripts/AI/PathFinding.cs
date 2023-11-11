using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [SerializeField] private Transform gridDebugObjectPrefab;
    
    private int width;
    private int height;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;
    private void Awake()
    {
        gridSystem =
            new GridSystem<PathNode>(10, 10, 2f, (_, gridPosition) => new PathNode(gridPosition));
        gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }
}
