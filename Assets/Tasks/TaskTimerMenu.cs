using TMPro;
using UnityEngine;

public class TaskTimerMenu : MonoBehaviour
{
    public TaskType taskType;

    [SerializeField] private TextMeshProUGUI timerText;
    public float timeRemaining = 0;
    public bool timerIsRunning = false;
    private string taskText = string.Empty;

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
                MainCoffeeManager.RemoveTaskFromScene(this.gameObject, taskType);
        }
    }

    private void UpdateTimerDisplay(float timeToDisplay)
    {
        foreach (var task in MainCoffeeManager.activeTasks)
        {
            if (task.type == taskType)
            {
                int indexTimer = MainCoffeeManager.activeTasks.IndexOf(task);
                MainCoffeeManager.activeTasks[indexTimer].timer = timeToDisplay;
                break;
            }
        }

        timeToDisplay = Mathf.Clamp(timeToDisplay, 0, Mathf.Infinity);

        int seconds = Mathf.FloorToInt(timeToDisplay);

        timerText.text = taskText + seconds.ToString();
    }

    public void SaveTaskText(string taskTxt)
    {
        taskText = taskTxt;
    }
}
