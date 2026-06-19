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

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

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

            EndDialogue();
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
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
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
        if(dialogueData != null && dialogueData.npcName == "Riven" && King.HasBeenDefeated)
            return rivenDialogueAfterKingDeath;

        return dialogueData.dialogueLines;
    }
}
