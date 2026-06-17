using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PriceCalculator : MonoBehaviour
{
    public TMP_Text priceText1;
    public TMP_Text priceText2;
    public TMP_Text totalText;

    void Update()
    {
        float price1 = GetPrice(priceText1.text);
        float price2 = GetPrice(priceText2.text);

        float total = price1 + price2;

        totalText.text = total.ToString("0.00") + " €";
    }

    private float GetPrice(string text)
    {
        text = text.Replace("€", "");
        text = text.Replace(",", ".");
        text = text.Trim();

        float.TryParse(text, 
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float value);

        return value;
    }
}