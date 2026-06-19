using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryMenager : MonoBehaviour
{
    public static InventoryMenager Instance { get; private set; }

    private static bool hasSavedInventory;
    private static readonly List<ItemData> savedItems = new List<ItemData>();
    private static readonly List<SpellData> savedOwnedSpells = new List<SpellData>();
    private static SpellData[] savedEquippedSlots = new SpellData[0];

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

    [Header("Spell Slots")]
    [Tooltip("Livello minimo richiesto per ogni spell slot, nello stesso ordine degli slot UI. Il primo slot viene sempre tenuto a livello 0.")]
    [SerializeField] private int[] spellSlotRequiredLevels = new int[] { 0, 0, 0 };
    [Tooltip("Opzionale: sprite da mostrare sopra gli spell slot bloccati. Se resta vuoto, viene disegnato un lucchetto semplice via UI.")]
    [SerializeField] private Sprite spellSlotLockSprite;

    private readonly List<ItemData> items = new List<ItemData>();
    private readonly List<SpellData> ownedSpellsWithoutInventory = new List<SpellData>();
    private readonly List<Button> spellSelectionButtons = new List<Button>();

    private Button[] itemButtons = new Button[0];
    private Button[] spellSlotButtons = new Button[0];
    private GameObject[] spellSlotLockIcons = new GameObject[0];
    private Button consumeButton;
    private TMP_Text playerCoinText;
    private GameObject spellSelectionPanel;
    private SpellData[] equippedSlots = new SpellData[0];

    private int selectedItemIndex = -1;
    private int selectedSpellSlot = -1;
    private int lastKnownPlayerLevel = -1;
    private bool isLoadingSavedInventory;
    private GraphicRaycaster graphicRaycaster;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            return;

        Instance = this;
        FindMissingReferences();
        LoadSavedInventory();
        ConnectButtons();
        RefreshAll();
    }

    private void OnDisable()
    {
        if(Instance == this)
            SaveInventory();
    }

    private void OnDestroy()
    {
        if(Instance != this)
            return;

        SaveInventory();
        Instance = null;
    }

    private void Start()
    {
        if(hideOnStart)
            SetInventoryVisible(false);
    }

    private void OnEnable()
    {
        FindMissingReferences();
        LoadSavedInventory();
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

        UpdateSpellSlotLocks();
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
        SaveInventory();
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

        if(added)
            AddUniqueSpell(savedOwnedSpells, spell);

        RefreshSpells();
        SaveInventory();
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
        SyncSpellSlotRequiredLevels();
        EnsureSpellSlotLockIcons();

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
            spellSlotButtons[i].onClick.AddListener(() =>
            {
                if(IsSpellSlotUnlocked(index))
                    ShowSpellSelection(index);
            });
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
        ItemData selectedItem = GetItemAt(selectedItemIndex);

        if(selectedItem == null)
            return;

        if(!selectedItem.Consume(player, this))
            return;

        items.RemoveAt(selectedItemIndex);
        selectedItemIndex = -1;
        RefreshItems();
        SaveInventory();
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
        if(!IsSpellSlotUnlocked(slotIndex))
            return;

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

        List<SpellData> ownedSpells = GetSelectableSpells();

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

            if(equippedSlot >= 0)
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
            SaveInventory();
            return;
        }

        if(GetEquippedSlot(spell) >= 0)
            return;

        UnequipSpellFromSlot(selectedSpellSlot);

        if(spellManager == null || spellManager.EquipSpell(spell))
            equippedSlots[selectedSpellSlot] = spell;

        ClearSelection();
        RefreshSpells();
        SaveInventory();
    }

    private void UnequipSpellFromSlot(int slotIndex)
    {
        if(slotIndex < 0 || slotIndex >= equippedSlots.Length)
            return;

        SpellData oldSpell = equippedSlots[slotIndex];

        if(oldSpell != null && spellManager != null)
            spellManager.UnequipSpell(oldSpell);

        equippedSlots[slotIndex] = null;
        SaveInventory();
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

    private ItemData GetItemAt(int index)
    {
        if(index < 0 || index >= items.Count)
            return null;

        return items[index];
    }

    private void SaveInventory()
    {
        if(isLoadingSavedInventory)
            return;

        hasSavedInventory = true;

        savedItems.Clear();

        foreach(ItemData item in items)
        {
            if(item != null)
                savedItems.Add(item);
        }

        foreach(SpellData spell in GetSelectableSpells())
            AddUniqueSpell(savedOwnedSpells, spell);

        savedEquippedSlots = new SpellData[equippedSlots != null ? equippedSlots.Length : 0];

        for(int i = 0; i < savedEquippedSlots.Length; i++)
            savedEquippedSlots[i] = equippedSlots[i];
    }

    private void LoadSavedInventory()
    {
        if(!hasSavedInventory)
            return;

        isLoadingSavedInventory = true;
        items.Clear();

        int itemLimit = itemButtons != null && itemButtons.Length > 0 ? Mathf.Min(maxItems, itemButtons.Length) : maxItems;

        for(int i = 0; i < savedItems.Count && i < itemLimit; i++)
        {
            if(savedItems[i] != null)
                items.Add(savedItems[i]);
        }

        ownedSpellsWithoutInventory.Clear();

        foreach(SpellData spell in savedOwnedSpells)
        {
            AddUniqueSpell(ownedSpellsWithoutInventory, spell);

            if(spellInventory != null && spell != null && !spellInventory.OwnsSpell(spell))
                spellInventory.AddSpell(spell);
        }

        if(equippedSlots == null || equippedSlots.Length != spellSlotButtons.Length)
            equippedSlots = new SpellData[spellSlotButtons.Length];

        ClearRuntimeEquippedSpells();

        for(int i = 0; i < equippedSlots.Length; i++)
        {
            SpellData spell = i < savedEquippedSlots.Length ? savedEquippedSlots[i] : null;

            if(spell == null || GetEquippedSlot(spell) >= 0 || !IsSpellSlotUnlocked(i))
                continue;

            AddUniqueSpell(savedOwnedSpells, spell);
            AddUniqueSpell(ownedSpellsWithoutInventory, spell);

            if(spellInventory != null && !spellInventory.OwnsSpell(spell))
                spellInventory.AddSpell(spell);

            if(spellManager == null || spellManager.EquipSpell(spell) || (spellInventory != null && spellInventory.IsEquipped(spell)))
                equippedSlots[i] = spell;
        }

        isLoadingSavedInventory = false;
    }

    private void ClearRuntimeEquippedSpells()
    {
        if(spellInventory == null)
        {
            for(int i = 0; i < equippedSlots.Length; i++)
                equippedSlots[i] = null;

            return;
        }

        List<SpellData> equippedCopy = new List<SpellData>();

        foreach(SpellData spell in spellInventory.EquippedSpells)
            AddUniqueSpell(equippedCopy, spell);

        foreach(SpellData spell in equippedCopy)
        {
            if(spellManager != null)
                spellManager.UnequipSpell(spell);
            else
                spellInventory.UnequipSpell(spell);
        }

        for(int i = 0; i < equippedSlots.Length; i++)
            equippedSlots[i] = null;
    }

    private List<SpellData> GetSelectableSpells()
    {
        List<SpellData> selectableSpells = new List<SpellData>();

        foreach(SpellData spell in GetOwnedSpells())
            AddUniqueSpell(selectableSpells, spell);

        if(equippedSlots != null)
        {
            foreach(SpellData spell in equippedSlots)
                AddUniqueSpell(selectableSpells, spell);
        }

        return selectableSpells;
    }

    private void AddUniqueSpell(List<SpellData> spells, SpellData spell)
    {
        if(spell != null && !spells.Contains(spell))
            spells.Add(spell);
    }

    private void RefreshItems()
    {
        for(int i = 0; i < itemButtons.Length; i++)
        {
            ItemData item = GetItemAt(i);
            SetButtonContent(itemButtons[i], item != null ? item.icon : null, item != null ? item.Name : "");
            itemButtons[i].interactable = item != null;
            SetButtonSelected(itemButtons[i], i == selectedItemIndex);
        }

        if(consumeButton != null)
            consumeButton.interactable = GetItemAt(selectedItemIndex) != null;
    }

    private void RefreshSpellSlots()
    {
        for(int i = 0; i < spellSlotButtons.Length; i++)
        {
            SpellData spell = i < equippedSlots.Length ? equippedSlots[i] : null;
            SetButtonContent(spellSlotButtons[i], spell != null ? spell.icon : null, spell != null ? spell.spellName : "");
            SetSpellSlotLocked(spellSlotButtons[i], i, !IsSpellSlotUnlocked(i), spell != null);
            SetButtonSelected(spellSlotButtons[i], i == selectedSpellSlot && IsSpellSlotUnlocked(i), false);
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
            image.preserveAspect = icon != null;
            image.color = icon != null ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        }

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);

        if(tmpText != null)
            tmpText.text = label;

        UnityEngine.UI.Text uiText = button.GetComponentInChildren<UnityEngine.UI.Text>(true);

        if(uiText != null)
            uiText.text = label;
    }

    private void SetButtonSelected(Button button, bool selected, bool resetUnselectedColor = true)
    {
        if(button == null)
            return;

        if(!button.interactable && !selected)
            return;

        ColorBlock colors = button.colors;
        colors.normalColor = selected ? new Color(1f, 0.86f, 0.38f, 1f) : resetUnselectedColor ? Color.white : colors.normalColor;
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
    }

    private void UpdateSpellSlotLocks()
    {
        int playerLevel = GetPlayerLevel();

        if(playerLevel == lastKnownPlayerLevel)
            return;

        lastKnownPlayerLevel = playerLevel;

        if(selectedSpellSlot >= 0 && !IsSpellSlotUnlocked(selectedSpellSlot))
            selectedSpellSlot = -1;

        RefreshSpellSlots();
    }

    private void SyncSpellSlotRequiredLevels()
    {
        if(spellSlotButtons == null)
            return;

        if(spellSlotRequiredLevels == null)
            spellSlotRequiredLevels = new int[spellSlotButtons.Length];

        if(spellSlotRequiredLevels.Length != spellSlotButtons.Length)
        {
            int[] resizedLevels = new int[spellSlotButtons.Length];

            for(int i = 0; i < resizedLevels.Length; i++)
                resizedLevels[i] = i < spellSlotRequiredLevels.Length ? spellSlotRequiredLevels[i] : 0;

            spellSlotRequiredLevels = resizedLevels;
        }

        if(spellSlotRequiredLevels.Length > 0)
            spellSlotRequiredLevels[0] = 0;

        for(int i = 1; i < spellSlotRequiredLevels.Length; i++)
            spellSlotRequiredLevels[i] = Mathf.Max(0, spellSlotRequiredLevels[i]);
    }

    private void EnsureSpellSlotLockIcons()
    {
        if(spellSlotButtons == null)
            return;

        if(spellSlotLockIcons == null || spellSlotLockIcons.Length != spellSlotButtons.Length)
            spellSlotLockIcons = new GameObject[spellSlotButtons.Length];

        for(int i = 0; i < spellSlotButtons.Length; i++)
        {
            if(spellSlotButtons[i] == null || spellSlotLockIcons[i] != null)
                continue;

            spellSlotLockIcons[i] = CreateLockIcon(spellSlotButtons[i].transform);
        }
    }

    private GameObject CreateLockIcon(Transform parent)
    {
        GameObject lockRoot = new GameObject("Lock Icon", typeof(RectTransform), typeof(CanvasGroup));
        lockRoot.transform.SetParent(parent, false);

        RectTransform rootRect = lockRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(28f, 28f);

        CanvasGroup canvasGroup = lockRoot.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        if(spellSlotLockSprite != null)
        {
            Image lockImage = lockRoot.AddComponent<Image>();
            lockImage.sprite = spellSlotLockSprite;
            lockImage.preserveAspect = true;
            lockImage.color = new Color(1f, 1f, 1f, 0.92f);
            lockImage.raycastTarget = false;
            return lockRoot;
        }

        CreateLockPart("Body", lockRoot.transform, new Vector2(0f, -5f), new Vector2(18f, 14f), new Color(1f, 1f, 1f, 0.9f));
        CreateLockPart("Shackle Left", lockRoot.transform, new Vector2(-7f, 4f), new Vector2(4f, 12f), new Color(1f, 1f, 1f, 0.9f));
        CreateLockPart("Shackle Right", lockRoot.transform, new Vector2(7f, 4f), new Vector2(4f, 12f), new Color(1f, 1f, 1f, 0.9f));
        CreateLockPart("Shackle Top", lockRoot.transform, new Vector2(0f, 10f), new Vector2(18f, 4f), new Color(1f, 1f, 1f, 0.9f));

        return lockRoot;
    }

    private void CreateLockPart(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject partObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        partObject.transform.SetParent(parent, false);

        RectTransform rectTransform = partObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = partObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private void SetSpellSlotLocked(Button button, int slotIndex, bool locked, bool hasContent)
    {
        if(button == null)
            return;

        button.interactable = !locked;

        Color visibleColor = locked ? new Color(1f, 1f, 1f, 0.25f) : hasContent ? Color.white : new Color(1f, 1f, 1f, 0.45f);
        SetButtonGraphicAlpha(button, visibleColor);

        ColorBlock colors = button.colors;
        colors.normalColor = visibleColor;
        colors.highlightedColor = locked ? visibleColor : Color.white;
        colors.pressedColor = locked ? visibleColor : new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = visibleColor;
        colors.disabledColor = visibleColor;
        button.colors = colors;

        if(spellSlotLockIcons != null && slotIndex >= 0 && slotIndex < spellSlotLockIcons.Length && spellSlotLockIcons[slotIndex] != null)
            spellSlotLockIcons[slotIndex].SetActive(locked);
    }

    private void SetButtonGraphicAlpha(Button button, Color color)
    {
        Image image = button.targetGraphic as Image;

        if(image == null)
            image = button.GetComponent<Image>();

        if(image != null)
            image.color = color;

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);

        if(tmpText != null)
            tmpText.color = color;

        UnityEngine.UI.Text uiText = button.GetComponentInChildren<UnityEngine.UI.Text>(true);

        if(uiText != null)
            uiText.color = color;
    }

    private bool IsSpellSlotUnlocked(int slotIndex)
    {
        if(slotIndex < 0)
            return false;

        if(spellSlotRequiredLevels == null || slotIndex >= spellSlotRequiredLevels.Length)
            return true;

        return GetPlayerLevel() >= spellSlotRequiredLevels[slotIndex];
    }

    private int GetPlayerLevel()
    {
        if(player == null)
            return 0;

        return Mathf.FloorToInt(player.livello);
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
        rectTransform.sizeDelta = new Vector2(280f, 172f);

        Image image = spellSelectionPanel.GetComponent<Image>();
        image.color = new Color(0.015f, 0.015f, 0.018f, 0.94f);
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
        rectTransform.anchoredPosition = new Vector2(0f, -8f - index * 42f);
        rectTransform.sizeDelta = new Vector2(-16f, 36f);

        Image background = buttonObject.GetComponent<Image>();
        background.color = interactable ? new Color(0.03f, 0.03f, 0.035f, 0.96f) : new Color(0.03f, 0.03f, 0.035f, 0.45f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.interactable = interactable;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(buttonObject.transform, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(22f, 0f);
        iconRect.sizeDelta = new Vector2(32f, 32f);

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = spell != null ? spell.icon : null;
        iconImage.preserveAspect = true;
        iconImage.color = spell != null && interactable ? Color.white : new Color(1f, 1f, 1f, 0.35f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(spell != null ? 48f : 10f, 0f);
        labelRect.offsetMax = new Vector2(-10f, 0f);

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 14f;
        text.alignment = spell != null ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center;
        text.color = Color.white;

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
