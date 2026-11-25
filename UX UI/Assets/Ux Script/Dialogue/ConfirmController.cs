using UnityEngine;
using UnityEngine.UI;

public class ConfirmController : MonoBehaviour
{
    [Header("References")]
    public GameObject confirmPanel;          
    public CustomerDialogue customer;        
    public CustomerPayment payment;          
    public DropZone changeZone;              

    [Header("Lines")]
    public string[] rightLines = { "Perfect! Thank you, see you again!" };
    public string[] wrongLines =
    {
        "Hmm, that's not the right change...",
        "Meow? Please count my change again."
    };
    public string[] emptyLines = { "Where's my change?" };
    public float lineSeconds = 2f;

    public System.Action OnConfirmedCorrect; 

    void Awake()
    {
        var img = confirmPanel.GetComponent<Image>();
        if (!img) img = confirmPanel.AddComponent<Image>();
        var btn = confirmPanel.GetComponent<Button>();
        if (!btn) btn = confirmPanel.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(OnConfirmClicked);
        confirmPanel.SetActive(false);
    }

    void OnConfirmClicked()
    {
        int expected = payment.moneyGiven - payment.totalPrice;
        int inBox = CountMoneyInBox();
        Debug.Log($"Confirm: money in box {inBox} | expected change {expected}");

        if (inBox == 0 && expected > 0)
        {
            Say(emptyLines);
        }
        else if (inBox == expected)
        {
            confirmPanel.SetActive(false);
            Say(rightLines);
            OnConfirmedCorrect?.Invoke();
        }
        else
        {
            Say(wrongLines);
        }
    }

    int CountMoneyInBox()
    {
        int sum = 0;
        foreach (var s in changeZone.stacks)
        {
            if (s == null) continue;
            if (s.transform.parent != changeZone.Rect) continue;
            sum += s.Value;
        }
        return sum;
    }

    void Say(string[] lines)
    {
        if (customer && lines != null && lines.Length > 0)
            customer.Say(lines[Random.Range(0, lines.Length)], lineSeconds);
    }

    public void ShowConfirm() => confirmPanel.SetActive(true);
}