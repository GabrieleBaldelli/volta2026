using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PriceCalculator : MonoBehaviour 
{
    [System.Serializable]
    private class Artefact
    {
        public Toggle artefactToggle;
        public TMP_Text priceText;
    }

    [Header("Prezzi")]
    public TMP_Text totalText;

    [Header("Artefacts")]
    [SerializeField] private Artefact[] artefacts;

    private float total = 0;

    void Start()
    {
        InitializeArtefacts();
        UpdateTotal();
    }

    private void OnDestroy()
    {
        if(artefacts == null)
            return;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact == null || artefact.artefactToggle == null)
                continue;

            artefact.artefactToggle.onValueChanged.RemoveListener(OnArtefactToggleChanged);
        }
    }

    private void InitializeArtefacts()
    {
        if(artefacts == null)
            return;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact == null)
                continue;

            if(artefact.artefactToggle != null)
                artefact.artefactToggle.onValueChanged.AddListener(OnArtefactToggleChanged);
        }
    }

    private float GetPrice(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
            return 0;

        text = text.Replace("coin", "").Replace("Coin", "").Trim();

        if(float.TryParse(text, 
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float value))
            return value;

        float.TryParse(text, 
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.CurrentCulture,
            out value);

        return value;
    }

    private void OnArtefactToggleChanged(bool value)
    {
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        total = 0;

        if(artefacts != null)
        {
            foreach(Artefact artefact in artefacts)
            {
                if(artefact == null || artefact.artefactToggle == null || artefact.priceText == null)
                    continue;

                if(artefact.artefactToggle.isOn)
                    total += GetPrice(artefact.priceText.text);
            }
        }

        if(totalText != null)
            totalText.text = total.ToString("0.00") + " coin";
    }
}
