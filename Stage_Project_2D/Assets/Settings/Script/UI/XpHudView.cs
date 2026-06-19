using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpHudView : MonoBehaviour
{
    public RectTransform rootRect;
    public Slider xpSlider;
    public TMP_Text levelText;
    public Text legacyLevelText;
    public RectTransform popupParent;
    public GameObject levelUpPopupTemplate;

    private PlayerMovement player;
    private int lastLevel;

    protected virtual void Awake()
    {
        FindReferences();
        PrepareSlider();

        if(levelUpPopupTemplate != null)
            levelUpPopupTemplate.SetActive(false);
    }

    protected virtual void OnValidate()
    {
        FindReferences();
        PrepareSlider();
    }

    protected virtual void Start()
    {
        FindPlayer();
        UpdateXpBar();
    }

    protected virtual void Update()
    {
        if(player == null || !player.gameObject.activeInHierarchy)
            FindPlayer();

        UpdateXpBar();
    }

    private void FindReferences()
    {
        if(rootRect == null)
            rootRect = GetComponent<RectTransform>();

        if(xpSlider == null)
            xpSlider = GetComponent<Slider>();

        if(xpSlider == null)
            xpSlider = GetComponentInChildren<Slider>(true);

        if(levelText == null)
            levelText = GetComponentInChildren<TMP_Text>(true);

        if(levelText == null)
            levelText = FindLevelTextInCanvas();

        if(legacyLevelText == null)
            legacyLevelText = GetComponentInChildren<Text>(true);

        if(legacyLevelText == null)
            legacyLevelText = FindLegacyLevelTextInCanvas();

        if(popupParent == null)
            popupParent = rootRect != null ? rootRect : GetComponent<RectTransform>();
    }

    private void FindPlayer()
    {
        player = FindObjectOfType<PlayerMovement>();
        lastLevel = player != null ? Mathf.FloorToInt(player.livello) : 0;
    }

    private void PrepareSlider()
    {
        if(xpSlider == null)
            return;

        xpSlider.minValue = 0f;
        xpSlider.maxValue = 1f;
        xpSlider.interactable = false;
        xpSlider.transition = Selectable.Transition.None;

        Image fillImage = xpSlider.fillRect != null ? xpSlider.fillRect.GetComponent<Image>() : null;
        if(fillImage != null)
        {
            fillImage.color = new Color(0.13f, 0.92f, 0.25f, 1f);
            fillImage.raycastTarget = false;
        }

        Transform background = xpSlider.transform.Find("Background");
        if(background != null && background.TryGetComponent(out Image backgroundImage))
        {
            backgroundImage.color = new Color(0f, 0f, 0f, 0.58f);
            backgroundImage.raycastTarget = false;
        }
    }

    private void UpdateXpBar()
    {
        if(player == null || xpSlider == null || (levelText == null && legacyLevelText == null))
            return;

        float nextLevelXp = Mathf.Max(1f, player.XpProssimoLivello);
        xpSlider.value = Mathf.Clamp01(player.XpAttuale / nextLevelXp);

        int currentLevel = Mathf.FloorToInt(player.livello);
        string levelLabel = "Livello " + currentLevel;
        if(levelText != null)
            levelText.text = levelLabel;

        if(legacyLevelText != null)
            legacyLevelText.text = levelLabel;

        if(lastLevel > 0 && currentLevel > lastLevel)
            StartCoroutine(ShowLevelUpPopup());

        lastLevel = currentLevel;
    }

    private IEnumerator ShowLevelUpPopup()
    {
        if(levelUpPopupTemplate == null)
            yield break;

        Transform parent = popupParent != null ? popupParent : transform;
        GameObject popupObject = Instantiate(levelUpPopupTemplate, parent, false);
        popupObject.SetActive(true);

        CanvasGroup group = popupObject.GetComponent<CanvasGroup>();
        if(group == null)
            group = popupObject.AddComponent<CanvasGroup>();

        RectTransform popupRect = popupObject.GetComponent<RectTransform>();
        Vector2 startPosition = popupRect != null ? popupRect.anchoredPosition : Vector2.zero;

        yield return new WaitForSeconds(0.35f);

        float elapsed = 0f;
        const float fadeTime = 0.75f;
        while(elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);

            group.alpha = 1f - t;
            if(popupRect != null)
                popupRect.anchoredPosition = startPosition + Vector2.up * (34f * t);

            yield return null;
        }

        Destroy(popupObject);
    }

    private TMP_Text FindLevelTextInCanvas()
    {
        Transform searchRoot = transform.root;
        TMP_Text[] texts = searchRoot.GetComponentsInChildren<TMP_Text>(true);

        foreach(TMP_Text text in texts)
        {
            if(text.name == "Livello testo" || text.name == "Livello Testo" || text.name == "Livello")
                return text;
        }

        foreach(TMP_Text text in texts)
        {
            if(text.name.ToLowerInvariant().Contains("livello"))
                return text;
        }

        return null;
    }

    private Text FindLegacyLevelTextInCanvas()
    {
        Transform searchRoot = transform.root;
        Text[] texts = searchRoot.GetComponentsInChildren<Text>(true);

        foreach(Text text in texts)
        {
            if(text.name == "Livello testo" || text.name == "Livello Testo" || text.name == "Livello")
                return text;
        }

        foreach(Text text in texts)
        {
            if(text.name.ToLowerInvariant().Contains("livello"))
                return text;
        }

        return null;
    }
}
