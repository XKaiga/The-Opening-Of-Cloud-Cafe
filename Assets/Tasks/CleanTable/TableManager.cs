using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TableManager : MonoBehaviour
{
    private Camera mainCam;
    public static bool isChangingScene = false;

    [SerializeField] private Timer cleanTimer;
    [SerializeField] private Timer trashTimer;

    [SerializeField] private GameObject tables;
    [SerializeField] private GameObject trash;
    [SerializeField] private TrashDrag trashDrag;

    [SerializeField] private GameObject tableBg;
    [SerializeField] private GameObject dirtyTable;
    [SerializeField] private GameObject cleanTable;

    private string dirtyTableName = string.Empty;

    private void Awake()
    {
        Debug.Log("trashMaxQty: " + TrashManager.trashMaxQty);
        mainCam = Camera.main;

        //foreach (var item in MainCoffeeManager.activeTasks)
        //    Debug.Log(item.type + ": " + item.timer + "\n");
    }

    private void Start()
    {
        isChangingScene = false;

        tables.SetActive(true);
        trash.SetActive(true);
        tableBg.SetActive(false);
        dirtyTable.SetActive(false);
        cleanTable.SetActive(false);

        if (TrashDrag.readyToRemoveTrash)
        {
            float trashStartTime = MainCoffeeManager.activeTasks.Find(t => t.type == TaskType.Trash).timer;
            trashTimer.StartTimer(trashStartTime);
        }

        if (CleanManager.clean)
            return;

        int rndTable = Random.Range(0, 2);
        switch (rndTable)
        {
            case 0:
                dirtyTableName = "left";
                break;
            case 1:
                dirtyTableName = "center";
                break;
            case 2:
                dirtyTableName = "right";
                break;
        }

        float cleanStartTime = MainCoffeeManager.activeTasks.Find(t => t.type == TaskType.Clean).timer;
        cleanTimer.StartTimer(cleanStartTime);
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        Collider2D collider = rayHit.collider;
        string colliderName = collider.name.ToLower();

        if (colliderName.Contains("backbtn"))
        {
            if (colliderName.Contains("tables"))
            {
                if (!CleanManager.clean)
                {
                    Task cleanTaskFound = MainCoffeeManager.activeTasks.Find(task => cleanTimer.gameObject.name.ToLower().Contains(task.type.ToString().ToLower()));
                    if (cleanTaskFound != null) 
                        cleanTaskFound.timer = float.Parse(cleanTimer.gameObject.GetComponentInChildren<Text>().text);
                }
                if (TrashDrag.readyToRemoveTrash)
                {
                    Task trashTaskFound = MainCoffeeManager.activeTasks.Find(task => trashTimer.gameObject.name.ToLower().Contains(task.type.ToString().ToLower()));
                    if (trashTaskFound != null)
                        trashTaskFound.timer = float.Parse(trashTimer.gameObject.GetComponentInChildren<Text>().text);
                }

                isChangingScene = true;
                Debug.Log("Changing Scenes");
                SceneManager.LoadScene("Dialogue");
            }
            else
            {
                tables.SetActive(true);
                trash.SetActive(true);
                tableBg.SetActive(false);
                dirtyTable.SetActive(false);
                cleanTable.SetActive(false);

                if (TrashDrag.readyToRemoveTrash)
                {
                    float trashStartTime = MainCoffeeManager.activeTasks.Find(t => t.type == TaskType.Trash).timer;
                    trashTimer.StartTimer(trashStartTime);
                }

                //if (TrashDrag.readyToRemoveTrash && CleanManager.clean && Timer.timeStart != (CleanManager.taskTimer + TrashManager.taskTimer))
                //    Timer.timeStart = TrashManager.taskTimer;
            }

            return;
        }

        if (colliderName.Contains("table"))
        {
            tables.SetActive(false);
            trash.SetActive(false);
            tableBg.SetActive(true);

            if (colliderName.Contains(dirtyTableName))
                dirtyTable.SetActive(true);
            else
                cleanTable.SetActive(true);

            return;
        }
    }

    public void RemoveTrashTimer()
    {
        trashTimer.StopTimer();
    }
}
