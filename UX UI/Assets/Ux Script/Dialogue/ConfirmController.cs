using UnityEngine;
using UnityEngine.UI;

public class ConfirmController : MonoBehaviour
{
    public GameObject confirmPanel;
    public DialogueManager dialogue;

    void Awake()
    {
        var img = confirmPanel.GetComponent<Image>();
        if (!img) img = confirmPanel.AddComponent<Image>();

        var btn = confirmPanel.GetComponent<Button>();
        if (!btn) btn = confirmPanel.AddComponent<Button>();
        btn.targetGraphic = img;

        btn.onClick.AddListener(() =>
        {
            Debug.Log ("Click Confirm");
            confirmPanel.SetActive(false);
            dialogue.StartDialogue();
        });

        confirmPanel.SetActive(false);
    }

    public void ShowConfirm()
    {
        confirmPanel.SetActive(true);
    }
}