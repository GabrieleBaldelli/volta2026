using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CoinHud : MonoBehaviour
{
    private const string GoldenCoinAssetPath = "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Misc/Golden Coin.png";
    private const string CoinHudResourcePath = "UI/CoinRewardPopupCanvas";
    private const string RoomWithCoinPopups = "Room 1";

    private static CoinHud instance;
    private static Sprite goldenCoinSprite;

    [SerializeField] private CoinRewardPopupView popupTemplate;

    private Canvas canvas;
    private RectTransform canvasRect;

    public static void ShowCoinReward(int coinAmount, Vector3 worldPosition)
    {
        if(coinAmount <= 0 || !IsRoomWithCoinPopups())
            return;

        EnsureExists();

        if(instance != null)
            instance.CreatePopup(coinAmount, worldPosition);
    }

    private static void EnsureExists()
    {
        if(instance != null)
            return;

        CoinHud sceneHud = FindSceneHud();
        if(sceneHud != null)
        {
            instance = sceneHud;
            instance.InitializeHud();
            return;
        }

        CoinHud prefab = Resources.Load<CoinHud>(CoinHudResourcePath);
        if(prefab != null)
        {
            Instantiate(prefab);
            return;
        }

        GameObject hudObject = new GameObject("Coin Reward Popup Canvas");
        instance = hudObject.AddComponent<CoinHud>();
    }

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeHud();
    }

    private void InitializeHud()
    {
        if(canvas == null)
            canvas = GetComponent<Canvas>();

        if(canvas == null)
            BuildCanvas();
        else
            canvasRect = canvas.GetComponent<RectTransform>();

        if(popupTemplate != null)
            popupTemplate.gameObject.SetActive(false);
    }

    private void BuildCanvas()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    private void CreatePopup(int coinAmount, Vector3 worldPosition)
    {
        if(canvasRect == null)
            InitializeHud();

        if(canvasRect == null)
            return;

        Camera popupCamera = FindCameraForWorldPosition(worldPosition);
        if(popupCamera == null)
            return;

        CoinRewardPopupView popup = CreatePopupView();
        if(popup == null || popup.popupRect == null || popup.canvasGroup == null)
            return;

        popup.gameObject.SetActive(true);
        popup.popupRect.anchoredPosition = WorldToCanvasPosition(worldPosition + Vector3.up * 1.15f, popupCamera);
        popup.canvasGroup.alpha = 1f;

        if(popup.amountText != null)
            popup.amountText.text = "+" + coinAmount;

        if(popup.coinIcon != null && popup.coinIcon.sprite == null)
            popup.coinIcon.sprite = GetGoldenCoinSprite();

        StartCoroutine(FadePopup(popup.canvasGroup, popup.popupRect));
    }

    private CoinRewardPopupView CreatePopupView()
    {
        if(popupTemplate != null)
            return Instantiate(popupTemplate, transform, false);

        return BuildPopupView();
    }

    private CoinRewardPopupView BuildPopupView()
    {
        GameObject popupObject = new GameObject("Coin Reward Popup", typeof(RectTransform), typeof(CanvasGroup), typeof(HorizontalLayoutGroup), typeof(CoinRewardPopupView));
        popupObject.transform.SetParent(transform, false);

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

        GameObject textObject = new GameObject("Coin Amount", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(popupObject.transform, false);

        LayoutElement textLayout = textObject.GetComponent<LayoutElement>();
        textLayout.preferredWidth = 80f;
        textLayout.preferredHeight = 42f;

        TMP_Text amountText = textObject.GetComponent<TMP_Text>();
        amountText.alignment = TextAlignmentOptions.MidlineRight;
        amountText.color = new Color(1f, 0.86f, 0.24f, 1f);
        amountText.fontSize = 32f;
        amountText.fontStyle = FontStyles.Bold;
        amountText.raycastTarget = false;

        GameObject iconObject = new GameObject("Golden Coin", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(popupObject.transform, false);

        LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
        iconLayout.preferredWidth = 30f;
        iconLayout.preferredHeight = 30f;

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = GetGoldenCoinSprite();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        CoinRewardPopupView popup = popupObject.GetComponent<CoinRewardPopupView>();
        popup.popupRect = popupRect;
        popup.canvasGroup = popupObject.GetComponent<CanvasGroup>();
        popup.amountText = amountText;
        popup.coinIcon = icon;
        return popup;
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition, Camera popupCamera)
    {
        Vector2 screenPosition = popupCamera.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 canvasPosition);
        return canvasPosition;
    }

    private static bool IsRoomWithCoinPopups()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == RoomWithCoinPopups || sceneName.StartsWith(RoomWithCoinPopups + " ");
    }

    private static Camera FindCameraForWorldPosition(Vector3 worldPosition)
    {
        Camera bestCamera = null;
        float bestDistance = float.MaxValue;

        foreach(Camera camera in Camera.allCameras)
        {
            if(camera == null || !camera.isActiveAndEnabled)
                continue;

            Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);
            bool pointIsVisible = viewportPosition.z > 0f
                && viewportPosition.x >= 0f && viewportPosition.x <= 1f
                && viewportPosition.y >= 0f && viewportPosition.y <= 1f;

            if(pointIsVisible)
                return camera;

            float distance = Vector2.SqrMagnitude((Vector2)camera.transform.position - (Vector2)worldPosition);
            if(distance < bestDistance)
            {
                bestDistance = distance;
                bestCamera = camera;
            }
        }

        return bestCamera != null ? bestCamera : Camera.main;
    }

    private static CoinHud FindSceneHud()
    {
        CoinHud[] huds = Resources.FindObjectsOfTypeAll<CoinHud>();
        foreach(CoinHud hud in huds)
        {
            if(hud == null || !hud.gameObject.scene.IsValid())
                continue;

            return hud;
        }

        return null;
    }

    private IEnumerator FadePopup(CanvasGroup group, RectTransform popupRect)
    {
        const float holdTime = 0.35f;
        const float fadeTime = 0.65f;
        const float moveUpDistance = 44f;

        Vector2 startPosition = popupRect.anchoredPosition;
        yield return new WaitForSeconds(holdTime);

        float elapsed = 0f;
        while(elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);

            group.alpha = 1f - t;
            popupRect.anchoredPosition = startPosition + Vector2.up * (moveUpDistance * t);
            yield return null;
        }

        Destroy(popupRect.gameObject);
    }

    private static Sprite GetGoldenCoinSprite()
    {
        if(goldenCoinSprite != null)
            return goldenCoinSprite;

#if UNITY_EDITOR
        goldenCoinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GoldenCoinAssetPath);
        if(goldenCoinSprite != null)
            return goldenCoinSprite;
#endif

        string absolutePath = Path.Combine(Application.dataPath, GoldenCoinAssetPath.Replace("Assets/", ""));
        if(!File.Exists(absolutePath))
            return null;

        byte[] imageBytes = File.ReadAllBytes(absolutePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if(!texture.LoadImage(imageBytes))
            return null;

        texture.filterMode = FilterMode.Point;
        goldenCoinSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        return goldenCoinSprite;
    }
}
