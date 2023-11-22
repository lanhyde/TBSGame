using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, Interactable
{
    private GridPosition gridPosition;
    private Animator animator;
    private Action onComplete;
    private float timer;
    private bool isActive;
    
    [SerializeField] private bool isOpen;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetInteractableAtGridPosition(gridPosition, this);

        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isActive = false;
            onComplete();
        }
    }

    public void Interact(Action completion)
    {
        this.onComplete = completion;
        isActive = true;
        timer = .5f;
        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        animator.SetBool("IsOpen", isOpen);
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, true);
    }

    private void CloseDoor()
    {
        isOpen = false;
        animator.SetBool("IsOpen", isOpen);
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
    }
}
