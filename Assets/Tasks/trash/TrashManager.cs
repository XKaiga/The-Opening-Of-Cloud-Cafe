using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    [SerializeField] private Timer trashTimer;
    [SerializeField] private TableManager tableManager;

    [SerializeField] private TrashDrag trash;
    private Vector2 initTrashPos = new Vector2(-3.17f, -0.59f); 
    public static int trashMaxQty = 10;
    public static int currTrashQty = 0;

    public static int taskTimer = 10;

    private void Start()
    {
        initTrashPos = trash.transform.position;
        trash.gameObject.SetActive(false);

        ShowTrash(0);
    }

    public static void FillTrash(int quantity)
    {
        if (quantity < 0)
            return;

        currTrashQty += quantity;

        if (currTrashQty >= trashMaxQty && !TrashDrag.readyToRemoveTrash)
        {
            MainCoffeeManager.activeTasks.Add(new(TrashManager.taskTimer, TaskType.Trash));
            currTrashQty = trashMaxQty;
            TrashDrag.readyToRemoveTrash = true;
        }
    }

    public void ShowTrash(int quantity)
    {
        FillTrash(quantity);

        float showLimit = trashMaxQty * 2 / 3;
        if (currTrashQty >= showLimit)
        {
            trash.gameObject.SetActive(true);

            float qtyOfTrashShowing = currTrashQty - showLimit;

            float qtyNeededToMoveOne = (float)(trashMaxQty - showLimit) / 5;

            float needToMove = qtyOfTrashShowing / qtyNeededToMoveOne;

            trash.transform.position = new Vector3(trash.transform.position.x, trash.transform.position.y + needToMove / 10, trash.transform.position.z);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (TableManager.inAnotherView)
            return;

        if (trashTimer.timerIsRunning)
            Money.AddTaskScore();

        TrashRemoved();
    }

    private void TrashRemoved()
    {
        currTrashQty = 0;
        trash.transform.position = initTrashPos;
        trash.gameObject.SetActive(false);

        TrashDrag.readyToRemoveTrash = false;

        MainCoffeeManager.RemoveTask(TaskType.Trash);

        tableManager.RemoveTrashTimer();
    }
}
