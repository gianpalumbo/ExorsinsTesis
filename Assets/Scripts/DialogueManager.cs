using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueBlock
{
    public DialogueLine[] lines;   // Unity serializa bien arrays de structs
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    //public List<List<DialogueLine>> allDialogues = new List<List<DialogueLine>>();

    [Header("Todos los bloques de diálogo")]
    public List<DialogueBlock> allDialogues = new List<DialogueBlock>();  // Ahora es una lista de wrappers, lista de arrays de structs

    [Header("Referencia al sistema de UI")]
    public DialogueSystem dialogueSystem; //dialogue system del dialogue manager

    [SerializeField]private int maxUnlockedBlocks = 0;
    [SerializeField]private int currentBlock = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            //return;
        }
    }

    /// <summary>
    /// Llamar al entrar en cada santuario para desbloquear un nuevo bloque.
    /// </summary>
    public void UnlockNextBlock()
    {
        if (maxUnlockedBlocks < allDialogues.Count) maxUnlockedBlocks++;
    }

    /// <summary>
    /// Intenta reproducir el siguiente bloque de diálogo desbloqueado.
    /// </summary>
    public void TryPlayDialogue()
    {
        if (maxUnlockedBlocks == 0) return;

        // Asegura que currentBlock esté dentro del rango [0, maxUnlockedBlocks-1]
        currentBlock = Mathf.Clamp(currentBlock, 0, maxUnlockedBlocks - 1);

        // Asigna al DialogueSystem el array de líneas del bloque actual
        dialogueSystem.lines = allDialogues[currentBlock].lines;
        dialogueSystem.StartDialogue();

        // Prepara el siguiente bloque para la próxima vez
        if (currentBlock < maxUnlockedBlocks) currentBlock++;
    }
}
