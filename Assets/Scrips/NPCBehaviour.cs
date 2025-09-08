using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public LLMNPCDialogue manager;
    private bool hasInteracted = false;
    public ParticleSystem particleGenerator;
    public AudioSource audioSource;

    [SerializeField] private const int enterHour = 10;
    [SerializeField] private const int leaveHour = 19;

    void Start()
    {
        dialogueText.text = "";
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasInteracted) { return; }
        hasInteracted = true;
        Debug.Log("Player has entered the NPC's trigger area.");
        if (collision.CompareTag("Player"))
        {
            if (manager != null) { dialogueText.text = manager.getDialogue(); }

        }
        audioSource.Play();

        StartCoroutine(ClearDialogueAfterDelay(5f)); // Clear dialogue after 5 seconds

    }

    private IEnumerator ClearDialogueAfterDelay(float v)
    {
        yield return new WaitForSeconds(v);
        dialogueText.text = "";
        hasInteracted = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        dialogueText.text = "";
        hasInteracted = false;
    }

    void OnEnable()
    {
        particleGenerator.Play();
    }
}
