using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NPC))]
public class RivenKeyReward : MonoBehaviour
{
    [Header("Key Reward")]
    [SerializeField] private string keyRewardId = "FinalRoomKey";
    [SerializeField] private Sprite keyRewardIcon;

    [Header("Reward Popup")]
    [SerializeField] private GameObject keyRewardPopup;
    [SerializeField] private Image keyRewardImage;
    [SerializeField] private TMP_Text keyRewardText;
    [SerializeField] private string keyRewardMessage = "Hai ottenuto la chiave";
    [SerializeField] private float keyRewardPopupDuration = 2f;

    private NPC npc;
    private Coroutine popupCoroutine;

    private void Awake()
    {
        npc = GetComponent<NPC>();
    }

    private void OnEnable()
    {
        if(npc == null)
            npc = GetComponent<NPC>();

        if(npc != null)
            npc.DialogueCompleted += OnDialogueCompleted;
    }

    private void OnDisable()
    {
        if(npc != null)
            npc.DialogueCompleted -= OnDialogueCompleted;
    }

    private void OnDialogueCompleted(NPC completedNpc)
    {
        if(completedNpc == null || !completedNpc.IsRivenPostKingDialogue())
            return;

        if(StoryKeyInventory.HasKey(keyRewardId))
            return;

        StoryKeyInventory.AddKey(keyRewardId, keyRewardIcon);
        ShowRewardPopup();
    }

    private void ShowRewardPopup()
    {
        if(keyRewardImage != null)
            keyRewardImage.sprite = keyRewardIcon;

        if(keyRewardText != null)
            keyRewardText.SetText(keyRewardMessage);

        if(keyRewardPopup == null)
            return;

        if(popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(RewardPopupCoroutine());
    }

    private IEnumerator RewardPopupCoroutine()
    {
        keyRewardPopup.SetActive(true);

        yield return new WaitForSecondsRealtime(keyRewardPopupDuration);

        keyRewardPopup.SetActive(false);
        popupCoroutine = null;
    }
}
