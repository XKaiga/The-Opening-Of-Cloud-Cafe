using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCoffeeManager : MonoBehaviour
{
    [SerializeField] private GameObject tasksMenu;
    [SerializeField] private TextMeshProUGUI tasksMenuTxt;
    [SerializeField] private GameObject tasksOpenMenuBtn;
    [SerializeField] private GameObject tasksCloseMenuBtn;

    private void Start() { UpdateTasks(); }

    public void OpenTasksMenu()
    {
        tasksMenu.SetActive(true);
        UpdateTasks();
        tasksOpenMenuBtn.SetActive(false);
        tasksCloseMenuBtn.SetActive(true);
    }
    public void CloseTasksMenu()
    {
        tasksMenu.SetActive(false);
        tasksOpenMenuBtn.SetActive(true);
        tasksCloseMenuBtn.SetActive(false);
    }

    private void UpdateTasks()
    {
        tasksMenuTxt.text = "";
        int taskNumber = 1;
        if (TrashDrag.readyToRemoveTrash)
        {
            tasksMenuTxt.text = taskNumber + ". Take out the trash!!\n\n";
            taskNumber++;
        }
        if (!CleanManager.clean)
            tasksMenuTxt.text += taskNumber + ". Clean table!";
    }

    public void LoadDrinkStationScene() { SceneManager.LoadScene("DrinkStation"); }
    public void LoadTablesScene() { SceneManager.LoadScene("Tables"); }
}
