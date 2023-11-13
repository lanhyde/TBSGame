using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PathFindingGridDebugObject : GridDebugObject
{
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;
    [SerializeField] private SpriteRenderer isWalkableSpriteRenderer;
    private PathNode pathNode;
    public override void SetGridObject(object gridObject)
    {
        base.SetGridObject(gridObject);
        pathNode = gridObject as PathNode;
    }

    protected override void Update()
    {
        base.Update();
        gCostText.text = pathNode.GCost.ToString();
        hCostText.text = pathNode.HCost.ToString();
        fCostText.text = pathNode.FCost.ToString();
        isWalkableSpriteRenderer.color = pathNode.IsWalkable ? Color.green : Color.red;

    }
}
