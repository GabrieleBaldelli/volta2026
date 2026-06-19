using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class HudCanvasPrefabBuilder
{
    private const string PrefabFolder = "Assets/Resources/UI";
    private const string CoinHudPrefabPath = PrefabFolder + "/CoinRewardPopupCanvas.prefab";
    private const string XpHudPrefabPath = PrefabFolder + "/XpHudCanvas.prefab";
    private const string GoldenCoinAssetPath = "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Misc/Golden Coin.png";

    static HudCanvasPrefabBuilder()
    {
        EditorApplication.delayCall += BuildMissingPrefabs;
    }

    [MenuItem("Tools/UI/Rebuild HUD Canvas Prefabs")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(PrefabFolder);
        BuildCoinRewardPrefab();
        BuildXpHudPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BuildMissingPrefabs()
    {
        Directory.CreateDirectory(PrefabFolder);

        if(!File.Exists(CoinHudPrefabPath))
            BuildCoinRewardPrefab();

        if(!File.Exists(XpHudPrefabPath))
            BuildXpHudPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BuildCoinRewardPrefab()
    {
        GameObject canvasObject = new GameObject("CoinRewardPopupCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CoinHud));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CoinRewardPopupView popup = CreateCoinPopupTemplate(canvasObject.transform);

        SerializedObject coinHud = new SerializedObject(canvasObject.GetComponent<CoinHud>());
        coinHud.FindProperty("popupTemplate").objectReferenceValue = popup;
        coinHud.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(canvasObject, CoinHudPrefabPath);
        UnityEngine.Object.DestroyImmediate(canvasObject);
    }

    private static CoinRewardPopupView CreateCoinPopupTemplate(Transform parent)
    {
        GameObject popupObject = new GameObject("Coin Reward Popup Template", typeof(RectTransform), typeof(CanvasGroup), typeof(HorizontalLayoutGroup), typeof(CoinRewardPopupView));
        popupObject.transform.SetParent(parent, false);
        popupObject.SetActive(false);

        RectTransform popupRect = popupObject.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.sizeDelta = new Vector2(140f, 42f);

        HorizontalLayoutGroup layout = popupObject.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 6f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TMP_Text amountText = CreateTmpText("Coin Amount", popupObject.transform, "+10", new Color(1f, 0.86f, 0.24f, 1f), 32f, TextAlignmentOptions.MidlineRight);
        LayoutElement amountLayout = amountText.gameObject.AddComponent<LayoutElement>();
        amountLayout.preferredWidth = 80f;
        amountLayout.preferredHeight = 42f;

        GameObject iconObject = new GameObject("Golden Coin", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(popupObject.transform, false);

        LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
        iconLayout.preferredWidth = 30f;
        iconLayout.preferredHeight = 30f;

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GoldenCoinAssetPath);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        CoinRewardPopupView popup = popupObject.GetComponent<CoinRewardPopupView>();
        popup.popupRect = popupRect;
        popup.canvasGroup = popupObject.GetComponent<CanvasGroup>();
        popup.amountText = amountText;
        popup.coinIcon = icon;
        return popup;
    }

    private static void BuildXpHudPrefab()
    {
        GameObject rootObject = new GameObject("XP_Bar", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(XpHudView));

        Canvas canvas = rootObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = rootObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(-538f, 416f);
        rootRect.sizeDelta = new Vector2(728f, 34f);

        TMP_Text levelText = CreateTmpText("XP Level Text", rootObject.transform, "Livello 1", Color.white, 18f, TextAlignmentOptions.Center);
        RectTransform labelRect = levelText.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, 11f);
        labelRect.sizeDelta = new Vector2(0f, 24f);

        Slider xpSlider = CreateXpSlider(rootObject.transform);
        GameObject popupTemplate = CreateLevelPopupTemplate(rootObject.transform);

        XpHudView view = rootObject.GetComponent<XpHudView>();
        view.rootRect = rootRect;
        view.xpSlider = xpSlider;
        view.levelText = levelText;
        view.popupParent = rootRect;
        view.levelUpPopupTemplate = popupTemplate;

        PrefabUtility.SaveAsPrefabAsset(rootObject, XpHudPrefabPath);
        UnityEngine.Object.DestroyImmediate(rootObject);
    }

    private static Slider CreateXpSlider(Transform parent)
    {
        GameObject sliderObject = new GameObject("XP Slider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0f);
        sliderRect.anchorMax = new Vector2(1f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = new Vector2(0f, 16f);

        Image background = CreateImage("Background", sliderObject.transform, new Color(0f, 0f, 0f, 0.55f));
        ApplyRoundedImage(background);
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        Image fill = CreateImage("Fill", fillAreaObject.transform, new Color(0.13f, 0.92f, 0.25f, 1f));
        ApplyRoundedImage(fill);
        RectTransform fillRect = fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.fillRect = fillRect;
        slider.targetGraphic = fill;
        return slider;
    }

    private static GameObject CreateLevelPopupTemplate(Transform parent)
    {
        TMP_Text popupText = CreateTmpText("Level Up Popup Template", parent, "+1 Livello", new Color(0.35f, 1f, 0.42f, 1f), 30f, TextAlignmentOptions.Center);
        popupText.gameObject.AddComponent<CanvasGroup>();
        popupText.gameObject.SetActive(false);

        RectTransform popupRect = popupText.rectTransform;
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = new Vector2(0f, 42f);
        popupRect.sizeDelta = new Vector2(220f, 42f);

        return popupText.gameObject;
    }

    private static TMP_Text CreateTmpText(string objectName, Transform parent, string text, Color color, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.alignment = alignment;
        tmpText.color = color;
        tmpText.fontSize = fontSize;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;
        return tmpText;
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void ApplyRoundedImage(Image image)
    {
        Sprite roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if(image == null || roundedSprite == null)
            return;

        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
    }
}
