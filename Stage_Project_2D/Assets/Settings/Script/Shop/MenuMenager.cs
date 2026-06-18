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
        public Loot loot;
        public SpellData spell;
    }

    [Header("Shop")]
    [SerializeField] private Canvas shop;

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
        RefreshArtefactBindings();
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
            buyButton = FindButtonByName(GetButtons(), "BuyButton");

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

        Toggle[] toggles = GetToggles();
        TMP_Text[] texts = GetTexts();
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
                priceText = priceText,
                loot = FindLoot(toggle, index, priceText),
                spell = FindSpellData(toggle, index, priceText)
            });
        }

        foundArtefacts.Sort((first, second) =>
        {
            int firstIndex = first != null && first.artefactToggle != null ? GetLastNumber(first.artefactToggle.gameObject.name) : -1;
            int secondIndex = second != null && second.artefactToggle != null ? GetLastNumber(second.artefactToggle.gameObject.name) : -1;
            return firstIndex.CompareTo(secondIndex);
        });

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

            if(artefact.loot == null && artefact.spell == null)
                return false;
        }

        return true;
    }

    private void RefreshArtefactBindings()
    {
        if(artefacts == null)
            return;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact == null || artefact.artefactToggle == null || artefact.priceText == null)
                continue;

            int index = GetLastNumber(artefact.artefactToggle.gameObject.name);
            artefact.loot = FindLoot(artefact.artefactToggle, index, artefact.priceText);
            artefact.spell = FindSpellData(artefact.artefactToggle, index, artefact.priceText);
        }
    }

    private TMP_Text FindPriceText(TMP_Text[] texts, int index)
    {
        foreach(TMP_Text text in texts)
        {
            if(!IsPriceTextName(text.gameObject.name))
                continue;

            if(GetLastNumber(text.gameObject.name) == index)
                return text;
        }

        return null;
    }

    private Toggle[] GetToggles()
    {
        if(shop != null)
            return shop.GetComponentsInChildren<Toggle>(true);

        return FindObjectsOfType<Toggle>();
    }

    private TMP_Text[] GetTexts()
    {
        if(shop != null)
            return shop.GetComponentsInChildren<TMP_Text>(true);

        return FindObjectsOfType<TMP_Text>();
    }

    private Button[] GetButtons()
    {
        if(shop != null)
            return shop.GetComponentsInChildren<Button>(true);

        return FindObjectsOfType<Button>();
    }

    private Button FindButtonByName(Button[] buttons, string buttonName)
    {
        foreach(Button button in buttons)
        {
            if(button.gameObject.name == buttonName)
                return button;
        }

        return null;
    }

    private bool IsPriceTextName(string objectName)
    {
        string lowerName = objectName.ToLower();
        return lowerName.Contains("price")
            || lowerName.Contains("prezzo")
            || lowerName.Contains("cost")
            || lowerName.Contains("costo");
    }

    private Loot FindLoot(Toggle toggle, int index, TMP_Text priceText)
    {
        Loot loot = FindComponentInParents<Loot>(toggle.transform);

        if(loot != null && loot.item != null)
            return loot;

        loot = FindLootByRowData(toggle, priceText);

        if(loot != null)
            return loot;

        foreach(Loot foundLoot in GetShopComponents<Loot>())
        {
            if(foundLoot != null && foundLoot.item != null && IsShopElementIndex(foundLoot.transform, foundLoot.quantity, index))
                return foundLoot;
        }

        return null;
    }

    private Loot FindLootByRowData(Toggle toggle, TMP_Text priceText)
    {
        float rowPrice = GetPrice(priceText != null ? priceText.text : string.Empty);
        string rowText = GetCombinedRowText(toggle != null ? toggle.transform : null);

        foreach(Loot foundLoot in GetShopComponents<Loot>())
        {
            if(foundLoot == null || foundLoot.item == null)
                continue;

            bool priceMatches = rowPrice > 0 && Mathf.Approximately(foundLoot.item.price, rowPrice);
            bool nameMatches = !string.IsNullOrWhiteSpace(rowText)
                && !string.IsNullOrWhiteSpace(foundLoot.item.Name)
                && rowText.Contains(foundLoot.item.Name.ToLower());

            if(priceMatches || nameMatches)
                return foundLoot;
        }

        return null;
    }

    private SpellData FindSpellData(Toggle toggle, int index, TMP_Text priceText)
    {
        SpellPrefab spellPrefab = FindComponentInParents<SpellPrefab>(toggle.transform);

        if(spellPrefab != null && spellPrefab.spell != null)
            return spellPrefab.spell;

        SpellData spell = FindSpellByRowData(toggle, priceText);

        if(spell != null)
            return spell;

        foreach(SpellPrefab foundSpellPrefab in GetShopComponents<SpellPrefab>())
        {
            if(foundSpellPrefab != null && foundSpellPrefab.spell != null && IsShopElementIndex(foundSpellPrefab.transform, foundSpellPrefab.quantity, index))
                return foundSpellPrefab.spell;
        }

        return null;
    }

    private SpellData FindSpellByRowData(Toggle toggle, TMP_Text priceText)
    {
        float rowPrice = GetPrice(priceText != null ? priceText.text : string.Empty);
        string rowText = GetCombinedRowText(toggle != null ? toggle.transform : null);

        foreach(SpellPrefab foundSpellPrefab in GetShopComponents<SpellPrefab>())
        {
            if(foundSpellPrefab == null || foundSpellPrefab.spell == null)
                continue;

            bool priceMatches = rowPrice > 0 && Mathf.Approximately(foundSpellPrefab.spell.price, rowPrice);
            bool nameMatches = !string.IsNullOrWhiteSpace(rowText)
                && !string.IsNullOrWhiteSpace(foundSpellPrefab.spell.spellName)
                && rowText.Contains(foundSpellPrefab.spell.spellName.ToLower());

            if(priceMatches || nameMatches)
                return foundSpellPrefab.spell;
        }

        return null;
    }

    private T FindComponentInParents<T>(Transform startTransform) where T : Component
    {
        Transform current = startTransform;

        while(current != null)
        {
            T component = current.GetComponent<T>();

            if(component != null)
                return component;

            if(shop != null && current == shop.transform)
                break;

            current = current.parent;
        }

        return null;
    }

    private T[] GetShopComponents<T>() where T : Component
    {
        if(shop != null)
            return shop.GetComponentsInChildren<T>(true);

        return FindObjectsOfType<T>();
    }

    private bool IsShopElementIndex(Transform element, int quantity, int index)
    {
        if(quantity > 0 && quantity == index)
            return true;

        Transform current = element;

        while(current != null)
        {
            if(GetLastNumber(current.gameObject.name) == index)
                return true;

            if(shop != null && current == shop.transform)
                break;

            current = current.parent;
        }

        return false;
    }

    private string GetCombinedRowText(Transform startTransform)
    {
        Transform rowRoot = FindIndexedParent(startTransform);

        if(rowRoot == null)
            rowRoot = startTransform;

        if(rowRoot == null)
            return string.Empty;

        TMP_Text[] texts = rowRoot.GetComponentsInChildren<TMP_Text>(true);
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        foreach(TMP_Text text in texts)
        {
            if(text == null || string.IsNullOrWhiteSpace(text.text))
                continue;

            builder.Append(text.text.ToLower());
            builder.Append(' ');
        }

        return builder.ToString();
    }

    private Transform FindIndexedParent(Transform startTransform)
    {
        Transform current = startTransform;

        while(current != null)
        {
            if(GetLastNumber(current.gameObject.name) >= 0)
                return current;

            if(shop != null && current == shop.transform)
                break;

            current = current.parent;
        }

        return null;
    }

    private InventoryMenager FindInventoryMenager()
    {
        if(InventoryMenager.Instance != null)
            return InventoryMenager.Instance;

        return FindObjectOfType<InventoryMenager>();
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

        List<Artefact> selectedArtefacts = GetSelectedArtefacts();
        InventoryMenager inventory = FindInventoryMenager();

        if(!CanRegisterSelectedArtefacts(selectedArtefacts, inventory))
            return;

        player.CoinSetGet -= Mathf.RoundToInt(total);

        RegisterSelectedArtefacts(selectedArtefacts, inventory);

        foreach(Artefact artefact in selectedArtefacts)
            artefact.artefactToggle.isOn = false;

        UpdateTotal();
    }

    private List<Artefact> GetSelectedArtefacts()
    {
        List<Artefact> selectedArtefacts = new List<Artefact>();

        if(artefacts == null)
            return selectedArtefacts;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact != null && artefact.artefactToggle != null && artefact.artefactToggle.isOn)
                selectedArtefacts.Add(artefact);
        }

        selectedArtefacts.Sort((first, second) =>
        {
            int firstIndex = first != null && first.artefactToggle != null ? GetLastNumber(first.artefactToggle.gameObject.name) : -1;
            int secondIndex = second != null && second.artefactToggle != null ? GetLastNumber(second.artefactToggle.gameObject.name) : -1;
            return firstIndex.CompareTo(secondIndex);
        });

        return selectedArtefacts;
    }

    private bool CanRegisterSelectedArtefacts(List<Artefact> selectedArtefacts, InventoryMenager inventory)
    {
        int pendingItems = 0;

        foreach(Artefact artefact in selectedArtefacts)
        {
            bool needsInventory = artefact.loot != null || artefact.spell != null;

            if(needsInventory && inventory == null)
                return false;

            if(artefact.loot != null)
            {
                pendingItems++;

                if(!inventory.CanAddItem(artefact.loot, pendingItems))
                    return false;
            }

            if(artefact.spell != null && !inventory.CanAddSpell(artefact.spell))
                return false;
        }

        return true;
    }

    private void RegisterSelectedArtefacts(List<Artefact> selectedArtefacts, InventoryMenager inventory)
    {
        if(inventory == null)
            return;

        foreach(Artefact artefact in selectedArtefacts)
        {
            if(artefact.loot != null)
                inventory.AddItem(artefact.loot);

            if(artefact.spell != null)
                inventory.AddSpell(artefact.spell);
        }
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
