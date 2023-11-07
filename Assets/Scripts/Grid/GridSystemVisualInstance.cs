using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisualInstance : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }
}
