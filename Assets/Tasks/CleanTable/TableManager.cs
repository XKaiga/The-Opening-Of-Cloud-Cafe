using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TableManager : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private GameObject tables;
    [SerializeField] private GameObject trash;
    [SerializeField] private TrashDrag trashDrag;

    [SerializeField] private GameObject tableBg;
    [SerializeField] private GameObject dirtyTable;
    [SerializeField] private GameObject cleanTable;

    private string dirtytTableName = string.Empty;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        tables.SetActive(true);
        trash.SetActive(true);
        tableBg.SetActive(false);
        dirtyTable.SetActive(false);
        cleanTable.SetActive(false);

        if (TrashDrag.readyToRemoveTrash)
            Timer.timeStart = TrashManager.taskTimer;

        if (CleanManager.clean)
            return;
        Timer.timeStart += CleanManager.taskTimer;
        int rndTable = Random.Range(0, 2);
        switch (rndTable)
        {
            case 0:
                dirtytTableName = "left";
                break;
            case 1:
                dirtytTableName = "center";
                break;
            case 2:
                dirtytTableName = "right";
                break;
        }

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
                SceneManager.LoadScene("Dialogue");
            else
            {
                tables.SetActive(true);
                trash.SetActive(true);
                tableBg.SetActive(false);
                dirtyTable.SetActive(false);
                cleanTable.SetActive(false);

                if (TrashDrag.readyToRemoveTrash && CleanManager.clean && Timer.timeStart != (CleanManager.taskTimer + TrashManager.taskTimer))
                    Timer.timeStart = TrashManager.taskTimer;
            }

            return;
        }

        if (colliderName.Contains("table"))
        {
            tables.SetActive(false);
            trash.SetActive(false);
            tableBg.SetActive(true);
            
            if (colliderName.Contains(dirtytTableName))
                dirtyTable.SetActive(true);
            else
                cleanTable.SetActive(true);
            
            return;
        }
    }
}
