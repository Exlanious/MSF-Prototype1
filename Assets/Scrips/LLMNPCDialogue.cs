using System;
using System.Collections.Generic;
using UnityEngine;

public class LLMNPCDialogue : MonoBehaviour
{
    [Serializable]
    public class DialogueGroup
    {
        public DialogueOptions dialogues;
        public List<string> dialogueLines = new List<string>();

        public const int refetchBuffer = 3;
        public const int initialize = 5;

        public int dialogueIndex = 0;
        public int activateHour = 0;
    }

    [SerializeField]
    public List<DialogueGroup> dialogues = new List<DialogueGroup>();
    public int currentDialogueGroup = 0;

    void Start()
    {
        // On start, check the current time to set the initial dialogue group
        if (TimeManager.Instance != null)
        {
            OnHourChanged(TimeManager.Instance.getTimeHours());
        }

        // Always initialize with non-AI options to prevent the crash
        foreach (DialogueGroup dialogueGroup in dialogues)
        {
            if (dialogueGroup.dialogues != null)
            {
                dialogueGroup.dialogueLines = new List<string>(dialogueGroup.dialogues.options);
            }
        }

        if (LLMCaller.Instance.getLLMProvider() == LLMProviders.Non_AI)
        {
            return;
        }
        else
        {
            generateBatchDialogues();
        }
    }

    void OnEnable()
    {
        LLMCaller.onAIOFF += forefeitAI;

        // Subscribe to the event from TimeManager
        if (TimeManager.Instance != null)
        {
            TimeManager.OnHourChanged += OnHourChanged;
        }
    }

    void OnDisable()
    {
        LLMCaller.onAIOFF -= forefeitAI;

        // Unsubscribe to prevent memory leaks
        if (TimeManager.Instance != null)
        {
            TimeManager.OnHourChanged -= OnHourChanged;
        }
    }

    private void OnHourChanged(int currentHour)
    {
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (currentHour == dialogues[i].activateHour)
            {
                currentDialogueGroup = i;

                // Re-initialize the dialogue group with non-AI options
                dialogues[currentDialogueGroup].dialogueLines = new List<string>(dialogues[currentDialogueGroup].dialogues.options);

                if (LLMCaller.Instance.getLLMProvider() != LLMProviders.Non_AI)
                {
                    generateBatchDialogues();
                }
                return;
            }
        }
    }

    private void forefeitAI()
    {
        foreach (DialogueGroup dialogue in dialogues)
        {
            if (dialogue.dialogues != null)
            {
                // Set dialogue lines to the non-AI options
                dialogue.dialogueLines = new List<string>(dialogue.dialogues.options);
            }
        }
    }

    public string getDialogue()
    {
        if (dialogues.Count == 0 || currentDialogueGroup >= dialogues.Count || dialogues[currentDialogueGroup].dialogueLines.Count == 0)
        {
            Debug.LogError("No dialogue groups or lines available.");
            return null;
        }

        DialogueGroup dialogueGroup = dialogues[currentDialogueGroup];

        if (LLMCaller.Instance.getLLMProvider() != LLMProviders.Non_AI)
        {
            if (dialogueGroup.dialogueIndex >= dialogueGroup.dialogueLines.Count - DialogueGroup.refetchBuffer)
            {

                generateBatchDialogues();
            }
        }

        string str = dialogueGroup.dialogueLines[dialogueGroup.dialogueIndex];
        dialogueGroup.dialogueIndex = (dialogueGroup.dialogueIndex + 1) % dialogueGroup.dialogueLines.Count;
        return str;
    }

    private void generateBatchDialogues()
    {
        DialogueGroup dialogueGroup = dialogues[currentDialogueGroup];
        if (LLMCaller.Instance.getLLMProvider() == LLMProviders.Non_AI || dialogueGroup.dialogues.LLMPrompt == null)
        {
            return;
        }

        const string delimiter = "/";
        string fullprompt = dialogueGroup.dialogues.LLMPrompt + "generate " + DialogueGroup.initialize + "instances of this prompt separated only by " + delimiter + ". No emojis. ";
        LLMCaller.Instance.RequestResponse(fullprompt, (response) =>
        {
            dialogueGroup.dialogueLines.Clear();

            if (response == null)
            {
                // Revert to non-AI options if the AI call fails
                dialogueGroup.dialogueLines = new List<string>(dialogueGroup.dialogues.options);
                return;
            }
            string[] parsedData = response.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            foreach (string dialogue in parsedData)
            {
                if (!string.IsNullOrEmpty(dialogue))
                {
                    dialogueGroup.dialogueLines.Add(dialogue.Trim());
                }
            }
            dialogueGroup.dialogueIndex = 0;
        });
    }
}