using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class HoldStatsUpgradeMenu : MonoBehaviour
{
    [Header("Input")]
    public KeyCode menuKey = KeyCode.G;

    [Header("References")]
    public GameObject menuPanel;
    public PlayerUpgradeStats playerStats;

    [Header("Texts")]
    public Text upgradePointsText;
    public Text swordStatsText;
    public Text shieldStatsText;
    public Text characterStatsText;

    [Header("Buttons")]
    public Button upgradeSwordButton;
    public Button upgradeShieldButton;
    public Button upgradeSpeedButton;

    private bool isOpen;
    private static HoldStatsUpgradeMenu instance;
    private static readonly Vector2 PanelSize = new Vector2(360f, 300f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeOnFirstScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureMenuExists();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureMenuExists();
    }

    private static void EnsureMenuExists()
    {
        if(instance != null)
            return;

        instance = FindObjectOfType<HoldStatsUpgradeMenu>();

        if(instance != null)
            return;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if(player == null)
            return;

        GameObject menuObject = new GameObject("Stats Upgrade Menu");
        instance = menuObject.AddComponent<HoldStatsUpgradeMenu>();
        instance.playerStats = player.GetComponent<PlayerUpgradeStats>();

        if(instance.playerStats == null)
            instance.playerStats = player.gameObject.AddComponent<PlayerUpgradeStats>();

        instance.CreateRuntimeUI();
    }

    private void Start()
    {
        if(instance == null)
            instance = this;

        if(playerStats == null)
        {
            PlayerMovement player = FindObjectOfType<PlayerMovement>();

            if(player != null)
            {
                playerStats = player.GetComponent<PlayerUpgradeStats>();

                if(playerStats == null)
                    playerStats = player.gameObject.AddComponent<PlayerUpgradeStats>();
            }
        }

        if(menuPanel == null)
            CreateRuntimeUI();

        HideSpeedUpgradeButton();
        AssignFallbackFonts();

        if(!Application.isPlaying)
            return;

        SetMenuOpen(false);
    }

    private void OnValidate()
    {
        HideSpeedUpgradeButton();
        AssignFallbackFonts();
    }

    private void Update()
    {
        if(!Application.isPlaying)
            return;

        if(Time.timeScale == 0f)
        {
            if(isOpen)
                SetMenuOpen(false);

            return;
        }

        bool shouldBeOpen = Input.GetKey(menuKey);

        if(shouldBeOpen != isOpen)
        {
            SetMenuOpen(shouldBeOpen);
        }

        if(isOpen)
        {
            Refresh();
        }
    }

    public void UpgradeSword()
    {
        if(playerStats != null)
        {
            playerStats.UpgradeSword();
            Refresh();
        }
    }

    public void UpgradeShield()
    {
        if(playerStats != null)
        {
            playerStats.UpgradeShield();
            Refresh();
        }
    }

    public void UpgradeSpeed()
    {
        HideSpeedUpgradeButton();
    }

    private void SetMenuOpen(bool open)
    {
        isOpen = open;

        if(menuPanel != null)
        {
            menuPanel.SetActive(open);
        }

        if(open)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if(playerStats == null)
            return;

        if(upgradePointsText != null)
        {
            upgradePointsText.text =
                "Livello: " + playerStats.PlayerLevel + "   Punti: " + playerStats.upgradePoints + "\n" +
                "XP: " + playerStats.CurrentXP + "/" + playerStats.NextLevelXP;
        }

        if(swordStatsText != null)
        {
            swordStatsText.text =
                "Spada\n" +
                "Livello: " + playerStats.swordLevel + "/" + playerStats.maxSwordLevel + "\n" +
                "Danno: " + playerStats.SwordDamage;
        }

        if(shieldStatsText != null)
        {
            shieldStatsText.text =
                "Scudo\n" +
                "Livello: " + playerStats.shieldLevel + "/" + playerStats.maxShieldLevel + "\n" +
                "Resistenza: " + playerStats.ShieldMax;
        }

        if(characterStatsText != null)
        {
            characterStatsText.text =
                "Personaggio\n" +
                "Velocita': " + playerStats.MoveSpeed;
        }

        SetButtonInteractable(upgradeSwordButton, playerStats.upgradePoints > 0 && playerStats.swordLevel < playerStats.maxSwordLevel);
        SetButtonInteractable(upgradeShieldButton, playerStats.upgradePoints > 0 && playerStats.shieldLevel < playerStats.maxShieldLevel);
        HideSpeedUpgradeButton();
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if(button != null)
        {
            button.interactable = interactable;
        }
    }

    private void CreateRuntimeUI()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("StatsUpgradeCanvas");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);

        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystemExists();

        menuPanel = CreateUIObject("StatsUpgradePanel", canvasObject.transform);
        RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-24f, 0f);
        panelRect.sizeDelta = PanelSize;

        Image panelImage = menuPanel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.09f, 0.11f, 0.92f);

        Text titleText = CreateText("Title", menuPanel.transform, defaultFont, "Statistiche", 22, TextAnchor.MiddleLeft);
        SetRect(titleText.rectTransform, new Vector2(18f, -20f), new Vector2(324f, 30f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        upgradePointsText = CreateText("UpgradePoints", menuPanel.transform, defaultFont, "", 16, TextAnchor.MiddleRight);
        upgradePointsText.fontSize = 14;
        SetRect(upgradePointsText.rectTransform, new Vector2(18f, -26f), new Vector2(324f, 44f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        swordStatsText = CreateText("SwordStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(swordStatsText.rectTransform, new Vector2(18f, -64f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        upgradeSwordButton = CreateButton("UpgradeSwordButton", menuPanel.transform, defaultFont, "Spada +", new Vector2(-18f, -74f));
        upgradeSwordButton.onClick.AddListener(UpgradeSword);

        shieldStatsText = CreateText("ShieldStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(shieldStatsText.rectTransform, new Vector2(18f, -134f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        upgradeShieldButton = CreateButton("UpgradeShieldButton", menuPanel.transform, defaultFont, "Scudo +", new Vector2(-18f, -144f));
        upgradeShieldButton.onClick.AddListener(UpgradeShield);

        characterStatsText = CreateText("CharacterStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(characterStatsText.rectTransform, new Vector2(18f, -204f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        Text hintText = CreateText("Hint", menuPanel.transform, defaultFont, "Tieni premuto G", 13, TextAnchor.MiddleCenter);
        hintText.color = new Color(0.8f, 0.84f, 0.9f, 1f);
        SetRect(hintText.rectTransform, new Vector2(0f, 18f), new Vector2(324f, 24f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName);
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    private Text CreateText(string objectName, Transform parent, Font font, string text, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        Text textComponent = textObject.AddComponent<Text>();
        textComponent.font = font;
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Truncate;
        return textComponent;
    }

    private Button CreateButton(string objectName, Transform parent, Font font, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetRect(buttonRect, anchoredPosition, new Vector2(104f, 34f), new Vector2(1f, 1f), new Vector2(1f, 1f));

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.28f, 0.34f, 0.62f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        Text buttonText = CreateText("Text", buttonObject.transform, font, label, 14, TextAnchor.MiddleCenter);
        SetRect(buttonText.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

        return button;
    }

    private void SetRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    private void HideSpeedUpgradeButton()
    {
        if(upgradeSpeedButton != null)
            upgradeSpeedButton.gameObject.SetActive(false);
    }

    private void EnsureEventSystemExists()
    {
        if(EventSystem.current != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void AssignFallbackFonts()
    {
        Font fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Text[] texts = GetComponentsInChildren<Text>(true);

        foreach(Text text in texts)
        {
            if(text != null && text.font == null)
                text.font = fallbackFont;
        }
    }
}
