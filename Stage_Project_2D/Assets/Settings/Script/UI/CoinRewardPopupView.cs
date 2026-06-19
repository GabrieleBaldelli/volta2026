using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinRewardPopupView : MonoBehaviour
{
    public RectTransform popupRect;
    public CanvasGroup canvasGroup;
    public TMP_Text amountText;
    public Image coinIcon;

    private void Awake()
    {
        if(popupRect == null)
            popupRect = GetComponent<RectTransform>();

        if(canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }
}
