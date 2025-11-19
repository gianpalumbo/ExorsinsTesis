using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[System.Serializable]
public struct DialogueLine
{
    [TextArea(2, 0)] public string speakerName;
    [TextArea(2, 5)] public string text;
}

public class DialogueSystem : MonoBehaviour
{
    [Header("Component")]
    public GameObject CanvasDialogue;
    public GameObject panelDialogue;
    //public TextMeshProUGUI xToSpeakText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    
    [Header("Dialogue")]
    public DialogueLine[] lines;

    private int currentIndex = 0;
    private string lastSpeaker = "";
    //private bool playerInRange = false;
    [SerializeField] private bool inDialogue = false;
    public bool IsDialoguing => inDialogue;

    /// <summary>Se dispara cuando el diálogo termina.</summary>
    public event Action OnDialogueEnd;

    void Start()
    {
        //xToSpeakText.gameObject.SetActive(false);
        panelDialogue.SetActive(false);
    }

    void Update()
    {
        //if (!playerInRange) return;

        //if (!inDialogue && Input.GetKeyDown(KeyCode.X))
        //{
        //    StartDialogue();
        //    return;
        //}
        //Debug.Log($"{gameObject.name}[Update] inDialogue = {inDialogue}");
        if (PauseManager.instance.isPaused) return;

        if (inDialogue && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("DIALOGUESYSTEM: siguiente dialogo");
            NextLine();
        }        
    }

    /// <summary>
    /// Comienza un diálogo siempre que 'lines' tenga al menos una línea.
    /// </summary>
    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;

        Debug.Log("DIALOGUESYSTEM: los dialogos no son nulos");
        inDialogue = true;
        Debug.Log($"[StartDialogue] inDialogue = {inDialogue}");
        currentIndex = 0;
        lastSpeaker = "";
        //xToSpeakText.gameObject.SetActive(false);
        panelDialogue.SetActive(true);
        ShowLine(currentIndex);
    }

    public void EndDialogue()
    {
        inDialogue = false;
        panelDialogue.SetActive(false);
        //if (playerInRange) xToSpeakText.gameObject.SetActive(true);
        //ACA TAMBIEN ESTARIA EL TEMA PODER MOVERME DE NUEVO E IRME CREO
        OnDialogueEnd?.Invoke();
    }

    public void NextLine()
    {
        //if (!inDialogue) return;
        if (currentIndex + 1 < lines.Length)
        {
            currentIndex++;
            ShowLine(currentIndex);
        }
        else { EndDialogue(); }  
    }

    private void ShowLine(int idx)
    {
        var line = lines[idx];
        if (line.speakerName != lastSpeaker)
        {
            nameText.text = line.speakerName;
            lastSpeaker = line.speakerName;
        }
        dialogueText.text = line.text;
    }
    
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!other.CompareTag("Player")) return;
    //    playerInRange = true;
    //    if (!inDialogue) xToSpeakText.gameObject.SetActive(true);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("Player")) return;
    //    playerInRange = false;
    //    xToSpeakText.gameObject.SetActive(false);
    //    if (inDialogue) EndDialogue();
    //}

    //LO CAMBIE POR UNA PROPIEDAD
    //public bool IsDialoguing()
    //{
    //    return inDialogue;
    //}
}