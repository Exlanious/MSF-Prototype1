using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<GameObject> NPCS;
    public int enableHour = 10;
    public int disableHour = 19;
    public AudioClip enterAudio;
    public AudioClip exitAudio;
    public AudioSource audioPlayer;

    private void OnEnable()
    {
        TimeManager.OnHourChanged += Enable;
        TimeManager.OnHourChanged += Disable;
    }

    private void OnDisable()
    {
        TimeManager.OnHourChanged -= Enable;
        TimeManager.OnHourChanged -= Disable;
    }

    private void Enable(int hour)
    {
        if (hour == enableHour)
        {
            foreach (GameObject NPC in NPCS)
            {
                NPC.SetActive(true);
            }
            audioPlayer.clip = enterAudio;
            audioPlayer.Play();
        }

    }

    private void Disable(int hour)
    {
        if (hour == disableHour)
        {
            foreach (GameObject NPC in NPCS)
            {
                NPC.SetActive(false);
            }
            audioPlayer.clip = exitAudio;
            audioPlayer.Play();
        }
    }
}