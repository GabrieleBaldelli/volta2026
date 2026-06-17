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

    private void Start()
    {
        AssignMissingToggles();

        if(Artefact1 != null)
            Artefact1.onValueChanged.AddListener(OnToggleChanged);

        if(Artefact2 != null)
            Artefact2.onValueChanged.AddListener(OnToggleChanged);

        price1 = priceText1 != null ? GetPrice(priceText1.text) : 0f;
        price2 = priceText2 != null ? GetPrice(priceText2.text) : 0f;

        UpdateTotal();
    }

    private void OnDestroy()
    {
        if(Artefact1 != null)
            Artefact1.onValueChanged.RemoveListener(OnToggleChanged);

        if(Artefact2 != null)
            Artefact2.onValueChanged.RemoveListener(OnToggleChanged);
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
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        total = 0f;

        if(Artefact1 != null && Artefact1.isOn)
            total += price1;

        if(Artefact2 != null && Artefact2.isOn)
            total += price2;

        if(totalText != null)
            totalText.text = total.ToString("0.00") + " coin";
    }

    private void AssignMissingToggles()
    {
        if(Artefact1 != null && Artefact2 != null)
            return;

        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if(parentCanvas == null)
            return;

        Toggle[] toggles = parentCanvas.GetComponentsInChildren<Toggle>(true);

        if(Artefact1 == null && toggles.Length > 0)
            Artefact1 = toggles[0];

        if(Artefact2 == null && toggles.Length > 1)
            Artefact2 = toggles[1];
    }
}
