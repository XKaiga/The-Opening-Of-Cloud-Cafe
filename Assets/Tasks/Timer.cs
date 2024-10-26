using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Text timerText;
    public float timeRemaining = 0;
    public static int timeStart = 0;

    public static bool timerIsRunning = false;

    private void Start()
    {
        timerText.gameObject.SetActive(false);

        timeRemaining = timeStart;

        if (timeRemaining != 0)
        {
            timerText.gameObject.SetActive(true);
            timerIsRunning = true;
        }
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                UpdateTimerDisplay(timeRemaining);
            }
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        timeToDisplay = Mathf.Clamp(timeToDisplay, 0, Mathf.Infinity);

        int seconds = Mathf.FloorToInt(timeToDisplay);

        timerText.text = seconds.ToString();
    }
}
