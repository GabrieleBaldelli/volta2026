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
        public ItemData item;
        public SpellData spell;
        [Min(0)] public int quantity;
        private int boughtQuantity;

        public int BoughtQuantity
        {
            get { return boughtQuantity; }
        }

        public bool HasQuantityLimit
        {
            get { return quantity > 0; }
        }

        public bool IsSoldOut
        {
            get { return HasQuantityLimit && boughtQuantity >= quantity; }
        }

        public void RegisterPurchase()
        {
            boughtQuantity++;
        }
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
    private bool isConfiguredManager = true;

    private void OnEnable()
    {
        isConfiguredManager = HasSerializedSetup();

        if(!isConfiguredManager)
            return;

        FindMissingReferences();
        BuildArtefactsIfEmpty();
        RefreshArtefactBindings();
        ApplyArtefactDataToRows();
        AddToggleListeners();
        AddBuyButtonListener();
        UpdateTotal();
        UpdatePlayerCoinText();
    }

    private void OnDisable()
    {
        if(!isConfiguredManager)
            return;

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
        if(!isConfiguredManager)
            return;

        UpdatePlayerCoinText();
        UpdateBuyButton();
        ApplyArtefactDataToRows();
    }

    private bool HasSerializedSetup()
    {
        return shop != null
            || buyButton != null
            || player != null
            || (artefacts != null && artefacts.Length > 0);
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
                priceText = priceText
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

            if(artefact.item == null && artefact.spell == null)
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

            if(artefact.spell != null)
            {
                artefact.item = null;
                continue;
            }
        }
    }

    private void ApplyArtefactDataToRows()
    {
        if(artefacts == null)
            return;

        foreach(Artefact artefact in artefacts)
        {
            if(artefact == null || artefact.artefactToggle == null)
                continue;

            Transform rowRoot = FindIndexedParent(artefact.artefactToggle.transform);

            if(rowRoot == null)
                continue;

            if(artefact.spell != null)
            {
                ApplySpellDataToRow(rowRoot, artefact.spell);
                UpdateArtefactAvailability(artefact);
                continue;
            }

            if(artefact.item != null)
                ApplyItemDataToRow(rowRoot, artefact.item);

            UpdateArtefactAvailability(artefact);
        }
    }

    private void ApplySpellDataToRow(Transform rowRoot, SpellData spell)
    {
        TMP_Text[] texts = rowRoot.GetComponentsInChildren<TMP_Text>(true);
        Image[] images = rowRoot.GetComponentsInChildren<Image>(true);

        TMP_Text nameText = FindText(texts, "name item", "name", "nome");
        TMP_Text descriptionText = FindText(texts, "description", "descrizione", "desc");
        TMP_Text priceText = FindText(texts, "price", "prezzo", "cost", "costo");
        Image iconImage = FindImage(images, "portrait", "icon", "icona", "sprite", "image", "immagine");

        if(nameText != null)
            nameText.SetText(spell.spellName);

        if(descriptionText != null)
            descriptionText.SetText(GetShopDescription(spell.description, spell.GetValueMeaning(player)));

        if(priceText != null)
            priceText.SetText(spell.price.ToString());

        if(iconImage != null)
            iconImage.sprite = spell.icon;
    }

    private void ApplyItemDataToRow(Transform rowRoot, ItemData item)
    {
        TMP_Text[] texts = rowRoot.GetComponentsInChildren<TMP_Text>(true);
        Image[] images = rowRoot.GetComponentsInChildren<Image>(true);

        TMP_Text nameText = FindText(texts, "name item", "name", "nome");
        TMP_Text descriptionText = FindText(texts, "description", "descrizione", "desc");
        TMP_Text priceText = FindText(texts, "price", "prezzo", "cost", "costo");
        Image iconImage = FindImage(images, "portrait", "icon", "icona", "sprite", "image", "immagine");

        if(nameText != null)
            nameText.SetText(item.Name);

        if(descriptionText != null)
            descriptionText.SetText(GetShopDescription(item.Description, item.GetValueMeaning(player)));

        if(priceText != null)
            priceText.SetText(item.price.ToString());

        if(iconImage != null)
            iconImage.sprite = item.icon;
    }

    private void UpdateArtefactAvailability(Artefact artefact)
    {
        if(artefact == null || artefact.artefactToggle == null)
            return;

        bool hasData = artefact.item != null || artefact.spell != null;
        bool canBuyMore = hasData && !artefact.IsSoldOut;
        artefact.artefactToggle.interactable = canBuyMore;

        if(!canBuyMore)
            artefact.artefactToggle.isOn = false;
    }

    private string GetShopDescription(string description, string valueMeaning)
    {
        if(string.IsNullOrWhiteSpace(valueMeaning))
            return description;

        if(string.IsNullOrWhiteSpace(description))
            return valueMeaning;

        return description + "\n" + valueMeaning;
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

    private TMP_Text FindText(TMP_Text[] texts, params string[] keywords)
    {
        foreach(TMP_Text text in texts)
        {
            string objectName = text.gameObject.name.ToLower();

            foreach(string keyword in keywords)
            {
                if(objectName.Contains(keyword))
                    return text;
            }
        }

        return null;
    }

    private Image FindImage(Image[] images, params string[] keywords)
    {
        foreach(Image image in images)
        {
            string objectName = image.gameObject.name.ToLower();

            foreach(string keyword in keywords)
            {
                if(objectName.Contains(keyword))
                    return image;
            }
        }

        return null;
    }

    private Transform FindIndexedParent(Transform startTransform)
    {
        Transform current = startTransform;

        while(current != null)
        {
            if(IsShopRowName(current.gameObject.name))
                return current;

            if(shop != null && current == shop.transform)
                break;

            current = current.parent;
        }

        return null;
    }

    private bool IsShopRowName(string objectName)
    {
        if(GetLastNumber(objectName) < 0)
            return false;

        string lowerName = objectName.ToLower();
        return lowerName.StartsWith("artefact")
            || lowerName.StartsWith("item")
            || lowerName.StartsWith("spell");
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
        if(buyButton == null)
            return;

        buyButton.interactable = player != null
            && total > 0
            && player.CoinSetGet >= total
            && CanRegisterSelectedArtefacts(GetSelectedArtefacts(), FindInventoryMenager());
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
            if(artefact.IsSoldOut)
                return false;

            bool needsInventory = artefact.item != null || artefact.spell != null;

            if(needsInventory && inventory == null)
                return false;

            if(artefact.item != null)
            {
                pendingItems++;

                if(!inventory.CanAddItem(artefact.item, pendingItems))
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
            bool registered = false;

            if(artefact.item != null)
                registered = inventory.AddItem(artefact.item);

            if(artefact.spell != null)
                registered = inventory.AddSpell(artefact.spell);

            if(registered)
                artefact.RegisterPurchase();
        }

        ApplyArtefactDataToRows();
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

                if(artefact.artefactToggle.isOn && !artefact.IsSoldOut)
                    total += GetPrice(artefact.priceText.text);
            }
        }

        if(totalText != null)
            totalText.text = total.ToString("0.00") + " coin";

        UpdatePlayerCoinText();
        UpdateBuyButton();
    }
}
