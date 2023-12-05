using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float MIN_FOLLOW_Y_OFFSET = 2f;
    private const float MAX_FOLLOW_Y_OFFSET = 15f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private Vector3 targetFollowOffset;

    private CinemachineTransposer transposer;
    
    public static CameraController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = transposer.m_FollowOffset;
    }

    // Update is called once per frame
    void Update()
    {
        HandleTranslation();
        HandleRotation();
        HandleZoom();
    }

    private void HandleZoom()
    {
        float zoomIncreaseAmount = 1f;
        targetFollowOffset.y += InputManager.Instance.GetCameraZoomAmount() * zoomIncreaseAmount;

        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_Y_OFFSET, MAX_FOLLOW_Y_OFFSET);
        float zoomSpeed = 5f;
        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
    }

    private void HandleRotation()
    {
        Vector3 rotationVector = Vector3.zero;
        rotationVector.y = InputManager.Instance.GetCameraRotateAmount();

        float angularSpeed = 100f;
        transform.eulerAngles += rotationVector * angularSpeed * Time.deltaTime;
    }

    private void HandleTranslation()
    {
        Vector2 inputMoveDir = InputManager.Instance.GetCameraMoveVector();

        float moveSpeed = 10f;
        Vector3 moveVector = transform.forward * inputMoveDir.y + transform.right * inputMoveDir.x;
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    public float GetCameraHeight()
    {
        return targetFollowOffset.y;
    }
}
