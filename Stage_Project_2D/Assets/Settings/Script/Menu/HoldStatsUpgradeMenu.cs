using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class HoldStatsUpgradeMenu : MonoBehaviour
{
    [Header("Input")]
    // Tasto da tenere premuto per aprire il menu upgrade durante il gioco.
    public KeyCode menuKey = KeyCode.G;

    [Header("References")]
    // Pannello che contiene tutta la UI visibile del menu.
    public GameObject menuPanel;

    // Script del player che contiene punti upgrade, livelli e statistiche.
    public PlayerUpgradeStats playerStats;

    [Header("Texts")]
    // Testo in alto: mostra livello, punti disponibili e XP.
    public Text upgradePointsText;

    // Testi delle singole statistiche mostrate nel menu.
    public Text swordStatsText;
    public Text shieldStatsText;
    public Text healthStatsText;
    public Text characterStatsText;

    [Header("Buttons")]
    // Bottoni collegati agli upgrade. Ogni bottone chiama una funzione pubblica qui sotto.
    public Button upgradeSwordButton;
    public Button upgradeShieldButton;
    public Button upgradeHealthButton;
    public Button upgradeSpeedButton;

    private bool isOpen;
    private int lastSwordUpgradeFrame = -1;
    private int lastShieldUpgradeFrame = -1;
    private int lastHealthUpgradeFrame = -1;

    // Tiene traccia del menu gia' presente, cosi' non ne vengono creati due nella stessa scena.
    private static HoldStatsUpgradeMenu instance;

    // Dimensione usata quando il menu viene creato automaticamente da codice.
    private static readonly Vector2 PanelSize = new Vector2(360f, 370f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeOnFirstScene()
    {
        // Ogni volta che cambia scena controlla se esiste un menu upgrade.
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureMenuExists();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Questo viene chiamato automaticamente dopo ogni cambio scena.
        // Serve per far funzionare il menu sia in Room 1 sia in Room 2.
        EnsureMenuExists();
    }

    private static void EnsureMenuExists()
    {
        // Se il menu e' gia' stato trovato o creato, non fa nulla.
        if(instance != null)
            return;

        // Prima cerca un HoldStatsUpgradeMenu gia' inserito nella scena dall'Inspector.
        instance = FindBestSceneMenu();

        if(instance != null)
            return;

        // Se la scena non ha il Canvas del menu, prova a crearlo usando il player trovato.
        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if(player == null)
            return;

        GameObject menuObject = new GameObject("Stats Upgrade Menu");
        instance = menuObject.AddComponent<HoldStatsUpgradeMenu>();
        instance.playerStats = player.GetComponent<PlayerUpgradeStats>();

        // Se al player manca PlayerUpgradeStats, lo aggiunge per evitare errori.
        if(instance.playerStats == null)
            instance.playerStats = player.gameObject.AddComponent<PlayerUpgradeStats>();

        instance.CreateRuntimeUI();
    }

    private static HoldStatsUpgradeMenu FindBestSceneMenu()
    {
        HoldStatsUpgradeMenu[] menus = FindObjectsOfType<HoldStatsUpgradeMenu>();
        HoldStatsUpgradeMenu bestMenu = null;
        int bestScore = -1;

        foreach(HoldStatsUpgradeMenu menu in menus)
        {
            int score = menu.GetConfigurationScore();

            if(score > bestScore)
            {
                bestMenu = menu;
                bestScore = score;
            }
        }

        return bestMenu;
    }

    private void Start()
    {
        if(!RegisterAsActiveMenu())
            return;

        AutoWireSceneReferences();
        EnsurePlayerStatsReference();

        if(menuPanel == null)
            CreateRuntimeUI();

        AutoWireSceneReferences();
        ConnectButtonCallbacks();

        // Il bottone velocita' non e' usato, quindi viene nascosto anche se esiste nel Canvas.
        HideSpeedUpgradeButton();
        AssignFallbackFonts();

        if(!Application.isPlaying)
            return;

        SetMenuOpen(false);
    }

    private void OnDestroy()
    {
        if(instance == this)
            instance = null;
    }

    private void OnValidate()
    {
        // OnValidate gira nell'Editor quando cambi valori nell'Inspector.
        // Aiuta a tenere nascosto il bottone velocita' anche senza avviare il gioco.
        HideSpeedUpgradeButton();
        AssignFallbackFonts();
    }

    private void Update()
    {
        if(!Application.isPlaying)
            return;

        // Se il gioco e' in pausa, chiude il menu upgrade e impedisce di aprirlo.
        if(Time.timeScale == 0f)
        {
            if(isOpen)
                SetMenuOpen(false);

            return;
        }

        // Il menu resta aperto solo mentre tieni premuto il tasto scelto.
        bool shouldBeOpen = Input.GetKey(menuKey);

        if(shouldBeOpen != isOpen)
        {
            SetMenuOpen(shouldBeOpen);
        }

        if(isOpen)
        {
            // Aggiorna i valori mentre il menu e' visibile, cosi' XP e punti restano corretti.
            Refresh();
        }
    }

    public void UpgradeSword()
    {
        if(lastSwordUpgradeFrame == Time.frameCount)
            return;

        lastSwordUpgradeFrame = Time.frameCount;

        // Chiamata dal bottone della spada.
        // Dopo l'upgrade aggiorna subito i testi del menu.
        if(playerStats != null)
        {
            playerStats.UpgradeSword();
            Refresh();
        }
    }

    public void UpgradeShield()
    {
        if(lastShieldUpgradeFrame == Time.frameCount)
            return;

        lastShieldUpgradeFrame = Time.frameCount;

        // Chiamata dal bottone dello scudo.
        // Se l'upgrade riesce, PlayerUpgradeStats aumenta lo scudo massimo.
        if(playerStats != null)
        {
            playerStats.UpgradeShield();
            Refresh();
        }
    }

    public void UpgradeHealth()
    {
        if(lastHealthUpgradeFrame == Time.frameCount)
            return;

        lastHealthUpgradeFrame = Time.frameCount;

        // Chiamata dal bottone della vita.
        // L'aumento vero della vita viene gestito dentro PlayerUpgradeStats.
        if(playerStats != null)
        {
            playerStats.UpgradeHealth();
            Refresh();
        }
    }

    public void UpgradeSpeed()
    {
        // La velocita' non viene piu' usata come upgrade, quindi il bottone viene solo nascosto.
        HideSpeedUpgradeButton();
    }

    private void SetMenuOpen(bool open)
    {
        isOpen = open;

        // Attiva o disattiva tutto il pannello visibile.
        if(menuPanel != null)
        {
            menuPanel.SetActive(open);
        }

        if(open)
        {
            // Quando si apre, aggiorna subito testi e bottoni.
            Refresh();
        }
    }

    private void Refresh()
    {
        AutoWireSceneReferences();
        ConnectButtonCallbacks();

        if(playerStats == null)
            EnsurePlayerStatsReference();

        if(playerStats == null)
            return;

        // Testo in alto: livello, punti upgrade disponibili e progressione XP.
        if(upgradePointsText != null)
        {
            upgradePointsText.text =
                "Livello: " + playerStats.PlayerLevel + "   Punti: " + playerStats.upgradePoints + "\n" +
                "XP: " + playerStats.CurrentXP + "/" + playerStats.NextLevelXP;
        }

        // Statistiche della spada: livello upgrade e danno attuale.
        if(swordStatsText != null)
        {
            swordStatsText.text =
                "Spada\n" +
                "Livello: " + playerStats.swordLevel + "/" + playerStats.maxSwordLevel + "\n" +
                "Danno: " + playerStats.SwordDamage;
        }

        // Statistiche dello scudo: livello upgrade e scudo massimo.
        if(shieldStatsText != null)
        {
            shieldStatsText.text =
                "Scudo\n" +
                "Livello: " + playerStats.shieldLevel + "/" + playerStats.maxShieldLevel + "\n" +
                "Resistenza: " + playerStats.ShieldMax;
        }

        // Statistiche della vita: livello upgrade e vita attuale.
        if(healthStatsText != null)
        {
            healthStatsText.text =
                "Vita\n" +
                "Livello: " + playerStats.healthLevel + "/" + playerStats.maxHealthLevel + "\n" +
                "PV: " + Mathf.CeilToInt(playerStats.HealthCurrent);
        }

        if(characterStatsText != null)
        {
            // Al momento resta qui solo come testo extra, nel caso serva mostrare altre statistiche.
            characterStatsText.text =
                "Personaggio\n" +
                "Velocita': " + playerStats.MoveSpeed;
        }

        // Un bottone e' cliccabile solo se hai punti e quell'upgrade non e' al massimo.
        SetButtonInteractable(upgradeSwordButton, playerStats.upgradePoints > 0 && playerStats.swordLevel < playerStats.maxSwordLevel);
        SetButtonInteractable(upgradeShieldButton, playerStats.upgradePoints > 0 && playerStats.shieldLevel < playerStats.maxShieldLevel);
        SetButtonInteractable(upgradeHealthButton, playerStats.upgradePoints > 0 && playerStats.healthLevel < playerStats.maxHealthLevel);
        HideSpeedUpgradeButton();
    }

    private bool RegisterAsActiveMenu()
    {
        if(instance == null)
        {
            instance = this;
            return true;
        }

        if(instance == this)
            return true;

        if(GetConfigurationScore() > instance.GetConfigurationScore())
        {
            instance.SetMenuOpen(false);
            instance.enabled = false;
            instance = this;
            return true;
        }

        SetMenuOpen(false);
        enabled = false;
        return false;
    }

    private int GetConfigurationScore()
    {
        int score = 0;

        if(menuPanel != null || HasNamedChild("StatsUpgradePanel"))
            score += 3;

        if(playerStats != null)
            score += 2;

        if(upgradePointsText != null || HasNamedChildComponent<Text>("UpgradePoints"))
            score++;

        if(swordStatsText != null || HasNamedChildComponent<Text>("SwordStats"))
            score++;

        if(shieldStatsText != null || HasNamedChildComponent<Text>("ShieldStats"))
            score++;

        if(healthStatsText != null || HasNamedChildComponent<Text>("HealthStats"))
            score++;

        if(characterStatsText != null || HasNamedChildComponent<Text>("CharacterStats"))
            score++;

        if(upgradeSwordButton != null || HasNamedChildComponent<Button>("UpgradeSwordButton"))
            score++;

        if(upgradeShieldButton != null || HasNamedChildComponent<Button>("UpgradeShieldButton"))
            score++;

        if(upgradeHealthButton != null || HasNamedChildComponent<Button>("UpgradeHealthButton"))
            score++;

        return score;
    }

    private void AutoWireSceneReferences()
    {
        if(menuPanel == null)
            menuPanel = FindChildGameObject("StatsUpgradePanel");

        if(upgradePointsText == null)
            upgradePointsText = FindChildComponent<Text>("UpgradePoints");

        if(swordStatsText == null)
            swordStatsText = FindChildComponent<Text>("SwordStats");

        if(shieldStatsText == null)
            shieldStatsText = FindChildComponent<Text>("ShieldStats");

        if(healthStatsText == null)
            healthStatsText = FindChildComponent<Text>("HealthStats");

        if(characterStatsText == null)
            characterStatsText = FindChildComponent<Text>("CharacterStats");

        if(upgradeSwordButton == null)
            upgradeSwordButton = FindChildComponent<Button>("UpgradeSwordButton");

        if(upgradeShieldButton == null)
            upgradeShieldButton = FindChildComponent<Button>("UpgradeShieldButton");

        if(upgradeHealthButton == null)
            upgradeHealthButton = FindChildComponent<Button>("UpgradeHealthButton");

        if(upgradeSpeedButton == null)
            upgradeSpeedButton = FindChildComponent<Button>("UpgradeSpeedButton");
    }

    private void EnsurePlayerStatsReference()
    {
        // Se il riferimento non e' stato collegato nella scena, cerca il player automaticamente.
        if(playerStats != null)
            return;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if(player == null)
            return;

        playerStats = player.GetComponent<PlayerUpgradeStats>();

        if(playerStats == null)
            playerStats = player.gameObject.AddComponent<PlayerUpgradeStats>();
    }

    private void ConnectButtonCallbacks()
    {
        ConnectButton(upgradeSwordButton, UpgradeSword);
        ConnectButton(upgradeShieldButton, UpgradeShield);
        ConnectButton(upgradeHealthButton, UpgradeHealth);
        ConnectButton(upgradeSpeedButton, UpgradeSpeed);
    }

    private void ConnectButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if(button == null)
            return;

        // Se il bottone e' gia' collegato dall'Inspector, non aggiungere un secondo listener.
        // Altrimenti un solo click spenderebbe piu' punti upgrade.
        if(HasPersistentCallback(button, action.Method.Name))
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private bool HasPersistentCallback(Button button, string methodName)
    {
        int persistentEventCount = button.onClick.GetPersistentEventCount();

        for(int i = 0; i < persistentEventCount; i++)
        {
            if(button.onClick.GetPersistentTarget(i) != null &&
                button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasNamedChild(string objectName)
    {
        return FindChildGameObject(objectName) != null;
    }

    private bool HasNamedChildComponent<T>(string objectName) where T : Component
    {
        return FindChildComponent<T>(objectName) != null;
    }

    private GameObject FindChildGameObject(string objectName)
    {
        if(gameObject.name == objectName)
            return gameObject;

        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach(Transform child in children)
        {
            if(child.name == objectName)
                return child.gameObject;
        }

        return null;
    }

    private T FindChildComponent<T>(string objectName) where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true);

        foreach(T component in components)
        {
            if(component.name == objectName)
                return component;
        }

        return null;
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        // Evita errori se in una scena un bottone non e' stato assegnato.
        if(button != null)
        {
            button.interactable = interactable;
        }
    }

    private void CreateRuntimeUI()
    {
        // Crea una versione semplice del menu se nella scena non e' stato messo a mano il Canvas.
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("StatsUpgradeCanvas");
        canvasObject.transform.SetParent(transform);

        // Canvas in ScreenSpaceOverlay: viene disegnato sopra la scena, senza bisogno di camera.
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        // CanvasScaler: mantiene dimensioni simili anche con risoluzioni diverse.
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);

        // GraphicRaycaster permette ai bottoni UI di ricevere click.
        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystemExists();

        // Pannello principale, agganciato al lato destro dello schermo.
        menuPanel = CreateUIObject("StatsUpgradePanel", canvasObject.transform);
        RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-24f, 0f);
        panelRect.sizeDelta = PanelSize;

        Image panelImage = menuPanel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.09f, 0.11f, 0.92f);

        // Titolo del pannello.
        Text titleText = CreateText("Title", menuPanel.transform, defaultFont, "Statistiche", 22, TextAnchor.MiddleLeft);
        SetRect(titleText.rectTransform, new Vector2(18f, -20f), new Vector2(324f, 30f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        // Riga superiore con livello, punti e XP.
        upgradePointsText = CreateText("UpgradePoints", menuPanel.transform, defaultFont, "", 16, TextAnchor.MiddleRight);
        upgradePointsText.fontSize = 14;
        SetRect(upgradePointsText.rectTransform, new Vector2(18f, -26f), new Vector2(324f, 44f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        // Sezione spada: testo a sinistra e bottone a destra.
        swordStatsText = CreateText("SwordStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(swordStatsText.rectTransform, new Vector2(18f, -64f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        upgradeSwordButton = CreateButton("UpgradeSwordButton", menuPanel.transform, defaultFont, "Spada +", new Vector2(-18f, -74f));
        upgradeSwordButton.onClick.AddListener(UpgradeSword);

        // Sezione scudo.
        shieldStatsText = CreateText("ShieldStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(shieldStatsText.rectTransform, new Vector2(18f, -134f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        upgradeShieldButton = CreateButton("UpgradeShieldButton", menuPanel.transform, defaultFont, "Scudo +", new Vector2(-18f, -144f));
        upgradeShieldButton.onClick.AddListener(UpgradeShield);

        // Sezione vita.
        healthStatsText = CreateText("HealthStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(healthStatsText.rectTransform, new Vector2(18f, -204f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        upgradeHealthButton = CreateButton("UpgradeHealthButton", menuPanel.transform, defaultFont, "Vita +", new Vector2(-18f, -214f));
        upgradeHealthButton.onClick.AddListener(UpgradeHealth);

        // Spazio lasciato per eventuali statistiche future del personaggio.
        characterStatsText = CreateText("CharacterStats", menuPanel.transform, defaultFont, "", 15, TextAnchor.UpperLeft);
        SetRect(characterStatsText.rectTransform, new Vector2(18f, -274f), new Vector2(210f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        // Piccolo suggerimento in basso.
        Text hintText = CreateText("Hint", menuPanel.transform, defaultFont, "Tieni premuto G", 13, TextAnchor.MiddleCenter);
        hintText.color = new Color(0.8f, 0.84f, 0.9f, 1f);
        SetRect(hintText.rectTransform, new Vector2(0f, 18f), new Vector2(324f, 24f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        // Crea un GameObject UI base e lo mette nel layer UI.
        GameObject uiObject = new GameObject(objectName);
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    private Text CreateText(string objectName, Transform parent, Font font, string text, int fontSize, TextAnchor alignment)
    {
        // Helper per creare testi UI senza ripetere sempre le stesse impostazioni.
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
        // Helper per creare un bottone con immagine, componente Button e testo figlio.
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
        // Centralizza le impostazioni del RectTransform usate dagli elementi creati da codice.
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    private void HideSpeedUpgradeButton()
    {
        // Il bottone velocita' puo' essere ancora presente in vecchie scene,
        // quindi lo nascondiamo da codice senza doverlo cancellare a mano ovunque.
        if(upgradeSpeedButton != null)
            upgradeSpeedButton.gameObject.SetActive(false);
    }

    private void EnsureEventSystemExists()
    {
        // I bottoni UI funzionano solo se nella scena esiste un EventSystem.
        if(EventSystem.current != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void AssignFallbackFonts()
    {
        // In alcune versioni di Unity i Text copiati possono perdere il font.
        // Questo evita testi invisibili assegnando un font di fallback.
        Font fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Text[] texts = GetComponentsInChildren<Text>(true);

        foreach(Text text in texts)
        {
            if(text != null && text.font == null)
                text.font = fallbackFont;
        }
    }
}
