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

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    //Puoi interagirci solo se non è già attivo un dialogo con questo NPC
    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    [Header("Player Interaction")]
    [SerializeField] private GameObject player;

    public void Interact()
    {
        //se manca il dialogueData oppure il gioco è in pausa perchè un dialogo è già attivo, non fare nulla
        if(dialogueData == null || (Time.timeScale == 0 && !isDialogueActive))
            return;

        if(isDialogueActive)
        {
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
        Time.timeScale = 0;

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if(isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if( ++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            //If another line, type next line
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
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
        Time.timeScale = 1;

        // rende nullo activeInteractable del player, così che, una volta terminato il dialogo
        // con il bottone E, se l'npc non è più vicino il dialogo non può attivarsi
        PlayerInteraction InteractionScript = player.GetComponent<PlayerInteraction>();
        InteractionScript.activeInteractableSetGet = null;
    }
}
