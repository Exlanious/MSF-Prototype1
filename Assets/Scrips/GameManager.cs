using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        TimeManager.Instance.startClock();
    }
}