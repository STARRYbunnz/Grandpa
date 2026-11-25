using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChangeController : MonoBehaviour
{
    enum Phase { Idle, Calculating, CollectingMoney, GivingChange, Done }

    [Header("References")]
    public CustomerPayment payment;      
    public CalculationChecker checker;   
    public DropZone[] cashierZones;      
    public SlidePanel cashierPanel;       
    public CustomerDialogue dialogue;    

    [Header("Behaviour")]
    [Tooltip("Check the change automatically when the right amount is in the zone. " +
             "You can also call ConfirmChange() from a button.")]
    public bool autoCheck = false;       
    public float finishDelay = 1.5f;

    [Header("Confirm button")]
    [Tooltip("Shown only while the player is giving change. Pressing it checks the change.")]
    public Button confirmButton;

    [Header("Customer lines")]
    public string collectHint = "Please put my money in the cashier first.";
    public string askChange = "Can I have my change, please?";
    public string[] tooMuchLines = { "That's too much change!", "You're giving me too much!" };
    public string[] notEnoughLines = { "That's not enough change.", "Hmm, I'm missing some change." };
    public string[] thanksLines = { "Thank you! Have a nice day!", "Perfect, thanks!" };
    public float reactionSeconds = 2f;

    [Header("Events")]
    public UnityEvent onCustomerFinished; 
    Phase phase = Phase.Idle;

    DropZone CustomerZone => payment ? payment.dropZone : null;
    int RequiredChange => payment.moneyGiven - payment.totalPrice;

    void Start()
    {
        SetConfirmVisible(false);
    }

    void OnEnable()
    {
        if (payment) payment.OnMoneyPutDown += OnMoneyPutDown;
        if (confirmButton) confirmButton.onClick.AddListener(ConfirmChange);
        if (checker) checker.OnCorrect += OnCalculatedCorrectly;

        if (CustomerZone != null) CustomerZone.OnTotalValueChanged += OnZoneChanged;
        foreach (DropZone z in cashierZones) if (z) z.OnTotalValueChanged += OnZoneChanged;
    }

    void OnDisable()
    {
        if (payment) payment.OnMoneyPutDown -= OnMoneyPutDown;
        if (confirmButton) confirmButton.onClick.RemoveListener(ConfirmChange);
        if (checker) checker.OnCorrect -= OnCalculatedCorrectly;

        if (CustomerZone != null) CustomerZone.OnTotalValueChanged -= OnZoneChanged;
        foreach (DropZone z in cashierZones) if (z) z.OnTotalValueChanged -= OnZoneChanged;
    }

    void OnMoneyPutDown()
    {
        phase = Phase.Calculating;
        SetConfirmVisible(false);
    }

    void OnCalculatedCorrectly()
    {
        phase = Phase.CollectingMoney;
        Say(collectHint);
        Evaluate(); 
    }

    void OnZoneChanged(int _)
    {
        Evaluate();
    }

    void Evaluate()
    {
        if (phase == Phase.CollectingMoney && AllCustomerMoneyInCashier())
        {
            phase = Phase.GivingChange;

            if (RequiredChange <= 0)
            {
                Finish();   
                return;
            }

            SetConfirmVisible(true);   
            Say(askChange);
        }

        if (phase == Phase.GivingChange && autoCheck)
        {
            int given = GivenChange();
            if (given == RequiredChange) Finish();
            else if (given > RequiredChange) Say(Pick(tooMuchLines));
        }
    }

    public void ConfirmChange()
    {
        if (phase != Phase.GivingChange) return;

        int given = GivenChange();
        if (given == RequiredChange) Finish();
        else if (given > RequiredChange) Say(Pick(tooMuchLines));
        else Say(Pick(notEnoughLines));
    }

    bool AllCustomerMoneyInCashier()
    {
        if (cashierZones == null || cashierZones.Length == 0) return false;
        if (payment.customerStacks.Count == 0) return false;

        foreach (MoneyStack s in payment.customerStacks)
        {
            if (!s || !InAnyCashierZone(s)) return false;
        }
        return true;
    }

    bool InAnyCashierZone(MoneyStack s)
    {
        foreach (DropZone z in cashierZones)
        {
            if (z && z.stacks.Contains(s)) return true;
        }
        return false;
    }

    int GivenChange()
    {
        int sum = 0;
        foreach (MoneyStack s in CustomerZone.stacks)
        {
            if (!s) continue;
            if (payment.customerStacks.Contains(s)) continue;
            if (s.transform.parent != CustomerZone.Rect) continue;
            sum += s.Value;
        }
        return sum;
    }

    void Finish()
    {
        if (phase == Phase.Done) return;
        phase = Phase.Done;
        SetConfirmVisible(false);

        Say(Pick(thanksLines));
        StartCoroutine(FinishRoutine());
    }

    IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(finishDelay);

        CustomerZone.stacks.RemoveAll(s => s == null);
        for (int i = CustomerZone.stacks.Count - 1; i >= 0; i--)
        {
            MoneyStack s = CustomerZone.stacks[i];
            if (payment.customerStacks.Contains(s)) continue;
            CustomerZone.RemoveStack(s);
            Destroy(s.gameObject);
        }

        if (cashierPanel) cashierPanel.SlideIn();
        payment.ResetForNewCustomer();
        phase = Phase.Idle;

        onCustomerFinished?.Invoke();
    }

    void SetConfirmVisible(bool visible)
    {
        if (confirmButton) confirmButton.gameObject.SetActive(visible);
    }

    string Pick(string[] options)
    {
        return (options == null || options.Length == 0)
            ? ""
            : options[Random.Range(0, options.Length)];
    }

    void Say(string message)
    {
        if (dialogue && !string.IsNullOrEmpty(message))
            dialogue.Say(message, reactionSeconds);
    }
}