using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFindingLinkMonoBehaviour))]
public class PathfindingLinkMonoBehaviourEditor : Editor
{
    private void OnSceneGUI()
    {
        PathFindingLinkMonoBehaviour pathFindingLinkMonoBehaviour = (PathFindingLinkMonoBehaviour)target;

        EditorGUI.BeginChangeCheck();
        Vector3 newLinkPositionA = Handles.PositionHandle(pathFindingLinkMonoBehaviour.linkPositionA, Quaternion.identity);
        Vector3 newLinkPositionB = Handles.PositionHandle(pathFindingLinkMonoBehaviour.linkPositionB, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pathFindingLinkMonoBehaviour, "Change List Position");
            pathFindingLinkMonoBehaviour.linkPositionA = newLinkPositionA;
            pathFindingLinkMonoBehaviour.linkPositionB = newLinkPositionB;
        }
    }
}
