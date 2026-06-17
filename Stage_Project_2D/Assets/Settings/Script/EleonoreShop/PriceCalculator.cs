using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PriceCalculator : MonoBehaviour
{
    [Header("Prezzi")]
    public TMP_Text priceText1;
    public TMP_Text priceText2;
    public TMP_Text totalText;

    [Header("Artefacts")]
    public Toggle Artefact1;
    public Toggle Artefact2;

    private float price1;
    private float price2;
    private float total = 0;

    private bool CanChangePrice1 = false;
    private bool CanChangePrice2 = false;

    void Start()
    {
        Artefact1.onValueChanged.AddListener(OnToggleChanged);
        Artefact2.onValueChanged.AddListener(OnToggleChanged);

        price1 = GetPrice(priceText1.text);
        price2 = GetPrice(priceText2.text);
    }

    void Update()
    {
        if( Artefact1.gameObject.GetComponent<Toggle>().isOn && !CanChangePrice1)
        {
            total += price1;
            CanChangePrice1 = true;
        }    
        else if (CanChangePrice1)
        {
            total -= price1;
            CanChangePrice1 = false;
        }

        if( Artefact2.gameObject.GetComponent<Toggle>().isOn && !CanChangePrice2) 
        {
            total += price2;
            CanChangePrice2 = true;
        }    
        else if (CanChangePrice2)
        {
            total -= price2;
            CanChangePrice2 = false;
        }
    }

    private float GetPrice(string text)
    {
        float.TryParse(text, 
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float value);

        return value;
    }

    private void OnToggleChanged(bool value)
    {
        totalText.text = total.ToString("0.00") + " coin";
    }
}