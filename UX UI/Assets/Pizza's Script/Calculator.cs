using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;

public class Calculator : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text displayText;

    private string currentExpression = "";


    public void Number0()
    {

        if (string.IsNullOrEmpty(currentExpression))
            return;

        char lastChar = currentExpression[currentExpression.Length - 1];
        if (lastChar == '+' || lastChar == '-' || lastChar == '*' || lastChar == '/')
            return;

        AppendToExpression("0");
    }
    public void Number1() => AppendToExpression("1");
    public void Number2() => AppendToExpression("2");
    public void Number3() => AppendToExpression("3");
    public void Number4() => AppendToExpression("4");
    public void Number5() => AppendToExpression("5");
    public void Number6() => AppendToExpression("6");
    public void Number7() => AppendToExpression("7");
    public void Number8() => AppendToExpression("8");
    public void Number9() => AppendToExpression("9");

    public void Decimal()
    {

        int lastOperatorIndex = -1;
        for (int i = currentExpression.Length - 1; i >= 0; i--)
        {
            char c = currentExpression[i];
            if (c == '+' || c == '-' || c == '*' || c == '/')
            {
                lastOperatorIndex = i;
                break;
            }
        }

        string currentNumber = currentExpression.Substring(lastOperatorIndex + 1);

        if (currentNumber.Contains("."))
            return;

        if (string.IsNullOrEmpty(currentNumber))
        {
            AppendToExpression("0.");
        }
        else
        {
            AppendToExpression(".");
        }
    }

    public void Add() => AppendOperator("+");
    public void Subtract() => AppendOperator("-");
    public void Multiply() => AppendOperator("*");
    public void Divide() => AppendOperator("/");

    private void AppendOperator(string op)
    {

        if (string.IsNullOrEmpty(currentExpression))
        {
            if (op == "-")
            {
                AppendToExpression(op);
            }
            return;
        }


        char lastChar = currentExpression[currentExpression.Length - 1];
        if (lastChar == '+' || lastChar == '-' || lastChar == '*' || lastChar == '/')
            return;

        AppendToExpression(op);
    }

    public void Delete()
    {
        if (currentExpression.Length > 0)
        {
            currentExpression = currentExpression.Substring(0, currentExpression.Length - 1);
            UpdateDisplay();
        }
    }

    public void ClearAll()
    {
        currentExpression = "";
        UpdateDisplay();
    }

    public void Equals()
    {
        if (string.IsNullOrEmpty(currentExpression))
            return;

        try
        {
            var result = new DataTable().Compute(currentExpression, null);
            float value = Convert.ToSingle(result);

            if (float.IsInfinity(value) || float.IsNaN(value))
            {
                displayText.text = "Error";
                currentExpression = "";
                return;
            }

            currentExpression = value.ToString();
            UpdateDisplay();
        }
        catch
        {
            displayText.text = "Error";
            currentExpression = "";
        }
    }

    private void AppendToExpression(string value)
    {
        currentExpression += value;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        displayText.text = string.IsNullOrEmpty(currentExpression) ? "0" : currentExpression;
    }
}