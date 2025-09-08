using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private int currentTimeSeconds;
    [SerializeField] private int timeMinutes = 0;
    [SerializeField] private int timeHours = 0;
    [SerializeField] private int currentAdvanceFactor = 10;

    [SerializeField] private TextMeshProUGUI timeDisplay;

    // Define the event
    public static event Action<int> OnHourChanged;

    //Singleton
    public static TimeManager Instance;

    public void startClock()
    {
        StartCoroutine(clock());
    }

    private IEnumerator clock()
    {
        // Store the previous hour to detect changes
        int previousHour = timeHours;

        while (true)
        {
            yield return new WaitForSeconds(1);

            currentTimeSeconds += currentAdvanceFactor;

            timeMinutes = (currentTimeSeconds / 60) % 60;
            timeHours = (currentTimeSeconds / 3600) % 24;

            if (timeHours > 19)
            {
                //Jump to next day. Skip the night
                currentTimeSeconds += 42000;
            }
            // Check if the hour has changed
            if (timeHours != previousHour)
            {
                // Invoke the event, passing the new hour
                OnHourChanged?.Invoke(timeHours);
                // Update the previous hour for the next check
                previousHour = timeHours;
            }
            if (timeDisplay != null)
            {
                string formattedMinutes = timeMinutes < 10 ? "0" + timeMinutes : timeMinutes.ToString();
                string formattedHours = timeHours < 10 ? "0" + timeHours : timeHours.ToString();
                timeDisplay.text = "Current Time: " + formattedHours + ":" + formattedMinutes;
            }
        }
    }

    public int getCurrentTimeMinutes()
    {
        return timeMinutes;
    }

    public int getTimeHours()
    {
        return timeHours;
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional but often useful for singletons
        }
        else
        {
            Destroy(gameObject); // Ensures only one instance exists
        }
    }
}