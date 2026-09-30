using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ConfirmController : MonoBehaviour
{
    public ConfirmController confirm;
    public GameObject confirmPanel;
    bool used;
    public DialogueManager dialogue;

    void Awake()
    {
        confirmPanel.SetActive(false);

    }

    public void ShowConfirm()
    {
        confirmPanel.SetActive(true);

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            used = true;
            confirm.ShowConfirm();
        }

    }

}

