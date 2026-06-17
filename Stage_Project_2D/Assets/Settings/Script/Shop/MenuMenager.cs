using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MenuMenager : MonoBehaviour 
{
    [System.Serializable]
    private class Artefact
    {
        public Toggle artefactToggle;
        public TMP_Text priceText;
    }

    [Header("Prezzi")]
    public TMP_Text totalText;
    public TMP_Text playerCoinText;
    public Button buyButton;

    [Header("Player")]
    [SerializeField] private PlayerMovement player;

    [Header("Artefacts")]
    [SerializeField] private Artefact[] artefacts;

    private float total = 0;

    private void OnEnable()
    {
        FindMissingReferences();
        BuildArtefactsIfEmpty();
        AddToggleListeners();
        AddBuyButtonListener();
        UpdateTotal();
        UpdatePlayerCoinText();
    }

    private void OnDisable()
    {
        RemoveToggleListeners();
        RemoveBuyButtonListener();
    }

    private void OnDestroy()
    {
        RemoveToggleListeners();
        RemoveBuyButtonListener();
    }

    private void Update()
    {
        UpdatePlayerCoinText();
        UpdateBuyButton();
    }

    private void AddToggleListeners()
    {
        if(artefacts == null)
            return;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact == null || artefact.artefactToggle == null)
                continue;

            artefact.artefactToggle.onValueChanged.RemoveListener(OnArtefactToggleChanged);
            artefact.artefactToggle.onValueChanged.AddListener(OnArtefactToggleChanged);
        }
    }

    private void RemoveToggleListeners()
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

    private void OnArtefactToggleChanged(bool value)
    {
        UpdateTotal();
    }

    private void AddBuyButtonListener()
    {
        if(buyButton == null)
            return;

        buyButton.onClick.RemoveListener(BuySelectedArtefacts);
        buyButton.onClick.AddListener(BuySelectedArtefacts);
    }

    private void RemoveBuyButtonListener()
    {
        if(buyButton != null)
            buyButton.onClick.RemoveListener(BuySelectedArtefacts);
    }

    private void FindMissingReferences()
    {
        if(totalText == null)
            totalText = GetComponent<TMP_Text>();

        if(playerCoinText == null)
        {
            foreach(TMP_Text text in FindObjectsOfType<TMP_Text>())
            {
                if(text.transform.parent != null && text.transform.parent.name == "Player Coin" && text.gameObject.name == "Ammount")
                {
                    playerCoinText = text;
                    break;
                }
            }
        }

        if(buyButton == null)
        {
            foreach(Button button in FindObjectsOfType<Button>())
            {
                if(button.gameObject.name == "BuyButton")
                {
                    buyButton = button;
                    break;
                }
            }
        }

        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();
        foreach(PlayerMovement foundPlayer in players)
        {
            if(player == null || foundPlayer.CoinSetGet > player.CoinSetGet)
                player = foundPlayer;
        }
    }

    private void BuildArtefactsIfEmpty()
    {
        if(HasValidArtefacts())
            return;

        Toggle[] toggles = FindObjectsOfType<Toggle>();
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>();
        List<Artefact> foundArtefacts = new List<Artefact>();

        foreach(Toggle toggle in toggles)
        {
            int index = GetLastNumber(toggle.gameObject.name);

            if(index < 0)
                continue;

            TMP_Text priceText = FindPriceText(texts, index);

            if(priceText == null)
                continue;

            foundArtefacts.Add(new Artefact
            {
                artefactToggle = toggle,
                priceText = priceText
            });
        }

        artefacts = foundArtefacts.ToArray();
    }

    private bool HasValidArtefacts()
    {
        if(artefacts == null || artefacts.Length == 0)
            return false;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact == null || artefact.artefactToggle == null || artefact.priceText == null)
                return false;
        }

        return true;
    }

    private TMP_Text FindPriceText(TMP_Text[] texts, int index)
    {
        foreach(TMP_Text text in texts)
        {
            if(!text.gameObject.name.Contains("Price"))
                continue;

            if(GetLastNumber(text.gameObject.name) == index)
                return text;
        }

        return null;
    }

    private int GetLastNumber(string text)
    {
        int number = -1;
        int multiplier = 1;

        for(int i = text.Length - 1; i >= 0; i--)
        {
            if(!char.IsDigit(text[i]))
                break;

            number = number < 0 ? 0 : number;
            number += (text[i] - '0') * multiplier;
            multiplier *= 10;
        }

        return number;
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

    private void UpdatePlayerCoinText()
    {
        if(player != null && playerCoinText != null)
            playerCoinText.text = player.CoinSetGet.ToString() + " coin";
    }

    private void UpdateBuyButton()
    {
        if(buyButton != null)
            buyButton.interactable = player != null && total > 0 && player.CoinSetGet >= total;
    }

    public void BuySelectedArtefacts()
    {
        UpdateTotal();

        if(player == null || total <= 0 || player.CoinSetGet < total)
            return;

        player.CoinSetGet -= Mathf.RoundToInt(total);
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

        UpdatePlayerCoinText();
        UpdateBuyButton();
    }
}
