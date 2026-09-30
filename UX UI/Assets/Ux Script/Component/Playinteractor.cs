using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Playerinteractor : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float interactionDistance;

    [SerializeField] private InputActionReference interactionReference;

    private void OnEnable()
    {
        interactionReference.action.Enable();
        interactionReference.action.started += Playerinteracted;
    }
    private void OnDisable()
    {
        interactionReference.action.Disable();

    }
}
