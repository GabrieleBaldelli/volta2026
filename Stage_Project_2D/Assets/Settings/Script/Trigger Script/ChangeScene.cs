using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public string Scene;
    public AudioClip musicToPlay;

    private static GameObject passageMessageCanvas;
    private static TMP_Text passageMessageText;

    public void OnTriggerEnter2D(Collider2D other)
   {
        if (other.CompareTag("Player"))
        {
            if(Scene == "Room 2" && (!NPC.HasTalkedTo("Anne") || !NPC.HasTalkedTo("Eleonore")))
            {
                ShowPassageMessage("Prima di passare devi parlare con Anne ed Eleonore.");
                return;
            }

            if (musicToPlay != null)
                BackgroundMusicManager.Instance.PlayMusic(musicToPlay);

            SceneManager.LoadScene(Scene);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
   }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
            HidePassageMessage();
    }

    private static void ShowPassageMessage(string message)
    {
        EnsurePassageMessageCanvas();

        if(passageMessageText != null)
            passageMessageText.text = message;

        if(passageMessageCanvas != null)
            passageMessageCanvas.SetActive(true);
    }

    private static void HidePassageMessage()
    {
        if(passageMessageCanvas != null)
            passageMessageCanvas.SetActive(false);
    }

    private static void EnsurePassageMessageCanvas()
    {
        if(passageMessageCanvas != null)
            return;

        passageMessageCanvas = new GameObject("Passage Message Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Object.DontDestroyOnLoad(passageMessageCanvas);

        Canvas canvas = passageMessageCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = passageMessageCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("Message Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(passageMessageCanvas.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.12f);
        panelRect.anchorMax = new Vector2(0.5f, 0.12f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(760f, 88f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject textObject = new GameObject("Message Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 10f);
        textRect.offsetMax = new Vector2(-24f, -10f);

        passageMessageText = textObject.GetComponent<TMP_Text>();
        passageMessageText.alignment = TextAlignmentOptions.Center;
        passageMessageText.color = Color.white;
        passageMessageText.fontSize = 28f;
        passageMessageText.enableWordWrapping = true;

        passageMessageCanvas.SetActive(false);
    }
}
