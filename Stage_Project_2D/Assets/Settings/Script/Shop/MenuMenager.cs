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
    private GameObject tooltipObject;
    private RectTransform tooltipRect;
    private TMP_Text tooltipTitleText;
    private TMP_Text tooltipBodyText;

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
                ConfigureTooltip(rowRoot, artefact.spell.spellName, GetSpellTooltipText(artefact.spell));
                UpdateArtefactAvailability(artefact);
                continue;
            }

            if(artefact.item != null)
            {
                ApplyItemDataToRow(rowRoot, artefact.item);
                ConfigureTooltip(rowRoot, artefact.item.Name, GetItemTooltipText(artefact.item));
            }

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
            descriptionText.SetText(spell.description);

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
            descriptionText.SetText(item.Description);

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

    private void ConfigureTooltip(Transform rowRoot, string title, string body)
    {
        if(rowRoot == null)
            return;

        ShopTooltipTarget tooltipTarget = rowRoot.GetComponent<ShopTooltipTarget>();

        if(tooltipTarget == null)
            tooltipTarget = rowRoot.gameObject.AddComponent<ShopTooltipTarget>();

        tooltipTarget.Configure(this, title, body);
    }

    public void ShowArtefactTooltip(string title, string body, Vector2 screenPosition)
    {
        if(string.IsNullOrWhiteSpace(body))
            return;

        EnsureTooltip();

        if(tooltipObject == null)
            return;

        tooltipTitleText.SetText(title);
        tooltipBodyText.SetText(body);
        tooltipObject.SetActive(true);
        MoveArtefactTooltip(screenPosition);
    }

    public void MoveArtefactTooltip(Vector2 screenPosition)
    {
        if(tooltipRect == null || shop == null)
            return;

        RectTransform canvasRect = shop.transform as RectTransform;

        if(canvasRect == null)
            return;

        Camera camera = shop.renderMode == RenderMode.ScreenSpaceOverlay ? null : shop.worldCamera;

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, camera, out Vector2 localPoint))
            return;

        Vector2 offset = new Vector2(18f, -18f);
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Rect canvasBounds = canvasRect.rect;
        Vector2 anchoredPosition = localPoint + offset;

        anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, canvasBounds.xMin + 8f, canvasBounds.xMax - tooltipSize.x - 8f);
        anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, canvasBounds.yMin + tooltipSize.y + 8f, canvasBounds.yMax - 8f);

        tooltipRect.anchoredPosition = anchoredPosition;
    }

    public void HideArtefactTooltip()
    {
        if(tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    private void EnsureTooltip()
    {
        if(tooltipObject != null)
            return;

        Transform parent = shop != null ? shop.transform : transform;
        tooltipObject = new GameObject("Artefact Tooltip", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        tooltipObject.transform.SetParent(parent, false);

        tooltipRect = tooltipObject.GetComponent<RectTransform>();
        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0f, 1f);
        tooltipRect.sizeDelta = new Vector2(310f, 118f);

        Image background = tooltipObject.GetComponent<Image>();
        background.color = new Color(0.04f, 0.035f, 0.03f, 0.96f);

        CanvasGroup canvasGroup = tooltipObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        tooltipTitleText = CreateTooltipText("Title", tooltipObject.transform, new Vector2(14f, -38f), new Vector2(-14f, -10f), 18f, FontStyles.Bold);
        tooltipBodyText = CreateTooltipText("Body", tooltipObject.transform, new Vector2(14f, 10f), new Vector2(-14f, -42f), 14f, FontStyles.Normal);
        tooltipObject.SetActive(false);
    }

    private TMP_Text CreateTooltipText(string objectName, Transform parent, Vector2 offsetMin, Vector2 offsetMax, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;

        return text;
    }

    private string GetSpellTooltipText(SpellData spell)
    {
        if(spell == null)
            return string.Empty;

        switch(spell.effectType)
        {
            case PassiveSpellEffectType.HealOnAttack:
                return GetHealTooltip("Ogni colpo andato a segno ruba un sorso di vita.", spell.value);
            case PassiveSpellEffectType.HealOnKill:
                return GetHealTooltip("Quando un nemico cade, il sangue torna a scorrere dalla tua parte.", spell.value);
            case PassiveSpellEffectType.MaxHealthBonus:
                return "Incide una runa di resistenza sul cuore: +" + FormatValue(spell.value) + " PV massimi.";
            case PassiveSpellEffectType.SpeedBonus:
                return "Alleggerisce armatura e passi: +" + FormatValue(spell.value) + " velocita'.";
            case PassiveSpellEffectType.DamageBonus:
                return GetDamageTooltip(spell.value);
            case PassiveSpellEffectType.ExtraCoinOnKill:
                return "Ogni nemico sconfitto lascia cadere " + Mathf.RoundToInt(spell.value) + " coin extra.";
            default:
                return string.Empty;
        }
    }

    private string GetItemTooltipText(ItemData item)
    {
        if(item == null)
            return string.Empty;

        switch(item.effectType)
        {
            case ItemEffectType.Heal:
                return GetHealTooltip("Bevila al momento giusto: richiude le ferite prima che diventino destino.", item.value);
            case ItemEffectType.RestoreShield:
                return "Rinforza lo scudo con una carica limpida: +" + FormatValue(item.value) + " scudo.";
            case ItemEffectType.SpeedBoost:
                return "Per " + FormatValue(item.duration) + " secondi ti muovi come una lama nel vento: +" + FormatValue(item.value) + " velocita'.";
            case ItemEffectType.DamageBoost:
                return "Per " + FormatValue(item.duration) + " secondi ogni fendente pesa di piu': +" + FormatValue(item.value) + " danni.";
            case ItemEffectType.MaxHealthBoost:
                return "Tempra il corpo in modo permanente: +" + FormatValue(item.value) + " PV massimi.";
            case ItemEffectType.AddCoins:
                return "Una piccola fortuna pronta in tasca: +" + Mathf.RoundToInt(item.value) + " coin.";
            default:
                return string.Empty;
        }
    }

    private string GetHealTooltip(string intro, float percent)
    {
        if(player == null)
            return intro + " Cura il " + FormatValue(percent) + "% della vita massima.";

        float healAmount = player.VitaMassima * (percent / 100f);
        return intro + " Cura il " + FormatValue(percent) + "% della vita massima: circa +" + FormatValue(healAmount) + " PV.";
    }

    private string GetDamageTooltip(float multiplier)
    {
        if(player == null)
            return "La staffa carica i colpi: danni x" + FormatValue(multiplier) + ".";

        float finalDamage = player.danno * multiplier;
        return "La staffa carica i colpi: " + FormatValue(player.danno) + " danni diventano " + FormatValue(finalDamage) + ".";
    }

    private string FormatValue(float number)
    {
        return number.ToString("0.##");
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
            totalText.text = Mathf.RoundToInt(total).ToString() + " coin";

        UpdatePlayerCoinText();
        UpdateBuyButton();
    }
}
