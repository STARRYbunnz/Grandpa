using UnityEngine;

public class CalculationChecker : MonoBehaviour
{
    public enum ExpectedAnswer
    {
        TotalPrice,   
        Change        
    }

    [Header("References")]
    public Calculator calculator;
    public CustomerPayment payment;
    public CustomerDialogue dialogue;     
    public SlidePanel cashierPanel;       

    [Header("What must the player calculate?")]
    public ExpectedAnswer expected = ExpectedAnswer.TotalPrice;

    [Header("Customer reactions")]
    public string[] wrongLines =
    {
        "Hmm, that doesn't look right...",
        "That's not correct! Please calculate again.",
        "Wait, you got it wrong!"
    };
    public string[] rightLines =
    {
        "Great, thank you!",
        "Correct! Here you go."
    };
    public float reactionSeconds = 2f;

    public event System.Action OnCorrect;

    bool solved;

    void OnEnable()
    {
        if (calculator) calculator.OnResult += CheckResult;
        else Debug.LogError("CalculationChecker: Calculator is not assigned.");

        if (payment) payment.OnMoneyPutDown += OnNewPayment;
    }

    void OnDisable()
    {
        if (calculator) calculator.OnResult -= CheckResult;
        if (payment) payment.OnMoneyPutDown -= OnNewPayment;
    }

    void OnNewPayment()
    {
        solved = false;
        if (cashierPanel) cashierPanel.SlideIn();
    }

    int GetExpectedValue()
    {
        int price = payment.totalPrice;
        return expected == ExpectedAnswer.TotalPrice
            ? price
            : payment.moneyGiven - price;
    }

    void CheckResult(float value)
    {
        if (!payment || !payment.MoneyPutDown || solved) return;

        int answer = GetExpectedValue();
        bool correct = Mathf.Abs(value - answer) < 0.01f;

        Debug.Log("CalculationChecker: player answered " + value + " | correct answer " + answer);

        if (correct)
        {
            solved = true;
            Say(rightLines);
            if (cashierPanel) cashierPanel.SlideOut();  
            OnCorrect?.Invoke();
        }
        else
        {
            Say(wrongLines);
        }
    }

    void Say(string[] options)
    {
        if (!dialogue || options == null || options.Length == 0) return;
        dialogue.Say(options[Random.Range(0, options.Length)], reactionSeconds);
    }
}