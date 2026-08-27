using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BirthdaySelector : MonoBehaviour
{
    public TMP_Dropdown yearDropdown;
    public TMP_Dropdown monthDropdown;
    public TMP_Dropdown dayDropdown;

    void Start()
    {
        List<string> years = new List<string>();
        for (int y = 2010; y >= 1900; y--)
            years.Add(y.ToString());
        yearDropdown.ClearOptions();
        yearDropdown.AddOptions(years);
        yearDropdown.value = years.IndexOf("1999");

        List<string> months = new List<string>();
        for (int m = 1; m <= 12; m++)
            months.Add(m.ToString("D2"));
        monthDropdown.ClearOptions();
        monthDropdown.AddOptions(months);
        monthDropdown.value = 0;

        List<string> days = new List<string>();
        for (int d = 1; d <= 31; d++)
            days.Add(d.ToString("D2"));
        dayDropdown.ClearOptions();
        dayDropdown.AddOptions(days);
        dayDropdown.value = 0;
    }

    public string GetBirthday()
    {
        string year = yearDropdown.options[yearDropdown.value].text;
        string month = monthDropdown.options[monthDropdown.value].text;
        string day = dayDropdown.options[dayDropdown.value].text;
        return year + month + day;
    }
}