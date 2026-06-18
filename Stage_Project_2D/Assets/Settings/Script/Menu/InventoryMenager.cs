using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryMenager : MonoBehaviour
{
    public static InventoryMenager Instance { get; private set; }

    [Header("Auto References")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PassiveSpellManager spellManager;
    [SerializeField] private PassiveSpellInventory spellInventory;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Q;
    [SerializeField] private bool hideOnStart = true;

    [Header("Items")]
    [SerializeField] private int maxItems = 10;

    private readonly List<ItemData> items = new List<ItemData>();
    private readonly List<SpellData> ownedSpellsWithoutInventory = new List<SpellData>();
    private readonly List<Button> spellSelectionButtons = new List<Button>();

    private Button[] itemButtons = new Button[0];
    private Button[] spellSlotButtons = new Button[0];
    private Button consumeButton;
    private TMP_Text playerCoinText;
    private GameObject spellSelectionPanel;
    private SpellData[] equippedSlots = new SpellData[0];

    private int selectedItemIndex = -1;
    private int selectedSpellSlot = -1;
    private GraphicRaycaster graphicRaycaster;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            return;

        Instance = this;
        FindMissingReferences();
        ConnectButtons();
        RefreshAll();
    }

    private void Start()
    {
        if(hideOnStart)
            SetInventoryVisible(false);
    }

    private void OnEnable()
    {
        FindMissingReferences();
        ConnectButtons();
        RefreshAll();
    }

    private void Update()
    {
        if(Input.GetKeyDown(toggleKey))
        {
            ToggleInventoryCanvas();
            return;
        }

        if(IsInventoryVisible() && Input.GetMouseButtonDown(0) && !IsPointerOverInventoryControl())
            ClearSelection();

        UpdatePlayerCoinText();
    }

    public bool CanAddItem(ItemData item, int amount = 1)
    {
        if(item == null || amount <= 0)
            return false;

        return items.Count + amount <= maxItems && items.Count + amount <= itemButtons.Length;
    }

    public bool AddItem(ItemData item)
    {
        if(!CanAddItem(item))
            return false;

        items.Add(item);
        selectedItemIndex = -1;
        RefreshItems();
        return true;
    }

    public bool CanAddSpell(SpellData spell)
    {
        if(spell == null)
            return false;

        if(spellInventory != null)
            return !spellInventory.OwnsSpell(spell);

        return !ownedSpellsWithoutInventory.Contains(spell);
    }

    public bool AddSpell(SpellData spell)
    {
        if(spell == null)
            return false;

        bool added = false;

        if(spellInventory != null)
        {
            added = spellInventory.OwnsSpell(spell) || spellInventory.AddSpell(spell);
        }
        else if(!ownedSpellsWithoutInventory.Contains(spell))
        {
            ownedSpellsWithoutInventory.Add(spell);
            added = true;
        }

        RefreshSpells();
        return added;
    }

    public void RefreshAll()
    {
        SyncEquippedSlotsFromInventory();
        RefreshItems();
        RefreshSpells();
        UpdatePlayerCoinText();
    }

    public void RefreshSpells()
    {
        RefreshSpellSlots();

        if(selectedSpellSlot >= 0)
            ShowSpellSelection(selectedSpellSlot);
    }

    private void FindMissingReferences()
    {
        if(inventoryCanvas == null)
            inventoryCanvas = GetComponent<Canvas>();

        if(inventoryCanvas == null)
            inventoryCanvas = GetComponentInParent<Canvas>();

        if(graphicRaycaster == null && inventoryCanvas != null)
            graphicRaycaster = inventoryCanvas.GetComponent<GraphicRaycaster>();

        if(player == null)
            player = FindObjectOfType<PlayerMovement>();

        if(spellManager == null)
            spellManager = FindObjectOfType<PassiveSpellManager>();

        if(spellInventory == null)
            spellInventory = FindObjectOfType<PassiveSpellInventory>();

        Transform root = inventoryCanvas != null ? inventoryCanvas.transform : transform;
        Transform itemsPanel = FindChildByName(root, "Items Panel");
        Transform spellSlotsPanel = FindChildByName(root, "Spell's Slot");

        if(spellSlotsPanel == null)
            spellSlotsPanel = FindChildByName(root, "Spells Slot");

        itemButtons = FindNumberedButtons(itemsPanel != null ? itemsPanel : root, "Item");
        spellSlotButtons = FindNumberedButtons(spellSlotsPanel != null ? spellSlotsPanel : root, "Spell");

        maxItems = Mathf.Max(maxItems, itemButtons.Length);

        if(equippedSlots == null || equippedSlots.Length != spellSlotButtons.Length)
            equippedSlots = new SpellData[spellSlotButtons.Length];

        if(consumeButton == null)
            consumeButton = FindButtonContaining(root, "consume");

        if(playerCoinText == null)
            playerCoinText = FindPlayerCoinText(root);
    }

    private void ConnectButtons()
    {
        for(int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;
            itemButtons[i].onClick.RemoveAllListeners();
            itemButtons[i].onClick.AddListener(() => SelectItem(index));
        }

        for(int i = 0; i < spellSlotButtons.Length; i++)
        {
            int index = i;
            spellSlotButtons[i].onClick.RemoveAllListeners();
            spellSlotButtons[i].onClick.AddListener(() => ShowSpellSelection(index));
        }

        if(consumeButton != null)
        {
            consumeButton.onClick.RemoveAllListeners();
            consumeButton.onClick.AddListener(ConsumeSelectedItem);
        }
    }

    private void SelectItem(int index)
    {
        selectedSpellSlot = -1;
        HideSpellSelectionPanel();

        if(index < 0 || index >= items.Count)
        {
            selectedItemIndex = -1;
        }
        else if(selectedItemIndex == index)
        {
            selectedItemIndex = -1;
        }
        else
        {
            selectedItemIndex = index;
        }

        RefreshItems();
        RefreshSpellSlots();
        ClearEventSystemSelection();
    }

    private void ConsumeSelectedItem()
    {
        if(selectedItemIndex < 0 || selectedItemIndex >= items.Count)
            return;

        if(!items[selectedItemIndex].Consume(player, this))
            return;

        items.RemoveAt(selectedItemIndex);
        selectedItemIndex = -1;
        RefreshItems();
    }

    private void ToggleInventoryCanvas()
    {
        if(inventoryCanvas == null)
            return;

        SetInventoryVisible(!inventoryCanvas.enabled);
    }

    private void SetInventoryVisible(bool visible)
    {
        if(inventoryCanvas == null)
            return;

        inventoryCanvas.enabled = visible;

        GraphicRaycaster raycaster = inventoryCanvas.GetComponent<GraphicRaycaster>();

        if(raycaster != null)
            raycaster.enabled = visible;

        if(!visible)
            ClearSelection();
    }

    private bool IsInventoryVisible()
    {
        return inventoryCanvas != null && inventoryCanvas.enabled;
    }

    private bool IsPointerOverInventoryControl()
    {
        if(EventSystem.current == null || graphicRaycaster == null)
            return false;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach(RaycastResult result in results)
        {
            GameObject hitObject = result.gameObject;

            if(IsButtonOrChild(hitObject, consumeButton))
                return true;

            foreach(Button button in itemButtons)
                if(IsButtonOrChild(hitObject, button))
                    return true;

            foreach(Button button in spellSlotButtons)
                if(IsButtonOrChild(hitObject, button))
                    return true;

            foreach(Button button in spellSelectionButtons)
                if(IsButtonOrChild(hitObject, button))
                    return true;
        }

        return false;
    }

    private bool IsButtonOrChild(GameObject hitObject, Button button)
    {
        if(hitObject == null || button == null)
            return false;

        return hitObject == button.gameObject || hitObject.transform.IsChildOf(button.transform);
    }

    private void ClearSelection()
    {
        selectedItemIndex = -1;
        selectedSpellSlot = -1;
        HideSpellSelectionPanel();
        RefreshItems();
        RefreshSpellSlots();
        ClearEventSystemSelection();
    }

    private void HideSpellSelectionPanel()
    {
        if(spellSelectionPanel != null)
            spellSelectionPanel.SetActive(false);
    }

    private void ClearEventSystemSelection()
    {
        if(EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void ShowSpellSelection(int slotIndex)
    {
        selectedItemIndex = -1;

        if(selectedSpellSlot == slotIndex)
        {
            ClearSelection();
            return;
        }

        selectedSpellSlot = slotIndex;
        EnsureSpellSelectionPanel();

        if(spellSelectionPanel == null)
            return;

        foreach(Button button in spellSelectionButtons)
            Destroy(button.gameObject);

        spellSelectionButtons.Clear();
        spellSelectionPanel.SetActive(true);
        RefreshItems();
        RefreshSpellSlots();
        ClearEventSystemSelection();

        IReadOnlyList<SpellData> ownedSpells = GetOwnedSpells();

        if(ownedSpells.Count == 0)
        {
            CreateSpellSelectionButton(null, "No spells", false);
            return;
        }

        foreach(SpellData spell in ownedSpells)
        {
            int equippedSlot = GetEquippedSlot(spell);
            bool isCurrentSlotSpell = equippedSlot == selectedSpellSlot;
            bool canSelect = equippedSlot < 0 || isCurrentSlotSpell;
            string label = GetSpellName(spell);

            if(isCurrentSlotSpell)
                label += " (Equipped)";

            SpellData selectedSpell = spell;
            Button button = CreateSpellSelectionButton(selectedSpell, label, canSelect);
            button.onClick.AddListener(() => ToggleSpellInSelectedSlot(selectedSpell));
        }
    }

    private void ToggleSpellInSelectedSlot(SpellData spell)
    {
        if(spell == null || selectedSpellSlot < 0 || selectedSpellSlot >= equippedSlots.Length)
            return;

        if(equippedSlots[selectedSpellSlot] == spell)
        {
            UnequipSpellFromSlot(selectedSpellSlot);
            ClearSelection();
            RefreshSpells();
            return;
        }

        if(GetEquippedSlot(spell) >= 0)
            return;

        UnequipSpellFromSlot(selectedSpellSlot);

        if(spellManager == null || spellManager.EquipSpell(spell))
            equippedSlots[selectedSpellSlot] = spell;

        ClearSelection();
        RefreshSpells();
    }

    private void UnequipSpellFromSlot(int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= equippedSlots.Length)
            return;

        SpellData oldSpell = equippedSlots[slotIndex];

        if(oldSpell != null && spellManager != null)
            spellManager.UnequipSpell(oldSpell);

        equippedSlots[slotIndex] = null;
    }

    private void SyncEquippedSlotsFromInventory()
    {
        if(spellInventory == null || equippedSlots == null)
            return;

        int slotIndex = 0;

        foreach(SpellData spell in spellInventory.EquippedSpells)
        {
            if(spell == null || GetEquippedSlot(spell) >= 0 || slotIndex >= equippedSlots.Length)
                continue;

            while(slotIndex < equippedSlots.Length && equippedSlots[slotIndex] != null)
                slotIndex++;

            if(slotIndex < equippedSlots.Length)
                equippedSlots[slotIndex] = spell;
        }
    }

    private void RefreshItems()
    {
        for(int i = 0; i < itemButtons.Length; i++)
        {
            ItemData item = i < items.Count ? items[i] : null;
            SetButtonContent(itemButtons[i], item != null ? item.icon : null, item != null ? item.Name : "");
            itemButtons[i].interactable = item != null;
            SetButtonSelected(itemButtons[i], i == selectedItemIndex);
        }

        if(consumeButton != null)
            consumeButton.interactable = selectedItemIndex >= 0 && selectedItemIndex < items.Count;
    }

    private void RefreshSpellSlots()
    {
        for(int i = 0; i < spellSlotButtons.Length; i++)
        {
            SpellData spell = i < equippedSlots.Length ? equippedSlots[i] : null;
            SetButtonContent(spellSlotButtons[i], spell != null ? spell.icon : null, spell != null ? spell.spellName : "");
            SetButtonSelected(spellSlotButtons[i], i == selectedSpellSlot);
        }
    }

    private void SetButtonContent(Button button, Sprite icon, string label)
    {
        if(button == null)
            return;

        Image image = button.targetGraphic as Image;

        if(image == null)
            image = button.GetComponent<Image>();

        if(image != null)
        {
            image.sprite = icon;
            image.color = icon != null ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);

        if(tmpText != null)
            tmpText.text = label;

        UnityEngine.UI.Text uiText = button.GetComponentInChildren<UnityEngine.UI.Text>(true);

        if(uiText != null)
            uiText.text = label;
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        if(button == null)
            return;

        ColorBlock colors = button.colors;
        colors.normalColor = selected ? new Color(1f, 0.86f, 0.38f, 1f) : Color.white;
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
    }

    private void EnsureSpellSelectionPanel()
    {
        if(spellSelectionPanel != null)
            return;

        Transform parent = inventoryCanvas != null ? inventoryCanvas.transform : transform;
        spellSelectionPanel = new GameObject("Spell Selection Panel", typeof(RectTransform), typeof(Image));
        spellSelectionPanel.transform.SetParent(parent, false);

        RectTransform rectTransform = spellSelectionPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -10f);
        rectTransform.sizeDelta = new Vector2(220f, 160f);

        Image image = spellSelectionPanel.GetComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
    }

    private Button CreateSpellSelectionButton(SpellData spell, string label, bool interactable)
    {
        GameObject buttonObject = new GameObject("Owned Spell", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(spellSelectionPanel.transform, false);

        int index = spellSelectionButtons.Count;
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -8f - index * 34f);
        rectTransform.sizeDelta = new Vector2(-16f, 28f);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = spell != null ? spell.icon : null;
        image.color = interactable ? Color.white : new Color(1f, 1f, 1f, 0.35f);

        Button button = buttonObject.GetComponent<Button>();
        button.interactable = interactable;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 14f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

        spellSelectionButtons.Add(button);
        return button;
    }

    private IReadOnlyList<SpellData> GetOwnedSpells()
    {
        if(spellInventory != null)
            return spellInventory.OwnedSpells;

        return ownedSpellsWithoutInventory;
    }

    private int GetEquippedSlot(SpellData spell)
    {
        if(spell == null || equippedSlots == null)
            return -1;

        for(int i = 0; i < equippedSlots.Length; i++)
        {
            if(equippedSlots[i] == spell)
                return i;
        }

        return -1;
    }

    private void UpdatePlayerCoinText()
    {
        if(player != null && playerCoinText != null)
            playerCoinText.text = player.CoinSetGet.ToString();
    }

    private TMP_Text FindPlayerCoinText(Transform root)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);

        foreach(TMP_Text text in texts)
        {
            string objectName = text.gameObject.name.ToLower();
            string parentName = text.transform.parent != null ? text.transform.parent.name.ToLower() : "";

            if((objectName.Contains("amount") || objectName.Contains("ammount")) && parentName.Contains("coin"))
                return text;
        }

        foreach(TMP_Text text in texts)
        {
            if(text.gameObject.name.ToLower().Contains("coin"))
                return text;
        }

        return null;
    }

    private Button[] FindNumberedButtons(Transform root, string prefix)
    {
        if(root == null)
            return new Button[0];

        List<Button> foundButtons = new List<Button>();
        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        foreach(Button button in buttons)
        {
            string objectName = button.gameObject.name;

            if(objectName.ToLower().StartsWith(prefix.ToLower()) && GetLastNumber(objectName) >= 0)
                foundButtons.Add(button);
        }

        foundButtons.Sort((first, second) => GetLastNumber(first.gameObject.name).CompareTo(GetLastNumber(second.gameObject.name)));
        return foundButtons.ToArray();
    }

    private Button FindButtonContaining(Transform root, string text)
    {
        if(root == null)
            return null;

        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        foreach(Button button in buttons)
        {
            if(button.gameObject.name.ToLower().Contains(text.ToLower()))
                return button;
        }

        return null;
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        if(root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach(Transform child in children)
        {
            if(child.name == childName)
                return child;
        }

        return null;
    }

    private string GetSpellName(SpellData spell)
    {
        if(spell == null)
            return "";

        return string.IsNullOrWhiteSpace(spell.spellName) ? spell.name : spell.spellName;
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
}
