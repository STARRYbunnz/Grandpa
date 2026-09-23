using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public void interact()
    {
        Debug.Log("Interacting...");
        gameObject.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

    }
}