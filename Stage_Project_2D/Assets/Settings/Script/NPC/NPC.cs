using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, Interactable
{
    [Header("Dialogue")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Canvas Shop;

    private bool talked = false;

    [Header("Unread Dialogue Indicator")]
    [SerializeField] private GameObject unreadDialogueIndicator;
    [SerializeField] private bool autoCreateUnreadDialogueIndicator = true;
    [SerializeField] private Vector3 unreadIndicatorOffset = new Vector3(0f, 1.35f, 0f);
    [SerializeField] private Color unreadIndicatorColor = new Color(1f, 0.85f, 0.15f, 1f);
    [SerializeField] private float unreadIndicatorFontSize = 5f;
    [SerializeField] private string dialogueId;
    [SerializeField] private bool hideIndicatorDuringDialogue = true;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public event Action<NPC> DialogueCompleted;

    private static readonly HashSet<string> readDialogueIds = new HashSet<string>();

    private static readonly HashSet<string> talkedNPCs = new HashSet<string>();

    private static readonly string[] rivenDialogueAfterKingDeath =
    {
        "Hai sconfitto il King. Non male, cavaliere.",
        "Come ricompensa ti do una chiave che ho trovato nel bagno del castello."
    };

    //Puoi interagirci solo se non è già attivo un dialogo con questo NPC
    public bool CanInteract()
    {
        return enabled && !isDialogueActive;
    }

    private void OnEnable()
    {
        EnsureUnreadDialogueIndicator();
        RefreshUnreadDialogueIndicator();
    }

    private void Update()
    {
        RefreshUnreadDialogueIndicator();
    }

    [Header("Player Interaction")]
    [SerializeField] private GameObject player;

    public void Interact()
    {
        //se manca il dialogueData oppure il gioco è in pausa
        if(dialogueData == null || (Time.timeScale == 0 && !isDialogueActive))
            return;

        if(isDialogueActive)
        {
            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKey(KeyCode.G))
            {
                StopDialogue(true);
                do
                {
                    if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.G))
                    {
                        StopDialogue(false);
                    }
                } 
                while (dialoguePanel.activeSelf);
            }
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        RefreshUnreadDialogueIndicator();

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if(isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(GetDialogueLines()[dialogueIndex]);
            isTyping = false;
        }
        else if( ++dialogueIndex < GetDialogueLines().Length)
        {
            //If another line, type next line
            StartCoroutine(TypeLine());
        }
        else
        {
            if(Shop != null)
                Shop.gameObject.SetActive(true);

            EndDialogue(true);
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");
        string[] dialogueLines = GetDialogueLines();

        foreach (char letter in dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        EndDialogue(false);
        talked = true;
    }

    private void EndDialogue(bool completedDialogue)
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);

       if(completedDialogue)
    {
        DialogueCompleted?.Invoke(this);

        if(dialogueData != null)
            talkedNPCs.Add(dialogueData.npcName);

        MarkCurrentDialogueAsRead();
        RefreshUnreadDialogueIndicator();
    }
    }

    public void StopDialogue(bool stop)
    {
        if(stop)
            StopAllCoroutines();
        else
            StartCoroutine(TypeLine());

        dialoguePanel.SetActive(!stop);
        isTyping = !stop;
        isDialogueActive = !stop;
    }

    private string[] GetDialogueLines()
    {
        if(IsRivenPostKingDialogue())
            return rivenDialogueAfterKingDeath;

        return dialogueData.dialogueLines;
    }

    public bool IsRivenPostKingDialogue()
    {
        return dialogueData != null && dialogueData.npcName == "Riven" && King.HasBeenDefeated;
    }

    private void MarkCurrentDialogueAsRead()
    {
        string key = GetCurrentDialogueId();

        if(!string.IsNullOrWhiteSpace(key))
            readDialogueIds.Add(key);
    }

    private void RefreshUnreadDialogueIndicator()
    {
        EnsureUnreadDialogueIndicator();

        if(unreadDialogueIndicator == null)
            return;

        bool shouldShow = enabled && dialogueData != null && !IsCurrentDialogueRead();

        if(hideIndicatorDuringDialogue && isDialogueActive)
            shouldShow = false;

        unreadDialogueIndicator.SetActive(shouldShow);
    }

    private void EnsureUnreadDialogueIndicator()
    {
        if(unreadDialogueIndicator != null || !autoCreateUnreadDialogueIndicator)
            return;

        GameObject indicator = new GameObject("Unread Dialogue Indicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = unreadIndicatorOffset;
        indicator.transform.localRotation = Quaternion.identity;
        indicator.transform.localScale = Vector3.one;

        TextMeshPro indicatorText = indicator.AddComponent<TextMeshPro>();
        indicatorText.text = "!";
        indicatorText.alignment = TextAlignmentOptions.Center;
        indicatorText.fontSize = unreadIndicatorFontSize;
        indicatorText.color = unreadIndicatorColor;
        indicatorText.fontStyle = FontStyles.Bold;
        indicatorText.enableWordWrapping = false;
        indicatorText.sortingOrder = 100;

        unreadDialogueIndicator = indicator;
    }

    private bool IsCurrentDialogueRead()
    {
        string key = GetCurrentDialogueId();
        return string.IsNullOrWhiteSpace(key) || readDialogueIds.Contains(key);
    }

    private string GetCurrentDialogueId()
    {
        if(dialogueData == null)
            return string.Empty;

        string baseId = !string.IsNullOrWhiteSpace(dialogueId) ? dialogueId : dialogueData.name;

        if(IsRivenPostKingDialogue())
            return baseId + ":after-king";

        return baseId;
    }

    public static bool HasTalkedTo(string npcName)
    {
        return talkedNPCs.Contains(npcName);
    }

    
}
