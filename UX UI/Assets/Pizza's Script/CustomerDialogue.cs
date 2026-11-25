using System.Collections;
using UnityEngine;
using TMPro;

public class CustomerDialogue : MonoBehaviour
{
    [Header("References")]
    public UISpawner candySpawner;        
    public CustomerPayment payment;      
    public GameObject dialogueBox;        
    public TMP_Text dialogueText;         

    [Header("Dialogue lines")]
    [Tooltip("Use {n} where the number of candies should appear.")]
    [TextArea(2, 4)]
    public string[] lines =
    {
        "Hello! Can I get {n} candies please?",
        "Here is my money!"
    };

    [Header("Timing")]
    public float charsPerSecond = 30f;   
    public float pauseBetweenLines = 1.0f;
    public float pauseAfterLastLine = 0.5f;
    public bool hideBoxWhenDone = true;

    Coroutine running;

    void OnEnable()
    {
        if (candySpawner) candySpawner.OnSpawned += StartDialogue;
        else Debug.LogError("CustomerDialogue: Candy Spawner is not assigned.");
    }

    void OnDisable()
    {
        if (candySpawner) candySpawner.OnSpawned -= StartDialogue;
    }

    void Start()
    {
        if (dialogueBox) dialogueBox.SetActive(false);

        if (candySpawner && candySpawner.LastSpawnCount > 0 && running == null)
            StartDialogue(candySpawner.LastSpawnCount);
    }

    public void StartDialogue(int candyCount)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PlayDialogue(candyCount));
    }

    Coroutine sayRoutine;

    public void Say(string message, float showSeconds = 2f)
    {
        if (!dialogueText) return;
        if (sayRoutine != null) StopCoroutine(sayRoutine);
        sayRoutine = StartCoroutine(SayRoutine(message, showSeconds));
    }

    IEnumerator SayRoutine(string message, float showSeconds)
    {
        if (dialogueBox) dialogueBox.SetActive(true);
        dialogueText.text = message;

        yield return new WaitForSeconds(showSeconds);

        if (hideBoxWhenDone && dialogueBox) dialogueBox.SetActive(false);
        sayRoutine = null;
    }

    IEnumerator PlayDialogue(int candyCount)
    {
        if (!dialogueText)
        {
            Debug.LogError("CustomerDialogue: Dialogue Text is not assigned.");
            yield break;
        }

        if (dialogueBox) dialogueBox.SetActive(true);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Replace("{n}", candyCount.ToString());
            dialogueText.text = "";

            float delay = 1f / Mathf.Max(1f, charsPerSecond);
            for (int c = 0; c < line.Length; c++)
            {
                dialogueText.text += line[c];
                yield return new WaitForSeconds(delay);
            }

            bool last = (i == lines.Length - 1);
            yield return new WaitForSeconds(last ? pauseAfterLastLine : pauseBetweenLines);
        }

        if (hideBoxWhenDone && dialogueBox) dialogueBox.SetActive(false);

        if (payment) payment.OnDialogueFinished();
        else Debug.LogWarning("CustomerDialogue: Payment is not assigned, no money will appear.");

        running = null;
    }
}