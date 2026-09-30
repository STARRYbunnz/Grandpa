using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    public DialogueManager dialogue;
    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Change"))
        {
            triggered = true;
            dialogue.StartDialogue();
        }
    }
}
