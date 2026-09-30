using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    [TextArea(2, 5)] public string[] lines;
    public float typeSpeed = 0.03f;

    int index;
    bool isTyping;
    Coroutine typingRoutine;

    public void StartDialogue()
    {
        index = 0;
        dialoguePanel.SetActive(true);
        Time.timeScale = 0f; // optional: pause the game
        ShowLine();
    }

    void ShowLine()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed); // realtime works while paused
        }
        isTyping = false;
    }

    // Hook this to your Next button, or call it on key press
    public void Next()
    {
        if (isTyping)
        {
            StopCoroutine(typingRoutine);
            dialogueText.text = lines[index];
            isTyping = false;
            return;
        }

        index++;
        if (index < lines.Length) ShowLine();
        else EndDialogue();
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
        // load credits, main menu, etc.
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
            Next();
    }
}