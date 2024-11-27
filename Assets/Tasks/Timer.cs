using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private GameObject timerTextGameObject;
    private Text timerText;
    private TaskType taskType;

    public float timeRemaining = 0;
    public bool timerIsRunning = false;

    private void Awake()
    {
        timerText = timerTextGameObject.GetComponent<Text>();
        timerTextGameObject.SetActive(false);
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
                StopTimer();
        }
    }

    public void StartTimer(float seconds)
    {
        if (timerIsRunning)
            return;

        timeRemaining = seconds;
        timerTextGameObject.SetActive(true);
        timerIsRunning = true;
    }

    public void StopTimer(float stopAt = 0)
    {
        timeRemaining = stopAt;
        timerIsRunning = false;
        UpdateTimerDisplay(timeRemaining);

        MainCoffeeManager.RemoveTask(taskType);
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        timeToDisplay = Mathf.Clamp(timeToDisplay, 0, Mathf.Infinity);

        int seconds = Mathf.FloorToInt(timeToDisplay);

        timerText.text = seconds.ToString();

        if (!timerIsRunning)
            StartCoroutine(HideTimerAfterDelay());
    }

    private IEnumerator HideTimerAfterDelay()
    {
        yield return new WaitForSeconds(2);

        timerTextGameObject.SetActive(false);
        timeRemaining = 0;
    }
}
