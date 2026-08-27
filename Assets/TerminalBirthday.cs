using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class TerminalBirthday : MonoBehaviour
{
    [Header("顯示文字")]
    public TextMeshProUGUI yearDisplay;
    public TextMeshProUGUI monthDisplay;
    public TextMeshProUGUI dayDisplay;
    public TextMeshProUGUI errorDisplay; // 顯示錯誤提示

    public NameInputController nameInputController;

    private float cursorTimer = 0f;
    private bool cursorVisible = true;
    private string yearValue = "";
    private string monthValue = "";
    private string dayValue = "";

    private enum Field { Year, Month, Day, Done }
    private Field currentField = Field.Year;

    void OnEnable()
    {
        yearValue = "";
        monthValue = "";
        dayValue = "";
        currentField = Field.Year;
        if (errorDisplay != null) errorDisplay.text = "";
        UpdateAllDisplays();
    }

    void Update()
    {
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= 0.5f)
        {
            cursorVisible = !cursorVisible;
            cursorTimer = 0f;
            UpdateAllDisplays();
        }

        if (!gameObject.activeInHierarchy) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        Key[] digitKeys = {
            Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
            Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };
        Key[] numpadKeys = {
            Key.Numpad0, Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4,
            Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9
        };

        for (int i = 0; i < digits.Length; i++)
        {
            if (keyboard[digitKeys[i]].wasPressedThisFrame ||
                keyboard[numpadKeys[i]].wasPressedThisFrame)
            {
                AddDigit(digits[i]);
            }
        }

        if (keyboard.backspaceKey.wasPressedThisFrame)
            DeleteDigit();

        if (keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame)
            NextField();
    }

    void AddDigit(string digit)
    {
        if (errorDisplay != null) errorDisplay.text = "";

        switch (currentField)
        {
            case Field.Year:
                if (yearValue.Length < 4)
                {
                    // 第一位只能是 1 或 2
                    if (yearValue.Length == 0 && digit != "1" && digit != "2")
                    {
                        ShowError("don't lie.");
                        return;
                    }
                    yearValue += digit;
                }
                break;

            case Field.Month:
                if (monthValue.Length < 2)
                {
                    // 第一位只能 0 或 1
                    if (monthValue.Length == 0 && int.Parse(digit) > 1)
                    {
                        ShowError("that's not a month.");
                        return;
                    }
                    monthValue += digit;
                }
                break;

            case Field.Day:
                if (dayValue.Length < 2)
                {
                    // 第一位只能 0-3
                    if (dayValue.Length == 0 && int.Parse(digit) > 3)
                    {
                        ShowError("that's not a day.");
                        return;
                    }
                    dayValue += digit;
                }
                break;
        }
        UpdateAllDisplays();
    }

    void NextField()
    {
        if (currentField == Field.Year)
        {
            if (!ValidateYear()) return;
            currentField = Field.Month;
        }
        else if (currentField == Field.Month)
        {
            if (!ValidateMonth()) return;
            currentField = Field.Day;
        }
        else if (currentField == Field.Day)
        {
            if (!ValidateDay()) return;
            currentField = Field.Done;
            nameInputController.ConfirmBirthday();
        }
        UpdateAllDisplays();
    }

    bool ValidateYear()
    {
        if (yearValue.Length < 4)
        {
            ShowError("enter a full year.");
            return false;
        }
        int year = int.Parse(yearValue);
        if (year < 1900 || year > 2010)
        {
            ShowError("...really?");
            yearValue = "";
            return false;
        }
        return true;
    }

    bool ValidateMonth()
    {
        if (monthValue.Length < 2)
        {
            ShowError("enter a valid month.");
            return false;
        }
        int month = int.Parse(monthValue);
        if (month < 1 || month > 12)
        {
            ShowError("that's not a month.");
            monthValue = "";
            return false;
        }
        return true;
    }

    bool ValidateDay()
    {
        if (dayValue.Length < 2)
        {
            ShowError("enter a valid day.");
            return false;
        }
        int day = int.Parse(dayValue);
        int month = int.Parse(monthValue);

        int[] maxDays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        int max = maxDays[month - 1];

        if (day < 1 || day > max)
        {
            ShowError("that date doesn't exist.");
            dayValue = "";
            return false;
        }
        return true;
    }

    void ShowError(string message)
    {
        if (errorDisplay != null)
            errorDisplay.text = message;
    }

    void DeleteDigit()
    {
        if (errorDisplay != null) errorDisplay.text = "";

        switch (currentField)
        {
            case Field.Year:
                if (yearValue.Length > 0)
                    yearValue = yearValue.Substring(0, yearValue.Length - 1);
                break;
            case Field.Month:
                if (monthValue.Length > 0)
                    monthValue = monthValue.Substring(0, monthValue.Length - 1);
                else
                    currentField = Field.Year;
                break;
            case Field.Day:
                if (dayValue.Length > 0)
                    dayValue = dayValue.Substring(0, dayValue.Length - 1);
                else
                    currentField = Field.Month;
                break;
        }
        UpdateAllDisplays();
    }

    void UpdateAllDisplays()
    {
        yearDisplay.text = FormatDisplay(yearValue, 4);
        monthDisplay.text = FormatDisplay(monthValue, 2);
        dayDisplay.text = FormatDisplay(dayValue, 2);

        switch (currentField)
        {
            case Field.Year:
                yearDisplay.text = FormatDisplayWithCursor(yearValue, 4);
                break;
            case Field.Month:
                monthDisplay.text = FormatDisplayWithCursor(monthValue, 2);
                break;
            case Field.Day:
                dayDisplay.text = FormatDisplayWithCursor(dayValue, 2);
                break;
        }
    }

    string FormatDisplay(string value, int maxLen)
    {
        string result = value;
        for (int i = value.Length; i < maxLen; i++)
        {
            if (i > 0) result += " ";
            result += "_";
        }
        return result;
    }

    string FormatDisplayWithCursor(string value, int maxLen)
    {
        string cursor = cursorVisible ? "<color=#FFFFFF>|</color>" : " ";
        string result = "<color=#FFFFFF>" + value + "</color>" + cursor;
        for (int i = value.Length; i < maxLen; i++)
        {
            if (i > 0) result += " ";
            result += "_";
        }
        return result;
    }

    public string GetBirthday()
    {
        return yearValue + monthValue + dayValue;
    }
}